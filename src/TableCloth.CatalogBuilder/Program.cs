using SixLabors.ImageSharp.Formats.Png;
using System.IO.Compression;
using System.Xml.Schema;
using System.Xml;
using BitMiracle.LibTiff.Classic;
using System.Data.SqlTypes;
using System.Reflection.PortableExecutable;

AppMain(args);

static void AppMain(string[] args)
{
    var sourceDirectory = args.ElementAtOrDefault(0);
    var targetDirectory = args.ElementAtOrDefault(1);

    if (string.IsNullOrWhiteSpace(sourceDirectory) ||
        !Directory.Exists(sourceDirectory))
    {
        Console.Error.WriteLine("Please specify source directory to run this tool.");
        Environment.Exit(1);
        return;
    }

    if (string.IsNullOrWhiteSpace(targetDirectory))
    {
        Console.Error.WriteLine("Please specify target directory to run this tool.");
        Environment.Exit(1);
        return;
    }

    try
    {
        Console.Out.WriteLine($"Source directory path: `{sourceDirectory}`");
        Console.Out.WriteLine($"Target directory path: `{targetDirectory}`");

        PrepareOutputDirectory(sourceDirectory, targetDirectory);
        ConvertSiteLogoImagesIntoIconFiles(targetDirectory);
        CreateImageResourceZipFile(targetDirectory);
        ValidatingCatalogSchemaFile(targetDirectory);

        Console.Out.WriteLine("Catalog builder runs with succeed result.");
        Environment.Exit(0);
    }
    catch (Exception thrownException)
    {
        Console.Error.WriteLine($"Error: {thrownException}");
        Environment.Exit(2);
    }
}

static string? GetPositionInfo(IXmlLineInfo? lineInfo)
{
    var positionText = "Unknown Position";

    if (lineInfo != null)
        positionText = $"Line: {lineInfo.LineNumber}, Line Position: {lineInfo.LinePosition}";

    return positionText;
}

const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.3";

static void ValidatingCatalogSchemaFile(string targetDirectory)
{
    var catalogXmlPath = Path.Combine(targetDirectory, "Catalog.xml");
    var schemaXsdPath = Path.Combine(targetDirectory, "Catalog.xsd");

    var config = new XmlReaderSettings()
    {
        ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings,
    };

    Console.Out.WriteLine($"Info: Using schema `{schemaXsdPath}`.");
    config.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
    config.ValidationType = ValidationType.Schema;
    config.Schemas.Add(null, schemaXsdPath);

    var warningCount = 0;
    var errorCount = 0;

    config.ValidationEventHandler += new ValidationEventHandler((_sender, _e) =>
    {
        if (_e.Severity == XmlSeverityType.Warning)
        {
            warningCount++;
            Console.Out.WriteLine($"Warning: {_e.Message} - {GetPositionInfo(_sender as IXmlLineInfo)}");
        }
        else if (_e.Severity == XmlSeverityType.Error)
        {
            errorCount++;
            Console.Error.WriteLine($"Error: {_e.Message} - {GetPositionInfo(_sender as IXmlLineInfo)}");
        }
    });

    var httpClient = new HttpClient()
    {
        Timeout = TimeSpan.FromSeconds(5d),
    };

    if (!string.IsNullOrWhiteSpace(userAgent))
    {
        Console.Out.WriteLine($"Info: Using User Agent String `{userAgent}`.");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
    }

    Console.Out.WriteLine($"Info: Validating `{catalogXmlPath}` file.");
    var testTargets = new Dictionary<string, Tuple<string, int>>();

    // Get the XmlReader object with the configured settings.
    using (XmlReader reader = XmlReader.Create(catalogXmlPath, config))
    {
        // Parsing the file will cause the validation to occur.
        while (reader.Read())
        {
            try
            {
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                switch (reader.Name)
                {
                    case "Companion":
                    case "Service":
                    case "Package":
                        var urlString = reader.GetAttribute("Url", null);

                        if (string.IsNullOrWhiteSpace(urlString))
                        {
                            errorCount++;
                            Console.Error.WriteLine($"Error: [{reader.Name}] URL string cannot be empty - {GetPositionInfo(reader as IXmlLineInfo)}");
                            continue;
                        }

                        if (!Uri.TryCreate(urlString, UriKind.Absolute, out Uri? result) ||
                            result == null)
                        {
                            errorCount++;
                            Console.Error.WriteLine($"Error: Cannot parse the URI `{urlString}` - {GetPositionInfo(reader as IXmlLineInfo)}");
                            continue;
                        }

                        if (testTargets.ContainsKey(urlString))
                        {
                            Console.Out.WriteLine($"Skipping visited `{reader.Name}` URL `{urlString}`...");
                            continue;
                        }

                        var lineInfo = reader as IXmlLineInfo;
                        testTargets[urlString] = new Tuple<string, int>(reader.Name, lineInfo?.LineNumber ?? 0);
                        break;
                }
            }
            catch (AggregateException aex)
            {
                errorCount++;
                var ex = aex.InnerException ?? aex;
                Console.Error.WriteLine($"Error: {ex.GetType().Name} / {ex.InnerException?.GetType().Name} throwed. ({ex.InnerException?.Message ?? ex.Message}) - {GetPositionInfo(reader as IXmlLineInfo)}");
                continue;
            }
            catch (Exception ex)
            {
                errorCount++;
                Console.Error.WriteLine($"Error: {ex.GetType().Name} / {ex.InnerException?.GetType().Name} throwed. ({ex.InnerException?.Message ?? ex.Message}) - {GetPositionInfo(reader as IXmlLineInfo)}");
                continue;
            }
        }
    }

    foreach (var eachTestTarget in testTargets.AsParallel())
    {
        var urlString = eachTestTarget.Key;

        try
        {
            var response = httpClient.GetAsync(urlString, HttpCompletionOption.ResponseHeadersRead).Result;

            if (response == null || !response.IsSuccessStatusCode)
            {
                errorCount++;
                Console.Out.WriteLine($"Warning: Cannot fetch the URI `{urlString}` (Remote response code: {(int?)response?.StatusCode}) - (Line: `{eachTestTarget.Value.Item2}`)");
            }
        }
        catch (AggregateException aex)
        {
            errorCount++;
            var ex = aex.InnerException ?? aex;
            Console.Out.WriteLine($"Warning: Cannot fetch the URI `{urlString}` ({ex.GetType().Name} throwed. {ex.InnerException?.Message ?? ex.Message}) - (Line: `{eachTestTarget.Value.Item2}`)");
            continue;
        }
        catch (Exception ex)
        {
            errorCount++;
            Console.Out.WriteLine($"Warning: Cannot fetch the URI `{urlString}` ({ex.GetType().Name} throwed. {ex.InnerException?.Message ?? ex.Message}) - (Line: `{eachTestTarget.Value.Item2}`)");
            continue;
        }
    }

    if (errorCount + warningCount < 1)
        Console.Out.WriteLine("Success: No XML warnings or errors found.");
    else if (errorCount < 1 && warningCount > 0)
        Console.Out.WriteLine("Warning: Some XML warnings found.");
    else
        Console.Error.WriteLine("Error: One or more XML errors or warnings found.");
}

static void CreateImageResourceZipFile(string targetDirectory)
{
    var imageDirectory = Path.Combine(targetDirectory, "images");
    Console.Out.WriteLine($"Investigating directory `{imageDirectory}`...");

    var pngFiles = Directory.GetFiles(imageDirectory, "*.png", SearchOption.AllDirectories);
    var icoFiles = Directory.GetFiles(imageDirectory, "*.ico", SearchOption.AllDirectories);
    var sourceFiles = Enumerable.Concat(pngFiles, icoFiles).ToArray();
    Console.Out.WriteLine($"Found total {sourceFiles.Length} source files.");

    var workingDirectory = Path.Combine(targetDirectory, "working");
    if (!Directory.Exists(workingDirectory))
    {
        Console.Out.WriteLine($"Creating working directory `{workingDirectory}`...");
        Directory.CreateDirectory(workingDirectory);
    }

    foreach (var eachResourceFile in sourceFiles)
    {
        var destFilePath = Path.Combine(workingDirectory, Path.GetFileName(eachResourceFile));
        
        if (File.Exists(destFilePath))
            Console.Out.WriteLine($"Warning: Duplicated file found. Source: `{eachResourceFile}`.");

        Console.Out.WriteLine($"Copying `{eachResourceFile}` file to `{destFilePath}` file...");
        File.Copy(eachResourceFile, destFilePath, true);
    }

    var zipFilePath = Path.Combine(targetDirectory, "Images.zip");
    Console.Out.WriteLine($"Creating `{zipFilePath}` zip file...");
    ZipFile.CreateFromDirectory(workingDirectory, zipFilePath, CompressionLevel.Optimal, false);

    Console.Out.WriteLine($"Removing working directory `{workingDirectory}`...");
    Directory.Delete(workingDirectory, true);
}

static void ConvertSiteLogoImagesIntoIconFiles(string targetDirectory)
{
    var imageDirectory = Path.Combine(targetDirectory, "images");
    Console.Out.WriteLine($"Investigating directory `{imageDirectory}`...");

    var pngFiles = Directory.GetFiles(imageDirectory, "*.png", SearchOption.AllDirectories);
    Console.Out.WriteLine($"Found {pngFiles.Length} png files in `{imageDirectory}` directory.");

    foreach (var eachPngFile in pngFiles)
    {
        var directoryPath = Path.GetDirectoryName(eachPngFile);

        if (string.IsNullOrEmpty(directoryPath))
        {
            Console.Error.WriteLine($"Cannot obtain directory path of `{eachPngFile}` file.");
            continue;
        }    

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(eachPngFile);
        var iconFilePath = Path.Combine(directoryPath, $"{fileNameWithoutExtension}.ico");

        Console.Out.WriteLine($"Converting `{eachPngFile}` image file into `{iconFilePath}` Win32 icon file.");
        ConvertImageToIcon(eachPngFile, iconFilePath);
    }
}

// https://stackoverflow.com/questions/21387391/how-to-convert-an-image-to-an-icon-without-losing-transparency
static void ConvertImageToIcon(string sourceImageFilePath, string targetIconFilePath)
{
    using (var outputFile = File.OpenWrite(targetIconFilePath))
    using (var outputWriter = new BinaryWriter(outputFile))
    using (var inputFile = File.OpenRead(sourceImageFilePath))
    using (var inputImage = Image.Load(inputFile))
    {
        // Header
        outputWriter.Write((short)0);   // 0 : reserved
        outputWriter.Write((short)1);   // 2 : 1=ico, 2=cur
        outputWriter.Write((short)1);   // 4 : number of images

        // Image directory
        var w = inputImage.Width;
        if (w >= 256) w = 0;
        outputWriter.Write((byte)w);    // 0 : width of image

        var h = inputImage.Height;
        if (h >= 256) h = 0;
        outputWriter.Write((byte)h);    // 1 : height of image

        outputWriter.Write((byte)0);    // 2 : number of colors in palette
        outputWriter.Write((byte)0);    // 3 : reserved
        outputWriter.Write((short)0);   // 4 : number of color planes
        outputWriter.Write((short)0);   // 6 : bits per pixel

        var sizeHere = outputFile.Position;
        outputWriter.Write(0);     // 8 : image size

        var start = (int)outputFile.Position + 4;
        outputWriter.Write(start);      // 12: offset of image data

        // Image data
        inputImage.Save(outputFile, PngFormat.Instance);
        var imageSize = (int)outputFile.Position - start;
        outputFile.Seek(sizeHere, SeekOrigin.Begin);
        outputWriter.Write(imageSize);
        outputFile.Seek(0L, SeekOrigin.Begin);
    }
}

static void PrepareOutputDirectory(string sourceDirectory, string targetDirectory)
{
    if (Directory.Exists(targetDirectory))
    {
        Console.Out.WriteLine($"Removing output directory `{targetDirectory}`...");
        Directory.Delete(targetDirectory, true);
    }

    Console.Out.WriteLine($"Copying all sub-items in `{sourceDirectory}` directory to `{targetDirectory}` directory...");
    CopyAll(sourceDirectory, targetDirectory, true);
}

static void CopyAll(string sourceDirectory, string targetDirectory, bool overwrite)
{
    if (!Directory.Exists(sourceDirectory))
        return;

    if (!Directory.Exists(targetDirectory))
    {
        Console.Out.WriteLine($"Creating `{targetDirectory}` directory...");
        Directory.CreateDirectory(targetDirectory);
    }

    foreach (var filePath in Directory.GetFiles(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly))
    {
        var fileName = Path.GetFileName(filePath);
        var destFile = Path.Combine(targetDirectory, fileName);
        Console.Out.WriteLine($"Copying `{filePath}` file into `{destFile}` file...");
        File.Copy(filePath, destFile, overwrite);
    }

    foreach (var directoryPath in Directory.GetDirectories(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly))
    {
        var directoryName = Path.GetFileName(directoryPath);
        var destDirectory = Path.Combine(targetDirectory, directoryName);
        Console.Out.WriteLine($"Copying `{directoryName}` directories into `{destDirectory}` directory...");
        CopyAll(directoryPath, destDirectory, overwrite);
    }
}
