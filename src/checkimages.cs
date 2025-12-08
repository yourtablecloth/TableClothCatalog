#!/usr/bin/env dotnet
#:property PublishAot=false

using System.Xml.Linq;

// Process command line arguments
var catalogPath = args.ElementAtOrDefault(0);
var imagesBasePath = args.ElementAtOrDefault(1);

// Validate required arguments
if (string.IsNullOrEmpty(catalogPath) || string.IsNullOrEmpty(imagesBasePath))
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("Catalog.xml Service Image Validation Tool");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine("Usage: dotnet run checkimages.cs <catalogPath> <imagesBasePath>");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  catalogPath     Path to the Catalog.xml file");
    Console.WriteLine("  imagesBasePath  Path to the images base directory");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine("  dotnet run .\\src\\checkimages.cs .\\docs\\Catalog.xml .\\docs\\images");
    return 1;
}

// Normalize paths
catalogPath = Path.GetFullPath(catalogPath);
imagesBasePath = Path.GetFullPath(imagesBasePath);

// Load XML file
if (!File.Exists(catalogPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: Catalog.xml file not found: {catalogPath}");
    Console.ResetColor();
    return 1;
}

var catalog = XDocument.Load(catalogPath);
var ns = catalog.Root?.Name.Namespace ?? XNamespace.None;

// Extract service information (Id, DisplayName, Category are all attributes)
var services = catalog.Descendants(ns + "Service")
    .Where(s => s.Parent?.Name.LocalName == "InternetServices")
    .Select(s => new ServiceInfo
    {
        Id = s.Attribute("Id")?.Value ?? string.Empty,
        DisplayName = s.Attribute("DisplayName")?.Value ?? string.Empty,
        Category = s.Attribute("Category")?.Value ?? string.Empty
    })
    .Where(s => !string.IsNullOrEmpty(s.Id) && !string.IsNullOrEmpty(s.Category))
    .ToList();

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=== Catalog.xml Service Image Validation ===");
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine($"Total services: {services.Count}");
Console.ResetColor();

// Check image existence
var validImages = new List<ServiceInfo>();
var missingImages = new List<ServiceInfo>();

foreach (var service in services)
{
    var expectedImagePath = Path.Combine(imagesBasePath, service.Category, $"{service.Id}.png");
    service.ExpectedImagePath = expectedImagePath;
    
    if (File.Exists(expectedImagePath))
    {
        validImages.Add(service);
    }
    else
    {
        missingImages.Add(service);
    }
}

// Find unused images
var categories = new[] { "Banking", "CreditCard", "Education", "Financing", "Government", "Insurance", "Other", "Security" };
var allServiceIds = services.Select(s => s.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
var extraImages = new List<ExtraImageInfo>();

foreach (var category in categories)
{
    var categoryPath = Path.Combine(imagesBasePath, category);
    if (!Directory.Exists(categoryPath)) continue;

    var imagesInFolder = Directory.GetFiles(categoryPath, "*.png")
        .Select(f => Path.GetFileNameWithoutExtension(f))
        .Where(name => !string.IsNullOrEmpty(name));

    foreach (var imageName in imagesInFolder)
    {
        if (!allServiceIds.Contains(imageName))
        {
            extraImages.Add(new ExtraImageInfo
            {
                ImageName = imageName,
                Category = category,
                FilePath = Path.Combine(categoryPath, $"{imageName}.png")
            });
        }
    }
}

// Output missing images
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("--- Missing Images (service exists but image not found) ---");
Console.ResetColor();

if (missingImages.Count == 0)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("  All services have images!");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  Missing images: {missingImages.Count}");
    Console.ResetColor();

    var groupedMissing = missingImages.GroupBy(s => s.Category).OrderBy(g => g.Key);
    foreach (var group in groupedMissing)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"  [{group.Key}] ({group.Count()})");
        Console.ResetColor();

        foreach (var item in group)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"    - {item.Id} ({item.DisplayName})");
            Console.ResetColor();
        }
    }
}

// Output unused images
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("--- Unused Images (image exists but no matching service) ---");
Console.ResetColor();

if (extraImages.Count == 0)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("  All images are in use!");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine($"  Unused images: {extraImages.Count}");
    Console.ResetColor();

    var groupedExtra = extraImages.GroupBy(e => e.Category).OrderBy(g => g.Key);
    foreach (var group in groupedExtra)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"  [{group.Key}] ({group.Count()})");
        Console.ResetColor();

        foreach (var item in group)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"    - {item.ImageName}");
            Console.ResetColor();
        }
    }
}

// Output summary
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=== Summary ===");
Console.ResetColor();

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine($"  Total services: {services.Count}");
Console.ResetColor();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"  Images found: {validImages.Count}");
Console.ResetColor();

Console.ForegroundColor = missingImages.Count > 0 ? ConsoleColor.Red : ConsoleColor.Green;
Console.WriteLine($"  Images missing: {missingImages.Count}");
Console.ResetColor();

Console.ForegroundColor = extraImages.Count > 0 ? ConsoleColor.DarkYellow : ConsoleColor.Green;
Console.WriteLine($"  Unused images: {extraImages.Count}");
Console.ResetColor();

return missingImages.Count > 0 || extraImages.Count > 0 ? 1 : 0;

// Model classes
class ServiceInfo
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ExpectedImagePath { get; set; } = string.Empty;
}

class ExtraImageInfo
{
    public string ImageName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}
