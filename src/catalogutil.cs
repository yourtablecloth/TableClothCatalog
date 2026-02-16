#!/usr/bin/env dotnet
#:package IronSoftware.System.Drawing@2025.9.3
#:package Microsoft.Extensions.Hosting@9.0.0
#:package Microsoft.Extensions.Http@9.0.0
#:property PublishAot=false

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

// Generic Host 설정
var builder = Host.CreateApplicationBuilder(args);

// HTTP 클라이언트 타임아웃 설정 (기본값: 15초, 최소값: 5초)
const double DefaultTimeoutSeconds = 15d;
const double MinTimeoutSeconds = 5d;
var configuredTimeout = builder.Configuration.GetValue("TimeoutSeconds", DefaultTimeoutSeconds);
var timeoutSeconds = Math.Max(configuredTimeout, MinTimeoutSeconds);

// 서비스 등록
builder.Services.AddSingleton<CatalogLoader>();
builder.Services.AddSingleton<ImageConverter>();
builder.Services.AddSingleton<ZipArchiver>();
builder.Services.AddSingleton<SchemaValidator>();
builder.Services.AddSingleton<UrlTester>();
builder.Services.AddSingleton<CatalogProcessor>();
builder.Services.AddHttpClient("CatalogClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "curl/8.4.0");
    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
});

// 커맨드라인 플래그 파싱
var verbose = args.Any(a => a is "--verbose" or "-v");
var positionalArgs = args.Where(a => !a.StartsWith('-')).ToArray();

// 로깅 구성
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(verbose ? LogLevel.Information : LogLevel.Warning);

using var host = builder.Build();

// Ctrl+C 강제 종료 처리
using var cts = new CancellationTokenSource();
var forceTerminate = false;

Console.CancelKeyPress += (sender, e) =>
{
    if (forceTerminate)
    {
        Console.WriteLine("Force exit requested. Terminating immediately...");
        Environment.Exit(130);
    }

    Console.WriteLine("Cancellation requested. Press Ctrl+C again to force exit...");
    forceTerminate = true;
    e.Cancel = true;
    cts.Cancel();
};

// Host 시작 (서비스 초기화)
await host.StartAsync();

var exitCode = 0;
try
{
    var processor = host.Services.GetRequiredService<CatalogProcessor>();
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
        cts.Token, 
        host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
    exitCode = await processor.ProcessAsync(positionalArgs, linkedCts.Token);
}
finally
{
    // Host 종료 (서비스 정리)
    await host.StopAsync();
}

Environment.Exit(exitCode);

public class CatalogLoader
{
    public async Task<XDocument> LoadAsync(string targetDirectory, CancellationToken cancellationToken = default)
    {
        var catalogXmlPath = Path.Combine(targetDirectory, "Catalog.xml");
        using var fileStream = File.OpenRead(catalogXmlPath);
        return await XDocument.LoadAsync(
            fileStream, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo, cancellationToken).ConfigureAwait(false);
    }
}

public class ImageConverter
{
    private readonly ILogger<ImageConverter> _logger;

    public ImageConverter(ILogger<ImageConverter> logger)
    {
        _logger = logger;
    }

    public void ConvertAllImages(XDocument catalog, string targetDirectory)
    {
        var imageDirectory = Path.Combine(targetDirectory, "images");
        _logger.LogInformation("Investigating directory `{ImageDirectory}`...", imageDirectory);

        var pngFiles = Directory.GetFiles(imageDirectory, "*.png", SearchOption.AllDirectories);
        _logger.LogInformation("Found {Count} png files in `{ImageDirectory}` directory.", pngFiles.Length, imageDirectory);

        foreach (var eachPngFile in pngFiles)
        {
            var directoryPath = Path.GetDirectoryName(eachPngFile);

            if (string.IsNullOrEmpty(directoryPath))
            {
                _logger.LogInformation("Cannot obtain directory path of `{PngFile}` file.", eachPngFile);
                continue;
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(eachPngFile);
            var iconFilePath = Path.Combine(directoryPath, $"{fileNameWithoutExtension}.ico");

            _logger.LogInformation("Converting `{PngFile}` image file into `{IconFile}` Win32 icon file.", eachPngFile, iconFilePath);
            ConvertPngToIcon(eachPngFile, iconFilePath);
        }
    }

    public void ConvertPngToIcon(string sourcePath, string targetPath)
    {
        using var outputFile = File.OpenWrite(targetPath);
        using var outputWriter = new BinaryWriter(outputFile);
        using var inputFile = File.OpenRead(sourcePath);
        using var inputImage = Image.Load(inputFile);

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
        outputWriter.Write(0);          // 8 : image size

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

public class ZipArchiver
{
    private readonly ILogger<ZipArchiver> _logger;

    public ZipArchiver(ILogger<ZipArchiver> logger)
    {
        _logger = logger;
    }

    public void CreateImageResourceZip(XDocument catalog, string targetDirectory)
    {
        var imageDirectory = Path.Combine(targetDirectory, "images");
        _logger.LogInformation("Investigating directory `{ImageDirectory}`...", imageDirectory);

        var pngFiles = Directory.GetFiles(imageDirectory, "*.png", SearchOption.AllDirectories);
        var icoFiles = Directory.GetFiles(imageDirectory, "*.ico", SearchOption.AllDirectories);
        var sourceFiles = Enumerable.Concat(pngFiles, icoFiles).ToArray();
        _logger.LogInformation("Found total {Count} source files.", sourceFiles.Length);

        var workingDirectory = Path.Combine(targetDirectory, "working");
        if (!Directory.Exists(workingDirectory))
        {
            _logger.LogInformation("Creating working directory `{WorkingDirectory}`...", workingDirectory);
            Directory.CreateDirectory(workingDirectory);
        }

        foreach (var eachResourceFile in sourceFiles)
        {
            var destFilePath = Path.Combine(workingDirectory, Path.GetFileName(eachResourceFile));

            if (File.Exists(destFilePath))
                _logger.LogWarning("Duplicated file found. Source: `{ResourceFile}`.", eachResourceFile);

            _logger.LogInformation("Copying `{ResourceFile}` file to `{DestFile}` file...", eachResourceFile, destFilePath);
            File.Copy(eachResourceFile, destFilePath, true);
        }

        var zipFilePath = Path.Combine(targetDirectory, "Images.zip");
        _logger.LogInformation("Creating `{ZipFile}` zip file...", zipFilePath);
        ZipFile.CreateFromDirectory(workingDirectory, zipFilePath, CompressionLevel.Optimal, false);

        _logger.LogInformation("Removing working directory `{WorkingDirectory}`...", workingDirectory);
        Directory.Delete(workingDirectory, true);
    }
}

public class UrlTester
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UrlTester> _logger;

    public UrlTester(IHttpClientFactory httpClientFactory, ILogger<UrlTester> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<bool> TestAsync(ConcurrentQueue<string> errorLogBuffer, string urlString, string element, int lineNumber, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient("CatalogClient");

        try
        {
            var response = await httpClient.GetAsync(urlString, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (response == null || !response.IsSuccessStatusCode)
            {
                var message = $"Cannot fetch the URI `{urlString}` (Remote response code: {(int?)response?.StatusCode}) - (Line: `{lineNumber}`)";
                _logger.LogWarning("{Message}", message);
                errorLogBuffer.Enqueue(message);
                return false;
            }
        }
        catch (AggregateException aex)
        {
            var ex = aex.InnerException ?? aex;
            var message = $"Cannot fetch the URI `{urlString}` ({ex.GetType().Name} throwed. {ex.InnerException?.Message ?? ex.Message}) - (Line: `{lineNumber}`)";
            _logger.LogWarning("{Message}", message);
            errorLogBuffer.Enqueue(message);
            return false;
        }
        catch (Exception ex)
        {
            var message = $"Cannot fetch the URI `{urlString}` ({ex.GetType().Name} throwed. {ex.InnerException?.Message ?? ex.Message}) - (Line: `{lineNumber}`)";
            _logger.LogWarning("{Message}", message);
            errorLogBuffer.Enqueue(message);
            return false;
        }

        _logger.LogInformation("Test succeed on `{Url}`.", urlString);
        return true;
    }
}

public class SchemaValidator
{
    private readonly UrlTester _urlTester;
    private readonly ILogger<SchemaValidator> _logger;

    public SchemaValidator(UrlTester urlTester, ILogger<SchemaValidator> logger)
    {
        _urlTester = urlTester;
        _logger = logger;
    }

    public List<ProblemItem> Validate(string targetDirectory, bool evaluateUrls)
    {
        var problems = new List<ProblemItem>();
        var catalogXmlPath = Path.Combine(targetDirectory, "Catalog.xml");
        var schemaXsdPath = Path.Combine(targetDirectory, "Catalog.xsd");

        var config = new XmlReaderSettings()
        {
            ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings,
        };

        _logger.LogInformation("Using schema `{SchemaPath}`.", schemaXsdPath);
        config.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
        config.ValidationType = ValidationType.Schema;
        config.Schemas.Add(null, schemaXsdPath);

        config.ValidationEventHandler += new ValidationEventHandler((_sender, _e) =>
        {
            var positionInfo = GetPositionInfo(_sender as IXmlLineInfo);
            var severity = _e.Severity == XmlSeverityType.Warning ? "Warning" : "Error";
            problems.Add(new ProblemItem(severity, _e.Message, positionInfo));
        });

        _logger.LogInformation("Validating `{CatalogPath}` file.", catalogXmlPath);
        var testTargets = new Dictionary<string, Tuple<string, int>>();

        using (XmlReader reader = XmlReader.Create(catalogXmlPath, config))
        {
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
                                var posInfo = GetPositionInfo(reader as IXmlLineInfo);
                                problems.Add(new ProblemItem("Error", $"[{reader.Name}] URL string cannot be empty", posInfo));
                                continue;
                            }

                            if (!Uri.TryCreate(urlString, UriKind.Absolute, out Uri? result) || result == null)
                            {
                                var posInfo = GetPositionInfo(reader as IXmlLineInfo);
                                problems.Add(new ProblemItem("Error", $"Cannot parse the URI `{urlString}`", posInfo));
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
                    var ex = aex.InnerException ?? aex;
                    var posInfo = GetPositionInfo(reader as IXmlLineInfo);
                    problems.Add(new ProblemItem("Error", $"{ex.GetType().Name} / {ex.InnerException?.GetType().Name} throwed. ({ex.InnerException?.Message ?? ex.Message})", posInfo));
                    continue;
                }
                catch (Exception ex)
                {
                    var posInfo = GetPositionInfo(reader as IXmlLineInfo);
                    problems.Add(new ProblemItem("Error", $"{ex.GetType().Name} / {ex.InnerException?.GetType().Name} throwed. ({ex.InnerException?.Message ?? ex.Message})", posInfo));
                    continue;
                }
            }
        }

        if (evaluateUrls)
        {
            var errorLogBuffer = new ConcurrentQueue<string>();
            var tasks = new List<Task<bool>>(testTargets.Count);

            foreach (var eachTestTarget in testTargets.AsParallel())
                tasks.Add(_urlTester.TestAsync(errorLogBuffer, eachTestTarget.Key, eachTestTarget.Value.Item1, eachTestTarget.Value.Item2));

            var results = Task.WhenAll(tasks).Result;

            foreach (var eachErrorLog in errorLogBuffer.ToList())
                problems.Add(new ProblemItem("Warning", eachErrorLog, null));

            _logger.LogInformation("Test runner completed. (Scheduled: {Scheduled} / Returned: {Returned} / Succeed: {Succeed} / Failed: {Failed})", testTargets.Count, results.Length, results.Count(x => x), results.Count(x => !x));

            if (testTargets.Count != results.Length)
                problems.Add(new ProblemItem("Warning", "Some test targets skipped due to task cancellation.", null));
        }

        return problems;
    }

    private static string? GetPositionInfo(IXmlLineInfo? lineInfo)
    {
        var positionText = "Unknown Position";
        if (lineInfo != null)
            positionText = $"Line: {lineInfo.LineNumber}, Line Position: {lineInfo.LinePosition}";
        return positionText;
    }
}

public class CatalogProcessor
{
    private readonly ILogger<CatalogProcessor> _logger;
    private readonly CatalogLoader _catalogLoader;
    private readonly ImageConverter _imageConverter;
    private readonly ZipArchiver _zipArchiver;
    private readonly SchemaValidator _schemaValidator;

    public CatalogProcessor(
        ILogger<CatalogProcessor> logger,
        CatalogLoader catalogLoader,
        ImageConverter imageConverter,
        ZipArchiver zipArchiver,
        SchemaValidator schemaValidator)
    {
        _logger = logger;
        _catalogLoader = catalogLoader;
        _imageConverter = imageConverter;
        _zipArchiver = zipArchiver;
        _schemaValidator = schemaValidator;
    }

    public async Task<int> ProcessAsync(string[] args, CancellationToken cancellationToken = default)
    {
        try
        {
            var sourceDirectory = args.ElementAtOrDefault(0);
            var targetDirectory = args.ElementAtOrDefault(1);

            if (string.IsNullOrWhiteSpace(sourceDirectory) || !Directory.Exists(sourceDirectory))
            {
                _logger.LogError("Please specify source directory to run this tool.");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(targetDirectory))
            {
                _logger.LogError("Please specify target directory to run this tool.");
                return 1;
            }

            _logger.LogInformation("Source directory path: `{SourceDirectory}`", sourceDirectory);
            _logger.LogInformation("Target directory path: `{TargetDirectory}`", targetDirectory);

            PrepareOutputDirectory(sourceDirectory, targetDirectory);
            var catalog = await _catalogLoader.LoadAsync(targetDirectory, cancellationToken).ConfigureAwait(false);

            _imageConverter.ConvertAllImages(catalog, targetDirectory);
            _zipArchiver.CreateImageResourceZip(catalog, targetDirectory);
            var problems = _schemaValidator.Validate(targetDirectory, true);

            // 문제 항목만 추려서 표시
            if (problems.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"=== 문제 항목 요약 ({problems.Count}건) ===");
                Console.WriteLine();

                var errors = problems.Where(p => p.Severity == "Error").ToList();
                var warnings = problems.Where(p => p.Severity == "Warning").ToList();

                if (errors.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[오류] {errors.Count}건:");
                    foreach (var error in errors)
                        Console.WriteLine($"  - {error.Message}" + (error.Position != null ? $" ({error.Position})" : ""));
                    Console.ResetColor();
                    Console.WriteLine();
                }

                if (warnings.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[경고] {warnings.Count}건:");
                    foreach (var warning in warnings)
                        Console.WriteLine($"  - {warning.Message}" + (warning.Position != null ? $" ({warning.Position})" : ""));
                    Console.ResetColor();
                    Console.WriteLine();
                }

                return errors.Count > 0 ? 1 : 0;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("문제 항목이 없습니다. 모든 검증을 통과했습니다.");
            }

            _logger.LogInformation("Catalog builder runs with succeed result.");
            return 0;
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Operation was cancelled.");
            return 130;
        }
        catch (Exception thrownException)
        {
            _logger.LogError(thrownException, "An error occurred.");
            return 2;
        }
    }

    private void PrepareOutputDirectory(string sourceDirectory, string targetDirectory)
    {
        if (Directory.Exists(targetDirectory))
        {
            _logger.LogInformation("Removing output directory `{TargetDirectory}`...", targetDirectory);
            Directory.Delete(targetDirectory, true);
        }

        if (!Directory.Exists(targetDirectory))
            Directory.CreateDirectory(targetDirectory);

        var utcNow = DateTimeOffset.UtcNow;
        _logger.LogInformation("UTC Date, time and timezone - {UtcNow}", utcNow.ToUniversalTime());
        _logger.LogInformation("Copying all sub-items in `{SourceDirectory}` directory to `{TargetDirectory}` directory...", sourceDirectory, targetDirectory);
        CopyAll(sourceDirectory, targetDirectory, true);
    }

    private void CopyAll(string sourceDirectory, string targetDirectory, bool overwrite)
    {
        if (!Directory.Exists(sourceDirectory))
            return;

        if (!Directory.Exists(targetDirectory))
        {
            _logger.LogInformation("Creating `{TargetDirectory}` directory...", targetDirectory);
            Directory.CreateDirectory(targetDirectory);
        }

        foreach (var filePath in Directory.GetFiles(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly))
        {
            var fileName = Path.GetFileName(filePath);
            var destFile = Path.Combine(targetDirectory, fileName);
            _logger.LogInformation("Copying `{FilePath}` file into `{DestFile}` file...", filePath, destFile);
            File.Copy(filePath, destFile, overwrite);
        }

        foreach (var directoryPath in Directory.GetDirectories(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly))
        {
            var directoryName = Path.GetFileName(directoryPath);
            var destDirectory = Path.Combine(targetDirectory, directoryName);
            _logger.LogInformation("Copying `{DirectoryName}` directories into `{DestDirectory}` directory...", directoryName, destDirectory);
            CopyAll(directoryPath, destDirectory, overwrite);
        }
    }
}

public record ProblemItem(string Severity, string Message, string? Position);