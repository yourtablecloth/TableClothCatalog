#!/usr/bin/env dotnet-script

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

const string schemaUrl = "http://yourtablecloth.app/TableClothCatalog/Catalog.xsd";

var path = Args.ElementAtOrDefault(0);

if (string.Equals(path, "--help", StringComparison.OrdinalIgnoreCase))
{
	Console.Out.WriteLine("Usage: <Program Path> <XML File Path>");
	Environment.Exit(0);
	return;
}

if (!File.Exists(path))
{
	Console.Error.WriteLine($"Error: Selected file path `{path}` does not exists.");
	Environment.Exit(1);
	return;
}

var config = new XmlReaderSettings()
{
	ValidationType = ValidationType.Schema,
	ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings | XmlSchemaValidationFlags.ProcessSchemaLocation,
};

config.Schemas.Add(null, schemaUrl);

var warningCount = 0;
var errorCount = 0;

config.ValidationEventHandler += new ValidationEventHandler((_sender, _e) =>
{
	var lineInfo = _sender as IXmlLineInfo;
	var positionText = "Unknown Position";

	if (lineInfo != null)
		positionText = $"Line: {lineInfo.LineNumber}, Pos: {lineInfo.LinePosition}";

	if (_e.Severity == XmlSeverityType.Warning)
	{
		warningCount++;
		Console.WriteLine("Warning: " + _e.Message + " - " + positionText);
	}
	else if (_e.Severity == XmlSeverityType.Error)
	{
		errorCount++;
		Console.WriteLine("Error: " + _e.Message + " - " + positionText);
	}
});

// Get the XmlReader object with the configured settings.
using (XmlReader reader = XmlReader.Create(path, config))
{
	// Parsing the file will cause the validation to occur.
	while (reader.Read()) ;
}

if (errorCount + warningCount < 1)
{
	Console.Out.WriteLine("Success: No XML warnings or errors found.");
	Environment.Exit(0);
	return;
}
else if (errorCount < 1 && warningCount > 0)
{
	Console.Out.WriteLine("Warning: Some XML warnings found.");
	Environment.Exit(0);
	return;
}
else
{
	Console.Error.WriteLine("Error: One or more XML errors or warnings found.");
	Environment.Exit(1);
	return;
}
