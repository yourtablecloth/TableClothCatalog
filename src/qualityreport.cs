#!/usr/bin/env dotnet
#:property PublishAot=false

// 카탈로그 품질 리포트 생성기 (진화형).
//
// checkimages.cs / catalogutil.cs 가 내보낸 JSON을 읽어, 이전 상태(state)와 병합하여
// "이번 주 신규 / 계속 남아있음(경과 주차) / 이번에 해결됨"을 추적하는 단일 리포트를 만듭니다.
// 검출은 자동, 실제 카탈로그 수정은 사람 또는 AI 에이전트가 담당합니다.
//
// 사용법:
//   dotnet run --file src/qualityreport.cs -- \
//     <checkimages.json> <catalogutil.json> <prev-state.json> <out-report.md> <out-state.json> [github-output]
//
// - 문제를 "안정적 키"로 식별합니다(라인 번호가 아니라 Category/Id·정규 메시지). 카탈로그를
//   한 줄 편집해도 라인 번호가 밀려 전부 신규/해결로 오검출되는 문제를 피합니다.
// - URL 접속 경고는 CI 일시 장애가 섞일 수 있어 "참고"로만 집계하며, 리포트 개폐 상태
//   (has_problems)에는 영향을 주지 않습니다.

using System.Text;
using System.Text.Json;

var checkImagesJsonPath = args.ElementAtOrDefault(0);
var catalogUtilJsonPath = args.ElementAtOrDefault(1);
var prevStatePath = args.ElementAtOrDefault(2);
var outReportPath = args.ElementAtOrDefault(3);
var outStatePath = args.ElementAtOrDefault(4);
var githubOutputPath = args.ElementAtOrDefault(5);

if (string.IsNullOrWhiteSpace(checkImagesJsonPath) ||
    string.IsNullOrWhiteSpace(outReportPath) ||
    string.IsNullOrWhiteSpace(outStatePath))
{
    Console.Error.WriteLine(
        "Usage: dotnet run --file src/qualityreport.cs -- " +
        "<checkimages.json> <catalogutil.json> <prev-state.json> <out-report.md> <out-state.json> [github-output]");
    return 1;
}

var today = DateTime.UtcNow.Date;
var todayStr = today.ToString("yyyy-MM-dd");
var nowStr = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

// ---------- 1) 현재 문제 집합(키 부여) ----------
var current = new List<Problem>();

if (File.Exists(checkImagesJsonPath))
{
    using var doc = JsonDocument.Parse(File.ReadAllText(checkImagesJsonPath));
    var root = doc.RootElement;

    if (root.TryGetProperty("missing", out var missing) && missing.ValueKind == JsonValueKind.Array)
        foreach (var m in missing.EnumerateArray())
        {
            var cat = GetStr(m, "category");
            var id = GetStr(m, "id");
            var name = GetStr(m, "displayName");
            var label = $"{cat}/{id}" + (string.IsNullOrEmpty(name) ? "" : $" ({name})");
            current.Add(new Problem($"image-missing:{cat}/{id}", "image-missing", label));
        }

    if (root.TryGetProperty("unused", out var unused) && unused.ValueKind == JsonValueKind.Array)
        foreach (var u in unused.EnumerateArray())
        {
            var cat = GetStr(u, "category");
            var img = GetStr(u, "imageName");
            current.Add(new Problem($"image-unused:{cat}/{img}", "image-unused", $"{cat}/{img}.png"));
        }
}

var urlWarnings = new List<string>();
if (!string.IsNullOrWhiteSpace(catalogUtilJsonPath) && File.Exists(catalogUtilJsonPath))
{
    using var doc = JsonDocument.Parse(File.ReadAllText(catalogUtilJsonPath));
    var root = doc.RootElement;

    if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
        foreach (var e in errors.EnumerateArray())
        {
            var msg = GetStr(e, "message");
            var pos = GetStr(e, "position");
            // 키는 메시지 본문(안정적). 위치(라인 번호)는 표시용 라벨에만 포함.
            var label = msg + (string.IsNullOrEmpty(pos) ? "" : $" ({pos})");
            current.Add(new Problem($"schema-error:{msg.Trim()}", "schema-error", label));
        }

    if (root.TryGetProperty("warnings", out var warnings) && warnings.ValueKind == JsonValueKind.Array)
        foreach (var w in warnings.EnumerateArray())
            urlWarnings.Add(GetStr(w, "message"));
}

// 키 중복 제거
current = current.GroupBy(p => p.Key).Select(g => g.First()).ToList();

// ---------- 2) 이전 상태 로드 ----------
var prevItems = new Dictionary<string, StateItem>(StringComparer.Ordinal);
if (!string.IsNullOrWhiteSpace(prevStatePath) && File.Exists(prevStatePath))
{
    try
    {
        var prev = JsonSerializer.Deserialize<StateFile>(File.ReadAllText(prevStatePath));
        if (prev?.items != null)
            foreach (var it in prev.items)
                if (!string.IsNullOrEmpty(it.key))
                    prevItems[it.key] = it;
    }
    catch
    {
        // 손상/구버전 → 신규 상태로 간주
    }
}

var currentKeys = current.Select(p => p.Key).ToHashSet(StringComparer.Ordinal);

var newItems = current.Where(p => !prevItems.ContainsKey(p.Key)).ToList();
var persisting = current.Where(p => prevItems.ContainsKey(p.Key)).ToList();
var resolved = prevItems.Values.Where(it => !currentKeys.Contains(it.key)).ToList();

// ---------- 3) 새 상태 구성 (해결된 항목은 제거, 지속 항목은 firstSeen 승계) ----------
var newState = new StateFile { schema = 1, generatedAt = nowStr, items = new List<StateItem>() };
foreach (var p in current)
{
    var firstSeen = prevItems.TryGetValue(p.Key, out var prevIt) && !string.IsNullOrEmpty(prevIt.firstSeen)
        ? prevIt.firstSeen
        : todayStr;
    newState.items.Add(new StateItem { key = p.Key, group = p.Group, label = p.Label, firstSeen = firstSeen });
}
newState.items = newState.items.OrderBy(i => i.firstSeen, StringComparer.Ordinal)
                               .ThenBy(i => i.key, StringComparer.Ordinal).ToList();

// ---------- 4) 카운트 ----------
var schemaErrors = current.Count(p => p.Group == "schema-error");
var imgMissing = current.Count(p => p.Group == "image-missing");
var imgUnused = current.Count(p => p.Group == "image-unused");
var hasProblems = current.Count > 0; // URL 경고는 상태에 미포함(비차단)

string Age(string firstSeen)
{
    if (DateTime.TryParse(firstSeen, out var fs))
    {
        var weeks = (int)Math.Floor((today - fs.Date).TotalDays / 7) + 1;
        return $"최초 {firstSeen}, {weeks}주째";
    }
    return $"최초 {firstSeen}";
}

// ---------- 5) 리포트 렌더링 ----------
var sb = new StringBuilder();
sb.AppendLine("<!-- catalog-quality-report -->");
sb.AppendLine("# 카탈로그 품질 리포트");
sb.AppendLine();
sb.AppendLine($"- **생성(UTC)**: {nowStr}");
sb.AppendLine($"- **상태**: {(hasProblems ? $"문제 {current.Count}건" : "문제 없음 ✅")}");
sb.AppendLine();
sb.AppendLine("이 리포트는 `catalog-quality` 워크플로가 주 1회 자동 생성하며, 이전 리포트와 병합하여 " +
             "신규/지속/해결을 추적합니다. 검출은 자동, 실제 카탈로그 수정은 사람 또는 AI 에이전트가 담당합니다.");
sb.AppendLine();
sb.AppendLine("### 요약");
sb.AppendLine();
sb.AppendLine("| 유형 | 건수 |");
sb.AppendLine("|---|---:|");
sb.AppendLine($"| 스키마/구조 오류 (must-fix) | {schemaErrors} |");
sb.AppendLine($"| 이미지 누락 | {imgMissing} |");
sb.AppendLine($"| 미사용 이미지 | {imgUnused} |");
sb.AppendLine($"| URL 접속 경고 (참고, 비차단) | {urlWarnings.Count} |");
sb.AppendLine();

if (schemaErrors > 0)
{
    sb.AppendLine("### ⛔ 스키마/구조 오류 — 즉시 수정 필요");
    sb.AppendLine();
    foreach (var p in current.Where(p => p.Group == "schema-error").OrderBy(p => p.Key, StringComparer.Ordinal))
        sb.AppendLine($"- {p.Label}");
    sb.AppendLine();
}

sb.AppendLine($"### 🆕 이번 주 신규 ({newItems.Count})");
sb.AppendLine();
if (newItems.Count == 0)
    sb.AppendLine("_(없음)_");
else
    foreach (var p in newItems.OrderBy(p => p.Group, StringComparer.Ordinal).ThenBy(p => p.Key, StringComparer.Ordinal))
        sb.AppendLine($"- `{p.Group}` {p.Label}");
sb.AppendLine();

sb.AppendLine($"### ⏳ 계속 남아있음 ({persisting.Count}) — 오래된 순");
sb.AppendLine();
if (persisting.Count == 0)
    sb.AppendLine("_(없음)_");
else
    foreach (var x in persisting
        .Select(p => new { p, fs = prevItems[p.Key].firstSeen ?? todayStr })
        .OrderBy(x => x.fs, StringComparer.Ordinal).ThenBy(x => x.p.Key, StringComparer.Ordinal))
        sb.AppendLine($"- `{x.p.Group}` {x.p.Label}  ({Age(x.fs)})");
sb.AppendLine();

sb.AppendLine($"### ✅ 이번에 해결됨 ({resolved.Count})");
sb.AppendLine();
if (resolved.Count == 0)
    sb.AppendLine("_(없음)_");
else
    foreach (var it in resolved.OrderBy(i => i.key, StringComparer.Ordinal))
        sb.AppendLine($"- `{it.group}` {it.label}  (해결: {todayStr})");
sb.AppendLine();

if (urlWarnings.Count > 0)
{
    sb.AppendLine($"<details><summary>URL 접속 경고 {urlWarnings.Count}건 (CI 일시 장애 포함 가능, 리포트 상태에 영향 없음)</summary>");
    sb.AppendLine();
    foreach (var w in urlWarnings)
        sb.AppendLine($"- {w}");
    sb.AppendLine();
    sb.AppendLine("</details>");
    sb.AppendLine();
}

sb.AppendLine("### 권장 조치");
sb.AppendLine();
sb.AppendLine("- **이미지 누락**: 공식 로고를 `docs/images/<Category>/<Id>.png`로 추가 (`src/fetchfavicon.cs` 참고).");
sb.AppendLine("- **미사용 이미지**: 삭제된 서비스의 잔여 이미지면 함께 제거.");
sb.AppendLine("- **스키마/구조 오류**: `Catalog.xml`을 `Catalog.xsd`에 맞게 수정.");
sb.AppendLine();
sb.AppendLine("재검증:");
sb.AppendLine();
sb.AppendLine("```bash");
sb.AppendLine("dotnet run --file src/checkimages.cs -- ./docs/Catalog.xml ./docs/images");
sb.AppendLine("dotnet run --file src/catalogutil.cs -- ./docs/ ./outputs/");
sb.AppendLine("```");
sb.AppendLine();
sb.AppendLine("---");
sb.AppendLine("*generated by `.github/workflows/catalog-quality.yml` + `src/qualityreport.cs`. 상태 파일: `quality/state.json`*");

File.WriteAllText(outReportPath, sb.ToString());
File.WriteAllText(outStatePath, JsonSerializer.Serialize(newState, new JsonSerializerOptions { WriteIndented = true }));

// ---------- 6) 워크플로용 출력 ----------
var outLines = new[]
{
    $"has_problems={(hasProblems ? "true" : "false")}",
    $"new_count={newItems.Count}",
    $"persisting_count={persisting.Count}",
    $"resolved_count={resolved.Count}",
    $"schema_errors={schemaErrors}",
    $"image_missing={imgMissing}",
    $"image_unused={imgUnused}",
    $"url_warnings={urlWarnings.Count}",
};
if (!string.IsNullOrWhiteSpace(githubOutputPath))
    File.AppendAllLines(githubOutputPath, outLines);
foreach (var l in outLines)
    Console.WriteLine(l);

return 0;

static string GetStr(JsonElement e, string prop) =>
    e.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

record Problem(string Key, string Group, string Label);

class StateFile
{
    public int schema { get; set; } = 1;
    public string generatedAt { get; set; } = "";
    public List<StateItem> items { get; set; } = new();
}

class StateItem
{
    public string key { get; set; } = "";
    public string group { get; set; } = "";
    public string label { get; set; } = "";
    public string firstSeen { get; set; } = "";
}
