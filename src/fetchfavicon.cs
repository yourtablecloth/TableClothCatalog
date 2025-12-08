#!/usr/bin/env dotnet
#:package SixLabors.ImageSharp@3.1.6
#:package HtmlAgilityPack@1.11.72
#:package System.Drawing.Common@9.0.0
#:property PublishAot=false

using HtmlAgilityPack;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using System.Text.RegularExpressions;
using SixImage = SixLabors.ImageSharp.Image;
using SixSize = SixLabors.ImageSharp.Size;

// Process command line arguments
var url = args.ElementAtOrDefault(0);
var outputPath = args.ElementAtOrDefault(1);
var targetSize = 256; // Default target size

if (args.Length > 2 && int.TryParse(args[2], out var size))
{
    targetSize = size;
}

// Validate required arguments
if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(outputPath))
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("Favicon Fetcher & Upscaler Tool");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine("Usage: dotnet run fetchfavicon.cs <url> <outputPath> [targetSize]");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  url         URL of the website to fetch favicon from");
    Console.WriteLine("  outputPath  Path to save the output PNG file");
    Console.WriteLine("  targetSize  Target image size in pixels (default: 256)");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine("  dotnet run .\\src\\fetchfavicon.cs https://www.wooribank.com .\\docs\\images\\Banking\\WooriBank.png");
    Console.WriteLine("  dotnet run .\\src\\fetchfavicon.cs https://www.wooribank.com .\\docs\\images\\Banking\\WooriBank.png 512");
    return 1;
}

// Ensure output directory exists
var outputDir = Path.GetDirectoryName(outputPath);
if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
{
    Directory.CreateDirectory(outputDir);
}

using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
httpClient.Timeout = TimeSpan.FromSeconds(30);

try
{
    var baseUri = new Uri(url);
    Console.WriteLine($"Fetching favicon from: {url}");
    Console.WriteLine();

    // Try multiple favicon sources
    var faviconUrls = await GetFaviconUrlsAsync(httpClient, baseUri);
    
    Console.WriteLine($"Found {faviconUrls.Count} potential favicon URL(s):");
    foreach (var faviconUrl in faviconUrls)
    {
        Console.WriteLine($"  - {faviconUrl}");
    }
    Console.WriteLine();

    SixImage? bestImage = null;
    var bestSize = 0;
    string? bestUrl = null;

    foreach (var faviconUrl in faviconUrls)
    {
        try
        {
            var imageBytes = await httpClient.GetByteArrayAsync(faviconUrl);
            
            // Try to load as ICO first (may contain multiple sizes)
            var images = await LoadImagesFromBytesAsync(imageBytes);
            
            foreach (var img in images)
            {
                var currentSize = Math.Max(img.Width, img.Height);
                if (currentSize > bestSize)
                {
                    bestImage?.Dispose();
                    bestImage = img;
                    bestSize = currentSize;
                    bestUrl = faviconUrl;
                }
                else
                {
                    img.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"  Warning: Failed to load {faviconUrl}: {ex.Message}");
            Console.ResetColor();
        }
    }

    if (bestImage == null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: Could not find or load any favicon.");
        Console.ResetColor();
        return 1;
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Best favicon found: {bestUrl}");
    Console.WriteLine($"  Original size: {bestImage.Width}x{bestImage.Height}");
    Console.ResetColor();

    // Upscale if needed
    if (bestImage.Width < targetSize || bestImage.Height < targetSize)
    {
        Console.WriteLine($"  Upscaling to: {targetSize}x{targetSize}");
        
        // Calculate aspect-ratio-preserving resize
        var scale = (double)targetSize / Math.Max(bestImage.Width, bestImage.Height);
        var newWidth = (int)(bestImage.Width * scale);
        var newHeight = (int)(bestImage.Height * scale);
        
        bestImage.Mutate(x => x
            .Resize(new ResizeOptions
            {
                Size = new SixSize(newWidth, newHeight),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3
            })
            .Pad(targetSize, targetSize)
        );
    }

    // Save as PNG
    var encoder = new PngEncoder
    {
        ColorType = PngColorType.RgbWithAlpha,
        CompressionLevel = PngCompressionLevel.BestCompression,
        BitDepth = PngBitDepth.Bit8
    };

    outputPath = Path.GetFullPath(outputPath);
    await bestImage.SaveAsPngAsync(outputPath, encoder);
    bestImage.Dispose();

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"Saved: {outputPath}");
    Console.ResetColor();

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Note: Please review the generated image and replace with an official logo if available.");
    Console.ResetColor();

    return 0;
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: {ex.Message}");
    Console.ResetColor();
    return 1;
}

// Get all possible favicon URLs from a website
async Task<List<string>> GetFaviconUrlsAsync(HttpClient client, Uri baseUri)
{
    var urls = new List<string>();
    
    try
    {
        // Fetch HTML and look for link tags
        var html = await client.GetStringAsync(baseUri);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Look for various favicon link patterns
        var linkSelectors = new[]
        {
            "//link[@rel='icon']",
            "//link[@rel='shortcut icon']",
            "//link[@rel='apple-touch-icon']",
            "//link[@rel='apple-touch-icon-precomposed']",
            "//link[contains(@rel, 'icon')]"
        };

        var foundHrefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var selector in linkSelectors)
        {
            var nodes = doc.DocumentNode.SelectNodes(selector);
            if (nodes == null) continue;

            foreach (var node in nodes)
            {
                var href = node.GetAttributeValue("href", null);
                if (string.IsNullOrEmpty(href)) continue;

                // Get sizes attribute if available (prefer larger icons)
                var sizes = node.GetAttributeValue("sizes", "");
                
                if (foundHrefs.Add(href))
                {
                    var absoluteUrl = GetAbsoluteUrl(baseUri, href);
                    if (!string.IsNullOrEmpty(absoluteUrl))
                    {
                        urls.Add(absoluteUrl);
                    }
                }
            }
        }

        // Look for meta og:image as fallback
        var ogImage = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
        if (ogImage != null)
        {
            var content = ogImage.GetAttributeValue("content", null);
            if (!string.IsNullOrEmpty(content))
            {
                var absoluteUrl = GetAbsoluteUrl(baseUri, content);
                if (!string.IsNullOrEmpty(absoluteUrl) && foundHrefs.Add(content))
                {
                    urls.Add(absoluteUrl);
                }
            }
        }

        // Look for Web App Manifest (manifest.json or site.webmanifest)
        var manifestLinks = doc.DocumentNode.SelectNodes("//link[@rel='manifest']");
        if (manifestLinks != null)
        {
            foreach (var manifestLink in manifestLinks)
            {
                var manifestHref = manifestLink.GetAttributeValue("href", null);
                if (!string.IsNullOrEmpty(manifestHref))
                {
                    var manifestUrl = GetAbsoluteUrl(baseUri, manifestHref);
                    if (!string.IsNullOrEmpty(manifestUrl))
                    {
                        var manifestIcons = await GetIconsFromManifestAsync(client, baseUri, manifestUrl);
                        foreach (var iconUrl in manifestIcons)
                        {
                            if (foundHrefs.Add(iconUrl))
                            {
                                urls.Add(iconUrl);
                            }
                        }
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"  Warning: Could not parse HTML: {ex.Message}");
        Console.ResetColor();
    }

    // Try to fetch common manifest locations
    var manifestPaths = new[]
    {
        "/manifest.json",
        "/site.webmanifest",
        "/manifest.webmanifest"
    };

    foreach (var manifestPath in manifestPaths)
    {
        try
        {
            var manifestUrl = new Uri(baseUri, manifestPath).AbsoluteUri;
            var manifestIcons = await GetIconsFromManifestAsync(client, baseUri, manifestUrl);
            foreach (var iconUrl in manifestIcons)
            {
                if (!urls.Contains(iconUrl, StringComparer.OrdinalIgnoreCase))
                {
                    urls.Add(iconUrl);
                }
            }
        }
        catch { }
    }

    // Always try common favicon locations (including mobile device icons)
    var commonPaths = new[]
    {
        // Standard favicons
        "/favicon.ico",
        "/favicon.png",
        "/favicon-32x32.png",
        "/favicon-16x16.png",
        // Apple Touch Icons (iOS)
        "/apple-touch-icon.png",
        "/apple-touch-icon-180x180.png",
        "/apple-touch-icon-152x152.png",
        "/apple-touch-icon-144x144.png",
        "/apple-touch-icon-120x120.png",
        "/apple-touch-icon-114x114.png",
        "/apple-touch-icon-76x76.png",
        "/apple-touch-icon-72x72.png",
        "/apple-touch-icon-60x60.png",
        "/apple-touch-icon-57x57.png",
        "/apple-touch-icon-precomposed.png",
        // Android/Chrome icons
        "/android-chrome-512x512.png",
        "/android-chrome-384x384.png",
        "/android-chrome-256x256.png",
        "/android-chrome-192x192.png",
        "/chrome-touch-icon-192x192.png",
        // Microsoft Tile icons
        "/mstile-310x310.png",
        "/mstile-150x150.png",
        "/mstile-144x144.png",
        // Safari Pinned Tab
        "/safari-pinned-tab.svg",
        // Common icon paths
        "/icon.png",
        "/icon-512x512.png",
        "/icon-256x256.png",
        "/icon-192x192.png",
        "/logo.png",
        "/images/favicon.png",
        "/assets/favicon.png"
    };

    foreach (var path in commonPaths)
    {
        var commonUrl = new Uri(baseUri, path).AbsoluteUri;
        if (!urls.Contains(commonUrl, StringComparer.OrdinalIgnoreCase))
        {
            urls.Add(commonUrl);
        }
    }

    // Add favicon service providers as fallback (Google and DuckDuckGo)
    var domain = baseUri.Host;
    var faviconServices = new[]
    {
        // Google Favicon Service - supports various sizes
        $"https://www.google.com/s2/favicons?domain={domain}&sz=128",
        $"https://www.google.com/s2/favicons?domain={domain}&sz=64",
        $"https://www.google.com/s2/favicons?domain={domain}&sz=32",
        // DuckDuckGo Favicon Service - returns ICO with multiple sizes
        $"https://icons.duckduckgo.com/ip3/{domain}.ico"
    };

    foreach (var serviceUrl in faviconServices)
    {
        if (!urls.Contains(serviceUrl, StringComparer.OrdinalIgnoreCase))
        {
            urls.Add(serviceUrl);
        }
    }

    // Sort URLs to prioritize larger icons (based on filename patterns)
    urls = urls
        .OrderByDescending(u => GetIconPriority(u))
        .ToList();

    return urls;
}

// Parse Web App Manifest to get icon URLs
async Task<List<string>> GetIconsFromManifestAsync(HttpClient client, Uri baseUri, string manifestUrl)
{
    var icons = new List<string>();
    
    try
    {
        var manifestJson = await client.GetStringAsync(manifestUrl);
        using var doc = JsonDocument.Parse(manifestJson);
        
        if (doc.RootElement.TryGetProperty("icons", out var iconsElement) && 
            iconsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var iconElement in iconsElement.EnumerateArray())
            {
                if (iconElement.TryGetProperty("src", out var srcElement))
                {
                    var src = srcElement.GetString();
                    if (!string.IsNullOrEmpty(src))
                    {
                        // Manifest URLs are relative to manifest location, not base URI
                        var manifestUri = new Uri(manifestUrl);
                        var absoluteUrl = GetAbsoluteUrl(manifestUri, src) ?? GetAbsoluteUrl(baseUri, src);
                        if (!string.IsNullOrEmpty(absoluteUrl))
                        {
                            icons.Add(absoluteUrl);
                        }
                    }
                }
            }
        }
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  Found {icons.Count} icon(s) in manifest: {manifestUrl}");
        Console.ResetColor();
    }
    catch (HttpRequestException)
    {
        // Manifest not found - this is expected for many sites
    }
    catch (JsonException ex)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"  Warning: Could not parse manifest {manifestUrl}: {ex.Message}");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"  Warning: Error fetching manifest {manifestUrl}: {ex.Message}");
        Console.ResetColor();
    }
    
    return icons;
}

string? GetAbsoluteUrl(Uri baseUri, string href)
{
    try
    {
        if (href.StartsWith("//"))
        {
            return $"{baseUri.Scheme}:{href}";
        }
        
        if (Uri.TryCreate(baseUri, href, out var absoluteUri))
        {
            return absoluteUri.AbsoluteUri;
        }
    }
    catch { }
    
    return null;
}

int GetIconPriority(string url)
{
    var priority = 0;
    var lowerUrl = url.ToLowerInvariant();

    // Prefer PNG over ICO
    if (lowerUrl.EndsWith(".png")) priority += 100;
    if (lowerUrl.EndsWith(".svg")) priority += 50;
    
    // Prefer apple-touch-icon (usually larger)
    if (lowerUrl.Contains("apple-touch-icon")) priority += 200;
    
    // Prefer Android/Chrome icons (usually high resolution)
    if (lowerUrl.Contains("android-chrome")) priority += 250;
    if (lowerUrl.Contains("chrome-touch-icon")) priority += 200;
    
    // Microsoft tile icons
    if (lowerUrl.Contains("mstile")) priority += 150;
    
    // Extract size from URL if present
    var sizeMatch = Regex.Match(lowerUrl, @"(\d+)x(\d+)");
    if (sizeMatch.Success && int.TryParse(sizeMatch.Groups[1].Value, out var size))
    {
        priority += size;
    }

    // Size in filename
    var sizeMatch2 = Regex.Match(lowerUrl, @"-(\d+)\.(?:png|ico)");
    if (sizeMatch2.Success && int.TryParse(sizeMatch2.Groups[1].Value, out var size2))
    {
        priority += size2;
    }

    // Handle Google Favicon Service size parameter (e.g., &sz=128)
    var googleSizeMatch = Regex.Match(lowerUrl, @"[?&]sz=(\d+)");
    if (googleSizeMatch.Success && int.TryParse(googleSizeMatch.Groups[1].Value, out var googleSize))
    {
        priority += googleSize;
    }

    // Favicon services are fallbacks, give them lower priority than direct sources
    if (lowerUrl.Contains("google.com/s2/favicons") || lowerUrl.Contains("icons.duckduckgo.com"))
    {
        priority -= 500;
    }

    return priority;
}

async Task<List<SixImage>> LoadImagesFromBytesAsync(byte[] bytes)
{
    var images = new List<SixImage>();
    
    // Check if it's an ICO file (starts with 0x00 0x00 0x01 0x00 or 0x00 0x00 0x02 0x00)
    if (bytes.Length >= 6 && bytes[0] == 0x00 && bytes[1] == 0x00 && 
        (bytes[2] == 0x01 || bytes[2] == 0x02) && bytes[3] == 0x00)
    {
        // Use System.Drawing to load ICO file (supports BMP-based icons)
        try
        {
            using var ms = new MemoryStream(bytes);
            using var icon = new Icon(ms);
            
            // Try to get the largest size
            var sizes = new[] { 256, 128, 64, 48, 32, 16 };
            Bitmap? bestBitmap = null;
            var bestSize = 0;
            
            foreach (var size in sizes)
            {
                try
                {
                    using var sizedIcon = new Icon(icon, size, size);
                    var bitmap = sizedIcon.ToBitmap();
                    if (bitmap.Width > bestSize)
                    {
                        bestBitmap?.Dispose();
                        bestBitmap = bitmap;
                        bestSize = bitmap.Width;
                    }
                    else
                    {
                        bitmap.Dispose();
                    }
                }
                catch { }
            }
            
            if (bestBitmap == null)
            {
                bestBitmap = icon.ToBitmap();
            }
            
            if (bestBitmap != null)
            {
                // Convert System.Drawing.Bitmap to ImageSharp.Image
                using var pngMs = new MemoryStream();
                bestBitmap.Save(pngMs, ImageFormat.Png);
                pngMs.Position = 0;
                
                var img = await SixImage.LoadAsync(pngMs);
                images.Add(img);
                bestBitmap.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"    ICO parse warning: {ex.Message}");
            Console.ResetColor();
        }
    }
    
    // If no images from ICO parsing, try loading as regular image
    if (images.Count == 0)
    {
        try
        {
            using var ms = new MemoryStream(bytes);
            var image = await SixImage.LoadAsync(ms);
            images.Add(image);
        }
        catch { }
    }
    
    return images;
}
