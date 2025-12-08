#!/usr/bin/env dotnet
#:package YamlDotNet@16.2.0
#:property PublishAot=false

using System.Text;
using System.Xml;
using System.Xml.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var targetDirectory = args.ElementAtOrDefault(0);

if (string.IsNullOrWhiteSpace(targetDirectory))
{
    Console.Error.WriteLine("Error: Please specify target directory to run this tool.");
    Environment.Exit(1);
    return;
}

// CancellationTokenSource 생성 및 Ctrl+C/종료 시그널 연결
using var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;
var gracefulShutdownTimeout = TimeSpan.FromSeconds(3);

Console.CancelKeyPress += (sender, e) =>
{
    if (cts.IsCancellationRequested)
    {
        Console.Out.WriteLine("Info: Force exit requested. Terminating immediately...");
        e.Cancel = false;
        return;
    }

    Console.Out.WriteLine($"Info: Cancellation requested. Shutting down gracefully (timeout: {gracefulShutdownTimeout.TotalSeconds}s)...");
    Console.Out.WriteLine("Info: Press Ctrl+C again to force exit.");
    e.Cancel = true;
    cts.CancelAfter(gracefulShutdownTimeout);
};

AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
{
    if (!cts.IsCancellationRequested)
    {
        Console.Out.WriteLine("Info: Process exit requested. Shutting down...");
        cts.Cancel();
    }
};

try
{
    Console.Out.WriteLine($"Info: Target directory path: `{targetDirectory}`");
    
    // winget-pkgs 리포지터리를 GitHub API를 통해 직접 쿼리
    var packages = new List<PackageInfo>();
    
    // Chrome, Edge, Adobe Reader 패키지 정의
    var packageConfigs = new[]
    {
        new { Id = "Google.Chrome", Path = "manifests/g/Google/Chrome" },
        new { Id = "Microsoft.Edge", Path = "manifests/m/Microsoft/Edge" },
        new { Id = "Adobe.Acrobat.Reader.64-bit", Path = "manifests/a/Adobe/Acrobat/Reader/64-bit" }
    };

    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Add("User-Agent", "TableClothCatalog-WinGetFeed/1.0");
    httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    
    // GitHub token이 환경 변수에 있으면 사용
    var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
    if (!string.IsNullOrEmpty(githubToken))
    {
        Console.Out.WriteLine("Info: Using GitHub token for authentication.");
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {githubToken}");
    }
    else
    {
        Console.Out.WriteLine("Warning: No GitHub token found. API rate limits may apply.");
    }

    foreach (var config in packageConfigs)
    {
        try
        {
            Console.Out.WriteLine($"Info: Fetching package information for {config.Id}...");
            var packageInfo = await FetchLatestPackageInfoAsync(httpClient, config.Id, config.Path, cancellationToken);
            
            if (packageInfo != null)
            {
                packages.Add(packageInfo);
                Console.Out.WriteLine($"Info: Found version {packageInfo.Version} for {config.Id}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: Failed to fetch {config.Id}: {ex.Message}");
        }
    }

    // XML 파일 생성
    await GeneratePackageFeedXmlAsync(packages, targetDirectory, cancellationToken);
    
    Console.Out.WriteLine("Info: WinGet package feed generation completed successfully.");
    Environment.Exit(0);
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Error: Operation was cancelled.");
    Environment.Exit(130);
}
catch (Exception thrownException)
{
    Console.Error.WriteLine($"Error: {thrownException}");
    Environment.Exit(2);
}

static async Task<PackageInfo?> FetchLatestPackageInfoAsync(
    HttpClient httpClient, 
    string packageId, 
    string manifestPath,
    CancellationToken cancellationToken)
{
    try
    {
        // GitHub API를 사용하여 최신 버전 디렉터리 목록 가져오기
        var apiUrl = $"https://api.github.com/repos/microsoft/winget-pkgs/contents/{manifestPath}";
        var response = await httpClient.GetStringAsync(apiUrl, cancellationToken);
        
        // JSON 파싱하여 디렉터리 목록 가져오기
        using var directories = System.Text.Json.JsonDocument.Parse(response);
        var directoriesRoot = directories.RootElement;
        
        // 버전 번호로 보이는 디렉터리 찾기 (숫자로 시작하는 것들)
        var versionDirs = new List<(string Name, string Path, string Url)>();
        
        foreach (var item in directoriesRoot.EnumerateArray())
        {
            if (item.GetProperty("type").GetString() == "dir")
            {
                var name = item.GetProperty("name").GetString();
                var path = item.GetProperty("path").GetString();
                var url = item.GetProperty("url").GetString();
                
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(url) && char.IsDigit(name[0]))
                {
                    versionDirs.Add((name, path, url));
                }
            }
        }
        
        if (!versionDirs.Any())
        {
            Console.Error.WriteLine($"Warning: No version directories found for {packageId}");
            return null;
        }
        
        // 버전 정렬 (최신 버전 찾기)
        var latestVersion = versionDirs
            .OrderByDescending(x => ParseVersion(x.Name))
            .First();
        
        Console.Out.WriteLine($"Info: Latest version for {packageId}: {latestVersion.Name}");
        
        // installer manifest 파일 가져오기
        var filesResponse = await httpClient.GetStringAsync(latestVersion.Url, cancellationToken);
        using var filesDoc = System.Text.Json.JsonDocument.Parse(filesResponse);
        var files = filesDoc.RootElement;
        
        string? installerManifestUrl = null;
        
        foreach (var file in files.EnumerateArray())
        {
            var fileName = file.GetProperty("name").GetString();
            if (fileName != null && fileName.EndsWith(".installer.yaml", StringComparison.OrdinalIgnoreCase))
            {
                installerManifestUrl = file.GetProperty("download_url").GetString();
                break;
            }
        }
        
        if (string.IsNullOrEmpty(installerManifestUrl))
        {
            Console.Error.WriteLine($"Warning: No installer manifest found for {packageId}");
            return null;
        }
        
        // YAML 파일 다운로드 및 파싱
        var yamlContent = await httpClient.GetStringAsync(installerManifestUrl, cancellationToken);
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        
        var manifest = deserializer.Deserialize<Dictionary<string, object>>(yamlContent);
        
        // 설치 프로그램 URL 추출
        var installerUrls = new List<string>();
        
        if (manifest.TryGetValue("Installers", out var installersObj) && installersObj is List<object> installers)
        {
            foreach (var installer in installers)
            {
                if (installer is Dictionary<object, object> installerDict)
                {
                    if (installerDict.TryGetValue("InstallerUrl", out var urlObj) && urlObj is string url)
                    {
                        installerUrls.Add(url);
                    }
                }
            }
        }
        
        if (!installerUrls.Any())
        {
            Console.Error.WriteLine($"Warning: No installer URLs found in manifest for {packageId}");
            return null;
        }
        
        // 첫 번째 URL 사용 (보통 x64용)
        return new PackageInfo(packageId, latestVersion.Name, installerUrls.First());
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error fetching package info for {packageId}: {ex.Message}");
        throw;
    }
}

static Version ParseVersion(string versionString)
{
    try
    {
        // 버전 문자열에서 숫자와 점만 추출
        var sb = new StringBuilder(versionString.Length);
        foreach (var c in versionString)
        {
            if (char.IsDigit(c) || c == '.')
            {
                sb.Append(c);
            }
        }
        var cleanVersion = sb.ToString().TrimEnd('.');
        
        // 빈 문자열이거나 점으로만 구성된 경우 실패
        if (string.IsNullOrWhiteSpace(cleanVersion))
        {
            return new Version(0, 0, 0, 0);
        }
        
        // 버전 파싱 시도
        if (Version.TryParse(cleanVersion, out var version))
        {
            return version;
        }
        
        // 파싱 실패 시 0.0.0.0 반환
        return new Version(0, 0, 0, 0);
    }
    catch
    {
        return new Version(0, 0, 0, 0);
    }
}

static async Task GeneratePackageFeedXmlAsync(
    List<PackageInfo> packages,
    string targetDirectory,
    CancellationToken cancellationToken)
{
    var outputPath = Path.Combine(targetDirectory, "PackageFeed.xml");
    
    Console.Out.WriteLine($"Info: Generating package feed XML at {outputPath}...");
    
    var xmlDocument = new XDocument(
        new XDeclaration("1.0", "utf-8", "yes"),
        new XElement("PackageFeed",
            new XAttribute("GeneratedAt", DateTimeOffset.UtcNow.ToString("o")),
            new XElement("Packages",
                packages.Select(p =>
                    new XElement("Package",
                        new XAttribute("Id", p.PackageId),
                        new XAttribute("Version", p.Version),
                        new XAttribute("InstallerUrl", p.InstallerUrl)
                    )
                )
            )
        )
    );
    
    await using var stream = File.Create(outputPath);
    await using var writer = XmlWriter.Create(stream, new XmlWriterSettings
    {
        Async = true,
        Encoding = new UTF8Encoding(false),
        Indent = true,
        IndentChars = "  "
    });
    
    await xmlDocument.SaveAsync(writer, cancellationToken);
    await writer.FlushAsync();
    
    Console.Out.WriteLine($"Info: Package feed XML generated successfully with {packages.Count} packages.");
}

record PackageInfo(string PackageId, string Version, string InstallerUrl);
