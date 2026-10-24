#!/usr/bin/env dotnet
#:property PublishAot=false

using System.Xml;
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
    var settings = new XmlReaderSettings
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        ValidationType = ValidationType.Schema,
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
    while (catalogReader.Read()) { }
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
