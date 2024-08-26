using SixLabors.ImageSharp.Formats.Png;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

await AppMainAsync(args, default);

static async Task AppMainAsync(string[] args, CancellationToken cancellationToken = default)
{
    var sourceDirectory = args.ElementAtOrDefault(0);
    var targetDirectory = args.ElementAtOrDefault(1);

    if (string.IsNullOrWhiteSpace(sourceDirectory) ||
        !Directory.Exists(sourceDirectory))
    {
        Console.Error.WriteLine("Error: Please specify source directory to run this tool.");
        Environment.Exit(1);
        return;
    }

    if (string.IsNullOrWhiteSpace(targetDirectory))
    {
        Console.Error.WriteLine("Error: Please specify target directory to run this tool.");
        Environment.Exit(1);
        return;
    }

    try
    {
        Console.Out.WriteLine($"Info: Source directory path: `{sourceDirectory}`");
        Console.Out.WriteLine($"Info: Target directory path: `{targetDirectory}`");

        using var logWriter = PrepareOutputDirectory(sourceDirectory, targetDirectory);
        var catalog = await LoadCatalogXmlFile(targetDirectory, cancellationToken).ConfigureAwait(false);

        ConvertSiteLogoImagesIntoIconFiles(catalog, logWriter, targetDirectory);
        CreateImageResourceZipFile(catalog, logWriter, targetDirectory);
        GenerateWin32RCFile(catalog, logWriter, targetDirectory);
        ValidatingCatalogSchemaFile(logWriter, targetDirectory, true);

        logWriter.WriteLine(Console.Out, "Info: Catalog builder runs with succeed result.");
        logWriter.Flush();
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

const string userAgent = "curl/8.4.0";

static async Task<bool> TestUrlAsync(ConcurrentQueue<string> errorLogBuffer, HttpClient httpClient, string urlString, string element, int lineNumber, CancellationToken cancellationToken = default)
{
    var message = string.Empty;

    try
    {
        var response = await httpClient.GetAsync(urlString, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

        if (response == null || !response.IsSuccessStatusCode)
        {
            message = $"Warning: Cannot fetch the URI `{urlString}` (Remote response code: {(int?)response?.StatusCode}) - (Line: `{lineNumber}`)";
            await Console.Out.WriteLineAsync(message.AsMemory(), cancellationToken).ConfigureAwait(false);
            errorLogBuffer.Enqueue(message);
            return false;
        }
    }
    catch (AggregateException aex)
    {
        var ex = aex.InnerException ?? aex;
        message = $"Warning: Cannot fetch the URI `{urlString}` ({ex.GetType().Name} throwed. {ex.InnerException?.Message ?? ex.Message}) - (Line: `{lineNumber}`)";
        await Console.Out.WriteLineAsync(message.AsMemory(), cancellationToken).ConfigureAwait(false);
        errorLogBuffer.Enqueue(message);
        return false;
    }
    catch (Exception ex)
    {
        message = $"Warning: Cannot fetch the URI `{urlString}` ({ex.GetType().Name} throwed. {ex.InnerException?.Message ?? ex.Message}) - (Line: `{lineNumber}`)";
        await Console.Out.WriteLineAsync(message.AsMemory(), cancellationToken).ConfigureAwait(false);
        errorLogBuffer.Enqueue(message);
        return false;
    }

    message = $"Info: Test succeed on `{urlString}`.";
    await Console.Out.WriteLineAsync(message.AsMemory(), cancellationToken).ConfigureAwait(false);
    return true;
}

static void ValidatingCatalogSchemaFile(TextWriter logWriter, string targetDirectory, bool evaluateUrls)
{
    var catalogXmlPath = Path.Combine(targetDirectory, "Catalog.xml");
    var schemaXsdPath = Path.Combine(targetDirectory, "Catalog.xsd");

    var config = new XmlReaderSettings()
    {
        ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings,
    };

    logWriter.WriteLine(Console.Out, $"Info: Using schema `{schemaXsdPath}`.");
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
            logWriter.WriteLine(Console.Out, $"Warning: {_e.Message} - {GetPositionInfo(_sender as IXmlLineInfo)}");
        }
        else if (_e.Severity == XmlSeverityType.Error)
        {
            errorCount++;
            logWriter.WriteLine(Console.Error, $"Error: {_e.Message} - {GetPositionInfo(_sender as IXmlLineInfo)}");
        }
    });

    var httpClient = new HttpClient();

    if (!string.IsNullOrWhiteSpace(userAgent))
    {
        logWriter.WriteLine(Console.Out, $"Info: Using User Agent String `{userAgent}`.");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
    }

    logWriter.WriteLine(Console.Out, $"Info: Validating `{catalogXmlPath}` file.");
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
                            logWriter.WriteLine(Console.Error, $"Error: [{reader.Name}] URL string cannot be empty - {GetPositionInfo(reader as IXmlLineInfo)}");
                            continue;
                        }

                        if (!Uri.TryCreate(urlString, UriKind.Absolute, out Uri? result) ||
                            result == null)
                        {
                            errorCount++;
                            logWriter.WriteLine(Console.Error, $"Error: Cannot parse the URI `{urlString}` - {GetPositionInfo(reader as IXmlLineInfo)}");
                            continue;
                        }

                        if (testTargets.ContainsKey(urlString))
                            continue;

                        var lineInfo = reader as IXmlLineInfo;
                        testTargets[urlString] = new Tuple<string, int>(reader.Name, lineInfo?.LineNumber ?? 0);
                        break;
                }
            }
            catch (AggregateException aex)
            {
                errorCount++;
                var ex = aex.InnerException ?? aex;
                logWriter.WriteLine(Console.Error, $"Error: {ex.GetType().Name} / {ex.InnerException?.GetType().Name} throwed. ({ex.InnerException?.Message ?? ex.Message}) - {GetPositionInfo(reader as IXmlLineInfo)}");
                continue;
            }
            catch (Exception ex)
            {
                errorCount++;
                logWriter.WriteLine(Console.Error, $"Error: {ex.GetType().Name} / {ex.InnerException?.GetType().Name} throwed. ({ex.InnerException?.Message ?? ex.Message}) - {GetPositionInfo(reader as IXmlLineInfo)}");
                continue;
            }
        }
    }

    if (evaluateUrls)
    {
        var errorLogBuffer = new ConcurrentQueue<string>();
        var tasks = new List<Task<bool>>(testTargets.Count);

        foreach (var eachTestTarget in testTargets.AsParallel())
            tasks.Add(TestUrlAsync(errorLogBuffer, httpClient, eachTestTarget.Key, eachTestTarget.Value.Item1, eachTestTarget.Value.Item2));

        var results = Task.WhenAll(tasks).Result;
        var succeedCount = results.Count(x => x == true);
        var failedCount = results.Count(x => x == false);
        errorCount += failedCount;

        foreach (var eachErrorLog in errorLogBuffer.ToList())
            logWriter.WriteLine(Console.Error, eachErrorLog);

        logWriter.WriteLine(Console.Out, $"Info: Test runner completed. (Scheduled: {testTargets.Count} / Returned: {results.Length} / Succed: {succeedCount} / Failed: {failedCount})");

        if (testTargets.Count != results.Length)
            logWriter.WriteLine(Console.Out, $"Warning: Some test targets skipped due to task cancellation.");
    }

    if (errorCount + warningCount < 1)
        logWriter.WriteLine(Console.Out, "Success: No XML warnings or errors found.");
    else if (errorCount < 1 && warningCount > 0)
        logWriter.WriteLine(Console.Out, "Warning: Some XML warnings found.");
    else
        logWriter.WriteLine(Console.Error, "Error: One or more XML errors or warnings found.");
}

static void CreateImageResourceZipFile(XDocument catalog, TextWriter logWriter, string targetDirectory)
{
    var imageDirectory = Path.Combine(targetDirectory, "images");
    logWriter.WriteLine(Console.Out, $"Info: Investigating directory `{imageDirectory}`...");

    var pngFiles = Directory.GetFiles(imageDirectory, "*.png", SearchOption.AllDirectories);
    var icoFiles = Directory.GetFiles(imageDirectory, "*.ico", SearchOption.AllDirectories);
    var sourceFiles = Enumerable.Concat(pngFiles, icoFiles).ToArray();
    logWriter.WriteLine(Console.Out, $"Info: Found total {sourceFiles.Length} source files.");

    var workingDirectory = Path.Combine(targetDirectory, "working");
    if (!Directory.Exists(workingDirectory))
    {
        logWriter.WriteLine(Console.Out, $"Info: Creating working directory `{workingDirectory}`...");
        Directory.CreateDirectory(workingDirectory);
    }

    foreach (var eachResourceFile in sourceFiles)
    {
        var destFilePath = Path.Combine(workingDirectory, Path.GetFileName(eachResourceFile));
        
        if (File.Exists(destFilePath))
            logWriter.WriteLine(Console.Out, $"Warning: Duplicated file found. Source: `{eachResourceFile}`.");

        logWriter.WriteLine(Console.Out, $"Info: Copying `{eachResourceFile}` file to `{destFilePath}` file...");
        File.Copy(eachResourceFile, destFilePath, true);
    }

    var zipFilePath = Path.Combine(targetDirectory, "Images.zip");
    logWriter.WriteLine(Console.Out, $"Info: Creating `{zipFilePath}` zip file...");
    ZipFile.CreateFromDirectory(workingDirectory, zipFilePath, CompressionLevel.Optimal, false);

    logWriter.WriteLine(Console.Out, $"Info: Removing working directory `{workingDirectory}`...");
    Directory.Delete(workingDirectory, true);
}

static async Task<XDocument> LoadCatalogXmlFile(string targetDirectory, CancellationToken cancellationToken = default)
{
    var catalogXmlPath = Path.Combine(targetDirectory, "Catalog.xml");
    using var fileStream = File.OpenRead(catalogXmlPath);
    var catalogXml = await XDocument.LoadAsync(
        fileStream, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo, cancellationToken).ConfigureAwait(false);
    return catalogXml;
}

static void ConvertSiteLogoImagesIntoIconFiles(XDocument catalog, TextWriter logWriter, string targetDirectory)
{
    var imageDirectory = Path.Combine(targetDirectory, "images");
    logWriter.WriteLine(Console.Out, $"Info: Investigating directory `{imageDirectory}`...");

    var pngFiles = Directory.GetFiles(imageDirectory, "*.png", SearchOption.AllDirectories);
    logWriter.WriteLine(Console.Out, $"Info: Found {pngFiles.Length} png files in `{imageDirectory}` directory.");

    foreach (var eachPngFile in pngFiles)
    {
        var directoryPath = Path.GetDirectoryName(eachPngFile);

        if (string.IsNullOrEmpty(directoryPath))
        {
            logWriter.WriteLine(Console.Error, $"Info: Cannot obtain directory path of `{eachPngFile}` file.");
            continue;
        }    

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(eachPngFile);
        var iconFilePath = Path.Combine(directoryPath, $"{fileNameWithoutExtension}.ico");

        logWriter.WriteLine(Console.Out, $"Info: Converting `{eachPngFile}` image file into `{iconFilePath}` Win32 icon file.");
        ConvertImageToIcon(eachPngFile, iconFilePath);
    }
}

static void GenerateWin32RCFile(XDocument catalog, TextWriter logWriter, string targetDirectory)
{
    var imageDirectory = Path.Combine(targetDirectory, "images");
    logWriter.WriteLine(Console.Out, $"Info: Investigating directory `{imageDirectory}`...");

    var icoFiles = Directory.GetFiles(imageDirectory, "*.ico", SearchOption.AllDirectories);
    logWriter.WriteLine(Console.Out, $"Info: Found {icoFiles.Length} ico files in `{imageDirectory}` directory.");

    var rcFilePath = Path.Combine(targetDirectory, "Catalog.rc");
    logWriter.WriteLine(Console.Out, $"Info: Creating `{rcFilePath}` Win32 resource file...");

    using (var rcWriter = new StreamWriter(File.OpenWrite(rcFilePath), Encoding.ASCII))
    {
        rcWriter.WriteLine(
            """
            #include <winver.h>

            """);

        for (int i = 0, count = 0; i < icoFiles.Length; i++)
        {
            rcWriter.WriteLine($"#define ICON_{(i + 1)} {++count}");
            rcWriter.WriteLine($"#define ID_{(i + 1)} {++count}");
            rcWriter.WriteLine($"#define URL_{(i + 1)} {++count}");
        }

        for (int i = 0; i < icoFiles.Length; i++)
        {
            var eachIcoFile = icoFiles[i];
            var identifier = Path.GetFileNameWithoutExtension(eachIcoFile)!.ToUpperInvariant();
            var relativePath = Path.GetRelativePath(targetDirectory, eachIcoFile).Replace('\\', '/');
            rcWriter.WriteLine($"ICON_{(i + 1)} ICON \"{relativePath}\"");
        }

        var now = DateTime.UtcNow;
        var major = now.Year;
        var minor = now.Month;
        var build = now.Day;
        var rev = now.Hour * 60 + now.Minute;

        rcWriter.WriteLine(
            $$"""

            VS_VERSION_INFO VERSIONINFO
            FILEVERSION {{major}},{{minor}},{{build}},{{rev}}
            PRODUCTVERSION {{major}},{{minor}},{{build}},{{rev}}
            FILEFLAGSMASK 0x3fL
            FILEFLAGS 0x0L
            FILEOS 0x4L
            FILETYPE 0x2L
            FILESUBTYPE 0x0L
            BEGIN
                BLOCK "StringFileInfo"
                BEGIN
                    BLOCK "040904b0"
                    BEGIN
                        VALUE "CompanyName", "rkttu.com\0"
                        VALUE "FileDescription", "Catalog Icon Library for Win32 applications\0"
                        VALUE "FileVersion", "{{major}},{{minor}},{{build}},{{rev}}\0"
                        VALUE "InternalName", "Catalog.dll\0"
                        VALUE "LegalCopyright", "(c) rkttu.com, All rights reserved.\0"
                        VALUE "OriginalFilename", "Catalog.dll\0"
                        VALUE "ProductName", "TableCloth\0"
                        VALUE "ProductVersion", "{{major}},{{minor}},{{build}},{{rev}}\0"
                    END
                END
                BLOCK "VarFileInfo"
                BEGIN
                    VALUE "Translation", 0x409, 1200
                END
            END

            """);

        rcWriter.WriteLine("STRINGTABLE");
        rcWriter.WriteLine("BEGIN");
        for (int i = 0; i < icoFiles.Length; i++)
        {
            var eachIcoFile = icoFiles[i];
            var identifier = Path.GetFileNameWithoutExtension(eachIcoFile)!.ToUpperInvariant();

            rcWriter.WriteLine($"    ID_{(i + 1)} \"{identifier}\"");

            var targetElement = catalog.XPathSelectElement($"/TableClothCatalog/InternetServices/Service[translate(@Id, 'abcdefghijklmnopqrstuvwxyz', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ') = '{identifier}']");
            var url = targetElement?.Attribute("Url")?.Value;

            if (string.IsNullOrWhiteSpace(url))
                url = "https://yourtablecloth.app/";

            rcWriter.WriteLine($"    URL_{(i + 1)} \"{url}\"");
        }
        rcWriter.WriteLine("END");
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

static TextWriter PrepareOutputDirectory(string sourceDirectory, string targetDirectory)
{
    if (Directory.Exists(targetDirectory))
    {
        Console.Out.WriteLine($"Info: Removing output directory `{targetDirectory}`...");
        Directory.Delete(targetDirectory, true);
    }

    if (!Directory.Exists(targetDirectory))
        Directory.CreateDirectory(targetDirectory);

    var logWriter = new StreamWriter(File.OpenWrite(Path.Combine(targetDirectory, "log.txt")), new UTF8Encoding(false)); ;
    var utcNow = DateTimeOffset.UtcNow;

    logWriter.WriteLine(Console.Out, $"Info: UTC Date, time and timezone - {utcNow.ToUniversalTime()}");
    logWriter.WriteLine(Console.Out, $"Info: Copying all sub-items in `{sourceDirectory}` directory to `{targetDirectory}` directory...");
    CopyAll(logWriter, sourceDirectory, targetDirectory, true);
    return logWriter;
}

static void CopyAll(TextWriter logWriter, string sourceDirectory, string targetDirectory, bool overwrite)
{
    if (!Directory.Exists(sourceDirectory))
        return;

    if (!Directory.Exists(targetDirectory))
    {
        logWriter.WriteLine(Console.Out, $"Info: Creating `{targetDirectory}` directory...");
        Directory.CreateDirectory(targetDirectory);
    }

    foreach (var filePath in Directory.GetFiles(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly))
    {
        var fileName = Path.GetFileName(filePath);
        var destFile = Path.Combine(targetDirectory, fileName);
        logWriter.WriteLine(Console.Out, $"Info: Copying `{filePath}` file into `{destFile}` file...");
        File.Copy(filePath, destFile, overwrite);
    }

    foreach (var directoryPath in Directory.GetDirectories(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly))
    {
        var directoryName = Path.GetFileName(directoryPath);
        var destDirectory = Path.Combine(targetDirectory, directoryName);
        logWriter.WriteLine(Console.Out, $"Info: Copying `{directoryName}` directories into `{destDirectory}` directory...");
        CopyAll(logWriter, directoryPath, destDirectory, overwrite);
    }
}

static class Extensions
{
    public static void WriteLine(this TextWriter logWriter, TextWriter secondaryLogWriter, string message)
    {
        logWriter.WriteLine(message);

        if (secondaryLogWriter != null)
            secondaryLogWriter.WriteLine(message);
    }

    public static async Task WriteLineAsync(this TextWriter logWriter, TextWriter secondaryLogWriter, string message, CancellationToken cancellationToken = default)
    {
        var memory = message.AsMemory();
        await logWriter.WriteLineAsync(memory, cancellationToken).ConfigureAwait(false);

        if (secondaryLogWriter != null)
            await secondaryLogWriter.WriteLineAsync(memory, cancellationToken).ConfigureAwait(false);
    }
}
