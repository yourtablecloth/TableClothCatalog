#!/usr/bin/env dotnet
#:property PublishAot=false

using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

if (args.Length > 2)
{
    Console.Error.WriteLine("Usage: dotnet run --file src/testskills.cs -- [schema.xsd] [catalog.xml]");
    return 2;
}

try
{
    var schemas = new XmlSchemaSet { XmlResolver = null };
    var schemaPath = args.Length > 0 ? args[0] : "docs/SkillCatalog.xsd";
    var catalogPath = args.Length > 1 ? args[1] : Path.Combine(Path.GetDirectoryName(Path.GetFullPath(schemaPath))!, "SkillCatalog.xml");
    using var schemaReader = XmlReader.Create(schemaPath,
        new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
    schemas.Add(null, schemaReader);
    schemas.Compile();

    var cases = new (string Path, bool Valid)[]
    {
        (".", true), ("a", true), ("_", true), ("-", true),
        ("skills/example", true), ("a/b", true), ("a.b/c.d", true), ("a..b", true),
        ("skills/example.", false), ("skills/example./nested", false), ("a.", false),
        ("a./b.", false), ("skills/./example", false), ("./example", false),
        ("..", false), ("../example", false), ("skills/../example", false),
        ("", false), ("skills//example", false), ("/example", false), ("skills/", false),
        ("skills\\example", false), ("skills/%2e%2e/example", false), ("skills/a:stream", false),
    };
    var failures = 0;
    foreach (var (path, expected) in cases)
    {
        var fixture = Fixture();
        fixture.Element("Skill")!.Element("Source")!.SetAttributeValue("Path", path);
        if (Accepts(fixture, schemas) == expected) continue;
        failures++;
        Console.Error.WriteLine($"Path '{path}': expected {(expected ? "acceptance" : "rejection")}.");
    }
    var lifecycle = new (string Name, Action<XElement> Change, bool Valid)[]
    {
        ("active entry without review attributes", _ => { }, true),
        ("removed entry as XML comment", xml => xml.Element("Skill")!.ReplaceWith(new XComment(xml.Element("Skill")!.ToString())), true),
        ("legacy listing status", xml => xml.Element("Skill")!.SetAttributeValue("Status", "listed"), false),
        ("legacy license review", xml => xml.Element("Skill")!.Element("License")!.SetAttributeValue("ReviewStatus", "pending"), false),
        ("legacy installation review", xml => xml.Element("Skill")!.Element("Install")!.SetAttributeValue("ReviewStatus", "pending"), false),
        ("MCP adapter without a skill", xml => xml.Element("Skill")!.Element("Install")!.SetAttributeValue("Mode", "adapter"), false),
        ("missing immutable source", xml => xml.Element("Skill")!.Element("Source")!.Attribute("Ref")!.Remove(), false),
        ("mutable source branch", xml => xml.Element("Skill")!.Element("Source")!.SetAttributeValue("Ref", "main"), false),
        ("missing license source", xml => xml.Element("Skill")!.Element("License")!.Attribute("Source")!.Remove(), false),
        ("duplicate skill ID", xml => xml.Add(new XElement(xml.Element("Skill")!)), false),
        ("updated pinned source", xml =>
        {
            var version = new string('2', 40);
            xml.Element("Skill")!.Element("Source")!.SetAttributeValue("Ref", version);
            xml.Element("Skill")!.Element("License")!.SetAttributeValue("Source", "https://github.com/fixture/skills/blob/" + version + "/LICENSE");
        }, true),
    };
    foreach (var (name, change, expected) in lifecycle)
    {
        var fixture = Fixture();
        change(fixture);
        if (Accepts(fixture, schemas) == expected) continue;
        failures++;
        Console.Error.WriteLine($"Lifecycle '{name}': expected {(expected ? "acceptance" : "rejection")}.");
    }
    var catalogText = File.ReadAllText(catalogPath);
    if (!AcceptsXml(catalogText, schemas, out var originalCount))
        throw new XmlException("The catalog fixture must pass schema validation before removal checks.");
    var removals = 0;
    foreach (var (id, start, length) in RawSkillSpans(catalogText))
    {
        removals++;
        var commented = catalogText[..start] + "<!--\n" + catalogText.Substring(start, length) + "\n-->" + catalogText[(start + length)..];
        if (AcceptsXml(commented, schemas, out var remaining) && remaining == originalCount - 1) continue;
        failures++;
        Console.Error.WriteLine($"Removal '{id}': commenting out the raw Skill block must remove exactly one valid entry.");
    }
    if (removals != originalCount) throw new XmlException("Could not locate every active raw Skill block.");
    if (failures != 0) return 1;
    Console.WriteLine($"All {cases.Length + lifecycle.Length + removals} path, lifecycle and raw-block removal checks passed.");
    return 0;
}
catch (Exception e) when (e is XmlException or XmlSchemaException or IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine($"Regression validation failed: {e.Message}");
    return 1;
}

static XElement Fixture() => XElement.Parse("""
    <SkillCatalog Version="1">
      <Skill Id="fixture" Category="Test" DisplayName="Fixture">
        <Description>Path and lifecycle validation fixture.</Description>
        <Source Repository="https://github.com/fixture/skills" Path="." Ref="1111111111111111111111111111111111111111" />
        <License SPDX="MIT" Source="https://github.com/fixture/skills/blob/1111111111111111111111111111111111111111/LICENSE" />
        <Requirements Runtime="Codex" Network="required" />
        <Install Mode="upstream" />
      </Skill>
    </SkillCatalog>
    """);

static bool Accepts(XElement fixture, XmlSchemaSet schemas) => AcceptsXml(fixture.ToString(), schemas, out _);

static bool AcceptsXml(string fixture, XmlSchemaSet schemas, out int activeCount)
{
    activeCount = 0;
    var valid = true;
    var settings = new XmlReaderSettings
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        ValidationType = ValidationType.Schema,
        ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints |
            XmlSchemaValidationFlags.ReportValidationWarnings,
        Schemas = schemas,
    };
    settings.ValidationEventHandler += (_, _) => valid = false;
    try
    {
        using var reader = XmlReader.Create(new StringReader(fixture), settings);
        var catalog = XDocument.Load(reader);
        activeCount = catalog.Root?.Elements("Skill").Count() ?? 0;
        return valid;
    }
    catch (Exception e) when (e is XmlException or XmlSchemaException) { return false; }
}

static IReadOnlyList<(string Id, int Start, int Length)> RawSkillSpans(string xml)
{
    var lines = new List<int> { 0 };
    for (var index = 0; index < xml.Length; index++)
    {
        if (xml[index] == '\r')
        {
            if (index + 1 < xml.Length && xml[index + 1] == '\n') index++;
            lines.Add(index + 1);
        }
        else if (xml[index] == '\n') lines.Add(index + 1);
    }
    var spans = new List<(string, int, int)>();
    using var reader = XmlReader.Create(new StringReader(xml),
        new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
    var start = -1;
    var id = "";
    while (reader.Read())
    {
        if (reader.Depth != 1 || reader.Name != "Skill") continue;
        var line = (IXmlLineInfo)reader;
        var position = lines[line.LineNumber - 1] + line.LinePosition - 1;
        var tagStart = xml.LastIndexOf('<', position);
        if (reader.NodeType == XmlNodeType.Element)
        {
            start = tagStart;
            id = reader.GetAttribute("Id") ?? "";
        }
        else if (reader.NodeType == XmlNodeType.EndElement && start >= 0)
        {
            spans.Add((id, start, xml.IndexOf('>', tagStart) + 1 - start));
            start = -1;
        }
    }
    return spans;
}
