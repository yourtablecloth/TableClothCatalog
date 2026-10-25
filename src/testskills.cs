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
    var current = XDocument.Parse(catalogText).Root!;
    if (!MatchesPocEvaluationSources(current))
    {
        failures++;
        Console.Error.WriteLine("PoC eligibility: the actual catalog contains an ID or pinned source outside this evaluation branch.");
    }
    var qualifiedFixture = ProductionFixture();
    var eligibility = new (string Name, Action<XElement> Change, bool Valid)[]
    {
        ("qualified pinned catalog", _ => { }, true),
        ("removal of a qualified skill", xml => xml.Elements("Skill").First().Remove(), true),
        ("all qualified skills removed", xml => xml.RemoveNodes(), true),
        ("pinned calculator PoC evaluation only", xml => xml.Add(PinnedSkill("tablecloth-calculators", "https://github.com/yourtablecloth/tablecloth-calculators", ".", "877b58cf0741db21342445f2b3fcb54ad1ada85c")), true),
        ("pinned BOK PoC evaluation only", xml => xml.Add(PinnedSkill("bok-ecos-stats", "https://github.com/NomaDamas/k-skill", "bok-ecos-stats", "0102e5133865f00886e37ac8cf39a49e2c7a3d39")), true),
        ("pinned KOSIS PoC evaluation only", xml => xml.Add(PinnedSkill("kosis-stats", "https://github.com/NomaDamas/k-skill", "kosis-stats", "0102e5133865f00886e37ac8cf39a49e2c7a3d39")), true),
        ("unconnected MCP reactivation", xml => xml.Add(PinnedSkill("korean-law-mcp", "https://github.com/chrisryugj/korean-law-mcp", "skills/korean-law", "4ccedb0dc89fce164493ffc9019b5ad35cb9b782")), false),
        ("credential-dependent candidate remains deferred", xml => xml.Add(PinnedSkill("k-dart", "https://github.com/NomaDamas/k-skill", "k-dart", "0102e5133865f00886e37ac8cf39a49e2c7a3d39")), false),
        ("unqualified skill insertion", xml =>
        {
            var added = new XElement(xml.Elements("Skill").First());
            added.SetAttributeValue("Id", "unqualified-skill");
            xml.Add(added);
        }, false),
        ("source update without a matching qualification", xml =>
        {
            var skill = xml.Elements("Skill").First();
            var source = skill.Element("Source")!;
            var version = new string('2', 40);
            source.SetAttributeValue("Ref", version);
            skill.Element("License")!.SetAttributeValue("Source", (string?)source.Attribute("Repository") + "/blob/" + version + "/LICENSE");
        }, false),
    };
    foreach (var (name, change, expected) in eligibility)
    {
        var fixture = new XElement(qualifiedFixture);
        change(fixture);
        if (Accepts(fixture, schemas) && MatchesPocEvaluationSources(fixture) == expected) continue;
        failures++;
        Console.Error.WriteLine($"Eligibility '{name}': expected {(expected ? "acceptance" : "rejection")}.");
    }
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
    Console.WriteLine($"All {cases.Length + lifecycle.Length + eligibility.Length + 1 + removals} path, lifecycle, eligibility and raw-block removal checks passed.");
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

// The public catalog additionally allows three pinned scripts for hands-on isolated evaluation.
// Inclusion here does not qualify script execution for production TableCloth chat.
static bool MatchesPocEvaluationSources(XElement catalog)
{
    var qualified = PocEvaluationSources();
    return catalog.Elements("Skill").All(skill =>
    {
        if (!qualified.TryGetValue((string?)skill.Attribute("Id") ?? "", out var expected)) return false;
        var source = skill.Element("Source");
        return (string?)source?.Attribute("Repository") == expected.Repository &&
            (string?)source?.Attribute("Path") == expected.Path &&
            string.Equals((string?)source?.Attribute("Ref"), expected.Ref, StringComparison.OrdinalIgnoreCase);
    });
}

static Dictionary<string, (string Repository, string Path, string Ref)> ProductionSources() =>
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["official-report-skillset"] = ("https://github.com/mouseco/k-gov-skills", "skills/official-report-skillset", "a55816b9b44e8391576204a095bf6337940d80f3"),
        ["tablecloth-certificate-expiry"] = ("https://github.com/yourtablecloth/tablecloth-certificate-expiry", ".", "9d2eca6431d1cba34bc872b80fc55fa71fa2dcb2"),
        ["tablecloth-windows-sandbox"] = ("https://github.com/yourtablecloth/tablecloth-windows-sandbox", ".", "b9e52bcd303624dbba81e7de66a3f56344e922fe"),
    };

static Dictionary<string, (string Repository, string Path, string Ref)> PocEvaluationSources()
{
    var sources = ProductionSources();
    sources.Add("bok-ecos-stats", ("https://github.com/NomaDamas/k-skill", "bok-ecos-stats", "0102e5133865f00886e37ac8cf39a49e2c7a3d39"));
    sources.Add("kosis-stats", ("https://github.com/NomaDamas/k-skill", "kosis-stats", "0102e5133865f00886e37ac8cf39a49e2c7a3d39"));
    sources.Add("tablecloth-calculators", ("https://github.com/yourtablecloth/tablecloth-calculators", ".", "877b58cf0741db21342445f2b3fcb54ad1ada85c"));
    return sources;
}

static XElement ProductionFixture()
{
    var catalog = new XElement("SkillCatalog", new XAttribute("Version", "1"));
    foreach (var (id, source) in ProductionSources())
        catalog.Add(PinnedSkill(id, source.Repository, source.Path, source.Ref));
    return catalog;
}

static XElement PinnedSkill(string id, string repository, string path, string version)
{
    var skill = new XElement(Fixture().Element("Skill")!);
    skill.SetAttributeValue("Id", id);
    var source = skill.Element("Source")!;
    source.SetAttributeValue("Repository", repository);
    source.SetAttributeValue("Path", path);
    source.SetAttributeValue("Ref", version);
    skill.Element("License")!.SetAttributeValue("Source", repository + "/blob/" + version + "/LICENSE");
    return skill;
}

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
