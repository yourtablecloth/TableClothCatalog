#!/usr/bin/env dotnet-script

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

var path = Args.ElementAtOrDefault(0);

const string schemaUrl = "http://yourtablecloth.app/TableClothCatalog/Catalog.xsd";
const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.164 Safari/537.36";

var getPositionInfo = (IXmlLineInfo lineInfo) =>
{
	var positionText = "Unknown Position";

	if (lineInfo != null)
		positionText = $"Line: {lineInfo.LineNumber}, Line Position: {lineInfo.LinePosition}";

	return positionText;
};

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
	ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings,
};

if (!string.IsNullOrWhiteSpace(schemaUrl))
{
	Console.Out.WriteLine($"Info: Using schema `{schemaUrl}`.");
	config.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
	config.ValidationType = ValidationType.Schema;
	config.Schemas.Add(null, schemaUrl);
}

var warningCount = 0;
var errorCount = 0;

config.ValidationEventHandler += new ValidationEventHandler((_sender, _e) =>
{
	if (_e.Severity == XmlSeverityType.Warning)
	{
		warningCount++;
		Console.Error.WriteLine($"Warning: {_e.Message} - {getPositionInfo(_sender as IXmlLineInfo)}");
	}
	else if (_e.Severity == XmlSeverityType.Error)
	{
		errorCount++;
		Console.Error.WriteLine($"Error: {_e.Message} - {getPositionInfo(_sender as IXmlLineInfo)}");
	}
});

var httpClient = new HttpClient();

if (!string.IsNullOrWhiteSpace(userAgent))
{
	Console.Out.WriteLine($"Info: Using User Agent String `{userAgent}`.");
	httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
}

Console.Out.WriteLine($"Info: Validating `{path}` file.");

// Get the XmlReader object with the configured settings.
using (XmlReader reader = XmlReader.Create(path, config))
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
						Console.Error.WriteLine($"Error: [{reader.Name}] URL string cannot be empty - {getPositionInfo(reader as IXmlLineInfo)}");
						continue;
					}

					if (!Uri.TryCreate(urlString, UriKind.Absolute, out Uri result) ||
						result == null)
					{
						errorCount++;
						Console.Error.WriteLine($"Error: Cannot parse the URI `{urlString}` - {getPositionInfo(reader as IXmlLineInfo)}");
						continue;
					}

					var response = await httpClient.GetAsync(result, HttpCompletionOption.ResponseHeadersRead);

					if (response == null || !response.IsSuccessStatusCode)
					{
						errorCount++;
						Console.Error.WriteLine($"Error: Cannot fetch the URI `{urlString}` (Remote response code: {(int?)response?.StatusCode}) - {getPositionInfo(reader as IXmlLineInfo)}");
						continue;
					}
					break;
			}
		}
		catch (Exception ex)
		{
			errorCount++;
			Console.Error.WriteLine($"Error: {ex.GetType().Name} / {ex.InnerException?.GetType().Name} throwed. ({ex.InnerException?.Message ?? ex.Message}) - {getPositionInfo(reader as IXmlLineInfo)}");
			continue;
		}
	}
}

if (errorCount + warningCount < 1)
	Console.Out.WriteLine("Success: No XML warnings or errors found.");
else if (errorCount < 1 && warningCount > 0)
	Console.Error.WriteLine("Warning: Some XML warnings found.");
else
	Console.Error.WriteLine("Error: One or more XML errors or warnings found.");
