#!/usr/bin/env dotnet
#:property PublishAot=false

using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

if (args.Length > 2)
{
    Console.Error.WriteLine("Usage: dotnet run --file src/checkskills.cs -- [catalog.xml] [schema.xsd]");
    return 2;
}

var catalogPath = args.Length > 0 ? args[0] : "docs/SkillCatalog.xml";
var schemaPath = args.Length > 1 ? args[1] : "docs/SkillCatalog.xsd";
var errors = 0;

try
{
    if (new FileInfo(catalogPath).Length > 512 * 1024)
        throw new XmlException("The skill catalog exceeds the 512 KiB limit.");
    var settings = new XmlReaderSettings
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        ValidationType = ValidationType.Schema,
        MaxCharactersInDocument = 512 * 1024,
        ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints |
            XmlSchemaValidationFlags.ReportValidationWarnings,
    };
    settings.ValidationEventHandler += (_, e) =>
    {
        errors++;
        Console.Error.WriteLine($"{e.Severity}: {e.Message}");
    };

    var schemaSettings = new XmlReaderSettings
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
    };
    using var schemaReader = XmlReader.Create(schemaPath, schemaSettings);
    settings.Schemas.XmlResolver = null;
    settings.Schemas.Add(null, schemaReader);
    using var catalogReader = XmlReader.Create(catalogPath, settings);
    var catalog = XDocument.Load(catalogReader);
    if (errors != 0) return 1;
    var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var skill in catalog.Root!.Elements("Skill"))
    {
        var id = (string?)skill.Attribute("Id") ?? "";
        if (!ids.Add(id)) throw new XmlException("Skill IDs must be unique without regard to case.");
        var source = skill.Element("Source")!;
        var prefix = (string?)source.Attribute("Repository") + "/blob/" + (string?)source.Attribute("Ref") + "/";
        var license = (string?)skill.Element("License")?.Attribute("Source") ?? "";
        if (!license.StartsWith(prefix, StringComparison.Ordinal) || !SafePath(license[prefix.Length..]))
            throw new XmlException($"Skill '{id}' must pin its license to the same repository and commit.");
        if (!SafePath((string?)source.Attribute("Path") ?? "", allowRoot: true))
            throw new XmlException($"Skill '{id}' has an unsupported repository path.");
    }
}
catch (Exception e) when (e is XmlException or XmlSchemaException or IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine($"Validation failed: {e.Message}");
    return 1;
}

if (errors != 0)
    return 1;

Console.WriteLine("Skill catalog schema validation passed.");
return 0;

static bool SafePath(string path, bool allowRoot = false)
{
    if (allowRoot && path == ".") return true;
    return path.Length is > 0 and <= 160 && path.Split('/').All(segment =>
    {
        var stem = segment.Split('.')[0];
        var device = stem.Equals("CON", StringComparison.OrdinalIgnoreCase) || stem.Equals("PRN", StringComparison.OrdinalIgnoreCase) ||
            stem.Equals("AUX", StringComparison.OrdinalIgnoreCase) || stem.Equals("NUL", StringComparison.OrdinalIgnoreCase) ||
            stem.Length == 4 && (stem.StartsWith("COM", StringComparison.OrdinalIgnoreCase) || stem.StartsWith("LPT", StringComparison.OrdinalIgnoreCase)) &&
            char.IsAsciiDigit(stem[3]);
        return segment.Length is > 0 and <= 100 && segment is not ("." or "..") && !segment.EndsWith('.') && !device &&
            segment.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.');
    });
}
