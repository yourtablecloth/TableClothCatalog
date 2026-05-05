#!/usr/bin/env dotnet
#:package Microsoft.Playwright@1.59.0
#:property PublishAot=false

using Microsoft.Playwright;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

Console.OutputEncoding = Encoding.UTF8;

if (args.Length == 0)
    return PrintUsage();

return args[0].ToLowerInvariant() switch
{
    "probe" => await RunProbeAsync(args.Skip(1).ToArray()),
    "triage" => await RunTriageAsync(args.Skip(1).ToArray()),
    "issue" => await RunIssueAsync(args.Skip(1).ToArray()),
    "help" or "-h" or "--help" => PrintUsage(),
    var unknown => UnknownSubcommand(unknown),
};

static int PrintUsage()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("Site Health Checker for TableCloth Catalog");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --file src/checksites.cs -- probe  <catalog-dir> <output-dir> [options]");
    Console.WriteLine("  dotnet run --file src/checksites.cs -- triage <report-dir>              [options]");
    Console.WriteLine("  dotnet run --file src/checksites.cs -- issue  <report-dir>              [options]");
    Console.WriteLine();
    Console.WriteLine("probe options:");
    Console.WriteLine("  --only <id1,id2,...>   Only check these Service Ids");
    Console.WriteLine("  --concurrency <N>      Max parallel pages (default: 4)");
    Console.WriteLine("  --timeout-ms <N>       Per-page navigation timeout (default: 30000)");
    Console.WriteLine();
    Console.WriteLine("triage options:");
    Console.WriteLine("  --all                  Walk every entry, not just NeedsReview");
    Console.WriteLine("  --no-browser           Don't auto-open the live URL in the system browser");
    Console.WriteLine();
    Console.WriteLine("issue options:");
    Console.WriteLine("  --dry-run              Print issue title/body without calling gh");
    Console.WriteLine("  --repo <owner/name>    Pass through to gh (default: gh's current repo)");
    Console.WriteLine("  --label <label>        Pass through to gh (e.g. site-health)");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run --file src/checksites.cs -- probe ./docs/ ./health-report/");
    Console.WriteLine("  dotnet run --file src/checksites.cs -- triage ./health-report/2026-05-05T143000Z/");
    Console.WriteLine("  dotnet run --file src/checksites.cs -- issue ./health-report/2026-05-05T143000Z/ --dry-run");
    return 0;
}

static int UnknownSubcommand(string name)
{
    Console.Error.WriteLine($"Unknown subcommand: {name}");
    Console.Error.WriteLine("Run with --help for usage.");
    return 2;
}

static async Task<int> RunProbeAsync(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("probe: <catalog-dir> and <output-dir> are required");
        return 2;
    }

    var catalogDir = args[0];
    var outputBase = args[1];
    var onlyRaw = ParseOption(args, "--only");
    var only = onlyRaw?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var concurrency = int.TryParse(ParseOption(args, "--concurrency"), out var c) && c > 0 ? c : 4;
    var timeoutMs = int.TryParse(ParseOption(args, "--timeout-ms"), out var t) && t > 0 ? t : 30_000;

    if (!Directory.Exists(catalogDir))
    {
        Console.Error.WriteLine($"probe: catalog directory not found: {catalogDir}");
        return 2;
    }

    Console.WriteLine($"[probe] catalog     : {Path.GetFullPath(catalogDir)}");
    Console.WriteLine($"[probe] only        : {(only is null ? "(all)" : string.Join(", ", only))}");
    Console.WriteLine($"[probe] concurrency : {concurrency}");
    Console.WriteLine($"[probe] timeout     : {timeoutMs}ms");

    var entries = LoadCatalog(catalogDir, only);
    var fromCatalog = entries.Count(e => e.Source == "Catalog.xml");
    var fromSites = entries.Count(e => e.Source == "sites.xml");
    Console.WriteLine($"[probe] entries     : {entries.Count} (Catalog.xml: {fromCatalog}, sites.xml: {fromSites})");
    Console.WriteLine();

    if (entries.Count == 0)
    {
        Console.Error.WriteLine("[probe] no entries to check — exiting");
        return 1;
    }

    if (await EnsurePlaywrightAsync() is var installCode and not 0)
        return installCode;

    var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHHmmssZ");
    var outputDir = Path.Combine(outputBase, timestamp);
    var screenshotsDir = Path.Combine(outputDir, "screenshots");
    Directory.CreateDirectory(screenshotsDir);
    Console.WriteLine($"[probe] output      : {Path.GetFullPath(outputDir)}");
    Console.WriteLine();

    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
    {
        Headless = true,
        Channel = "msedge",
    });

    var results = new List<ProbeResult>();
    var consoleLock = new object();
    var done = 0;

    await Parallel.ForEachAsync(
        entries,
        new ParallelOptions { MaxDegreeOfParallelism = concurrency },
        async (entry, ct) =>
        {
            var raw = await ProbeWithRetryAsync(browser, entry, timeoutMs, screenshotsDir);
            var (tier, signals) = Classify(raw);
            var result = raw with { Tier = tier, Signals = signals };
            var seq = Interlocked.Increment(ref done);
            lock (consoleLock)
            {
                results.Add(result);
                var status = result.Error is not null
                    ? $"ERR  {result.Error}"
                    : $"HTTP {result.HttpStatus?.ToString() ?? "???"}";
                var sig = result.Signals.Length > 0 ? "  [" + string.Join(", ", result.Signals) + "]" : "";
                WriteTierBadge(result.Tier);
                Console.WriteLine($" [{seq,3}/{entries.Count}] {result.Id,-40} {status}{sig}");
            }
        });

    Console.WriteLine();
    var byTier = results.GroupBy(r => r.Tier).ToDictionary(g => g.Key, g => g.Count());
    Console.WriteLine($"[probe] done: {results.Count} entries probed");
    Console.WriteLine($"[probe] tiers: AutoOk={byTier.GetValueOrDefault("AutoOk", 0)}, NeedsReview={byTier.GetValueOrDefault("NeedsReview", 0)}, AutoDead={byTier.GetValueOrDefault("AutoDead", 0)}");

    var sortedResults = results
        .OrderBy(r => TierSortRank(r.Tier))
        .ThenBy(r => r.Category, StringComparer.OrdinalIgnoreCase)
        .ThenBy(r => r.Id, StringComparer.OrdinalIgnoreCase)
        .ToList();

    var report = new ProbeReport(
        SchemaVersion: 1,
        ProbedAt: DateTimeOffset.UtcNow,
        CatalogRoot: Path.GetFullPath(catalogDir),
        Summary: new ProbeSummary(
            Total: results.Count,
            AutoOk: byTier.GetValueOrDefault("AutoOk", 0),
            NeedsReview: byTier.GetValueOrDefault("NeedsReview", 0),
            AutoDead: byTier.GetValueOrDefault("AutoDead", 0)),
        Entries: sortedResults);

    var reportJsonPath = Path.Combine(outputDir, "report.json");
    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    await File.WriteAllTextAsync(reportJsonPath, JsonSerializer.Serialize(report, jsonOptions));

    var reportMdPath = Path.Combine(outputDir, "report.md");
    await File.WriteAllTextAsync(reportMdPath, BuildMarkdownReport(report));

    Console.WriteLine($"[probe] report.json: {reportJsonPath}");
    Console.WriteLine($"[probe] report.md  : {reportMdPath}");
    Console.WriteLine($"[probe] screenshots: {Path.GetFullPath(screenshotsDir)}");
    return 0;
}

static int TierSortRank(string tier) => tier switch
{
    "AutoDead" => 0,
    "NeedsReview" => 1,
    "AutoOk" => 2,
    _ => 3,
};

static string BuildMarkdownReport(ProbeReport report)
{
    var sb = new StringBuilder();
    sb.AppendLine("# 식탁보 카탈로그 사이트 헬스 리포트");
    sb.AppendLine();
    sb.AppendLine($"- 생성: {report.ProbedAt:yyyy-MM-ddTHH:mm:ssZ}");
    sb.AppendLine($"- 카탈로그: `{report.CatalogRoot}`");
    sb.AppendLine($"- 합계: 전체 **{report.Summary.Total}** / AutoOk **{report.Summary.AutoOk}** / NeedsReview **{report.Summary.NeedsReview}** / AutoDead **{report.Summary.AutoDead}**");
    sb.AppendLine();

    void EmitTier(string tier, string heading)
    {
        var entries = report.Entries.Where(e => e.Tier == tier).ToList();
        sb.AppendLine($"## {heading} ({entries.Count})");
        sb.AppendLine();
        if (entries.Count == 0)
        {
            sb.AppendLine("_없음_");
            sb.AppendLine();
            return;
        }
        foreach (var grouping in entries.GroupBy(e => e.Category).OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine($"### {grouping.Key} ({grouping.Count()})");
            sb.AppendLine();
            foreach (var e in grouping.OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase))
            {
                var status = e.Error is not null ? $"ERR `{e.Error}`" : $"HTTP `{e.HttpStatus}`";
                sb.AppendLine($"- **{e.Id}** — {e.DisplayName}");
                sb.AppendLine($"  - 원래: `{e.Url}`");
                if (!string.Equals(e.Url, e.FinalUrl, StringComparison.Ordinal))
                    sb.AppendLine($"  - 최종: `{e.FinalUrl}`");
                sb.AppendLine($"  - 상태: {status}");
                if (!string.IsNullOrWhiteSpace(e.Title))
                    sb.AppendLine($"  - 타이틀: {e.Title}");
                if (e.Signals.Length > 0)
                    sb.AppendLine($"  - 시그널: `{string.Join("`, `", e.Signals)}`");
                if (e.ScreenshotPath is not null)
                    sb.AppendLine($"  - 스크린샷: ![{e.Id}]({e.ScreenshotPath})");
                sb.AppendLine();
            }
        }
    }

    EmitTier("AutoDead", "사망 추정 (AutoDead)");
    EmitTier("NeedsReview", "검토 필요 (NeedsReview)");
    EmitTier("AutoOk", "자동 OK (AutoOk)");

    return sb.ToString();
}

static async Task<ProbeResult> ProbeWithRetryAsync(IBrowser browser, CatalogEntry entry, int timeoutMs, string screenshotsDir)
{
    var first = await ProbeOneAsync(browser, entry, timeoutMs, screenshotsDir);
    if (first.Error is "timeout" or "connection-reset" or "connection-refused" or "connection-timeout" or "aborted")
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        return await ProbeOneAsync(browser, entry, timeoutMs, screenshotsDir);
    }
    return first;
}

static async Task<ProbeResult> ProbeOneAsync(IBrowser browser, CatalogEntry entry, int timeoutMs, string screenshotsDir)
{
    var probedAt = DateTimeOffset.UtcNow;
    var finalUrl = entry.Url;
    int? status = null;
    string? title = null;
    string? error = null;
    string? screenshotRel = null;

    IBrowserContext? context = null;
    IPage? page = null;
    try
    {
        context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0",
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 },
            IgnoreHTTPSErrors = true,
            Locale = "ko-KR",
            TimezoneId = "Asia/Seoul",
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["sec-ch-ua"] = "\"Microsoft Edge\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"",
                ["sec-ch-ua-mobile"] = "?0",
                ["sec-ch-ua-platform"] = "\"Windows\"",
                ["sec-ch-ua-platform-version"] = "\"15.0.0\"",
                ["sec-ch-ua-arch"] = "\"x86\"",
                ["sec-ch-ua-bitness"] = "\"64\"",
            },
        });
        await context.AddInitScriptAsync(@"
            Object.defineProperty(navigator, 'platform', { get: () => 'Win32', configurable: true });
            const fakeUAData = {
                brands: [
                    { brand: 'Microsoft Edge', version: '131' },
                    { brand: 'Chromium', version: '131' },
                    { brand: 'Not_A Brand', version: '24' }
                ],
                mobile: false,
                platform: 'Windows',
                getHighEntropyValues: (hints) => Promise.resolve({
                    architecture: 'x86',
                    bitness: '64',
                    model: '',
                    platform: 'Windows',
                    platformVersion: '15.0.0',
                    uaFullVersion: '131.0.0.0',
                    fullVersionList: [
                        { brand: 'Microsoft Edge', version: '131.0.0.0' },
                        { brand: 'Chromium', version: '131.0.0.0' },
                        { brand: 'Not_A Brand', version: '24.0.0.0' }
                    ],
                    wow64: false
                }),
                toJSON: function () { return { brands: this.brands, mobile: this.mobile, platform: this.platform }; }
            };
            Object.defineProperty(navigator, 'userAgentData', { get: () => fakeUAData, configurable: true });
        ");
        page = await context.NewPageAsync();

        var response = await page.GotoAsync(entry.Url, new PageGotoOptions
        {
            Timeout = timeoutMs,
            WaitUntil = WaitUntilState.Load,
        });

        if (response is not null)
            status = response.Status;
        finalUrl = page.Url;
        try { title = await page.TitleAsync(); } catch { }
        if (string.IsNullOrWhiteSpace(title))
        {
            try { await page.WaitForTimeoutAsync(1500); } catch { }
            try { title = await page.TitleAsync(); } catch { }
            try { finalUrl = page.Url; } catch { }
        }

        var safeId = SanitizeId(entry.Id);
        var screenshotPath = Path.Combine(screenshotsDir, $"{safeId}.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = false });
        screenshotRel = $"screenshots/{safeId}.png";
    }
    catch (TimeoutException)
    {
        error = "timeout";
    }
    catch (PlaywrightException ex)
    {
        error = ClassifyPlaywrightError(ex.Message);
    }
    catch (Exception ex)
    {
        error = $"unknown: {ex.GetType().Name}";
    }
    finally
    {
        if (page is not null) try { await page.CloseAsync(); } catch { }
        if (context is not null) try { await context.CloseAsync(); } catch { }
    }

    return new ProbeResult(
        Id: entry.Id,
        DisplayName: entry.DisplayName,
        Category: entry.Category,
        Source: entry.Source,
        Url: entry.Url,
        FinalUrl: finalUrl,
        HttpStatus: status,
        Title: title,
        Error: error,
        ScreenshotPath: screenshotRel,
        Tier: "Unclassified",
        Signals: Array.Empty<string>(),
        ProbedAt: probedAt);
}

static string ClassifyPlaywrightError(string message)
{
    if (message.Contains("ERR_NAME_NOT_RESOLVED")) return "dns";
    if (message.Contains("ERR_CONNECTION_REFUSED")) return "connection-refused";
    if (message.Contains("ERR_CONNECTION_RESET")) return "connection-reset";
    if (message.Contains("ERR_CONNECTION_TIMED_OUT")) return "connection-timeout";
    if (message.Contains("ERR_CERT") || message.Contains("SSL") || message.Contains("TLS")) return "ssl-error";
    if (message.Contains("ERR_ABORTED")) return "aborted";
    if (message.Contains("Timeout") || message.Contains("timeout")) return "timeout";
    if (message.Contains("ERR_TOO_MANY_REDIRECTS")) return "too-many-redirects";
    return "playwright-error";
}

static string SanitizeId(string id) =>
    string.Concat(id.Select(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_'));

static (string tier, string[] signals) Classify(ProbeResult r)
{
    if (r.Error is not null)
    {
        if (r.Error is "dns" or "connection-refused")
            return ("AutoDead", [$"error:{r.Error}"]);
        return ("NeedsReview", [$"error:{r.Error}"]);
    }

    var strong = new List<string>();
    var weak = new List<string>();

    if (r.HttpStatus is null)
        strong.Add("no-http-response");
    else if (r.HttpStatus >= 400)
        strong.Add($"http-{r.HttpStatus}");

    if (Uri.TryCreate(r.Url, UriKind.Absolute, out var origUri) &&
        Uri.TryCreate(r.FinalUrl, UriKind.Absolute, out var finalUri))
    {
        var origDomain = RegistrableDomain(origUri.Host);
        var finalDomain = RegistrableDomain(finalUri.Host);
        if (!string.Equals(origDomain, finalDomain, StringComparison.OrdinalIgnoreCase))
            strong.Add($"off-domain-redirect:{finalDomain}");
    }

    var title = r.Title ?? "";
    string[] closureKeywords = [
        "서비스 종료", "서비스가 종료", "운영이 종료", "종료되었습니다",
        "통합되었습니다", "통합 운영", "이전되었습니다", "변경되었습니다",
        "페이지를 찾을 수 없", "Page Not Found", "Not Found",
    ];
    foreach (var kw in closureKeywords)
        if (title.Contains(kw, StringComparison.OrdinalIgnoreCase))
            strong.Add($"closure-keyword:{kw}");

    if (string.IsNullOrWhiteSpace(r.Title))
        weak.Add("empty-title");

    var all = strong.Concat(weak).ToArray();
    return strong.Count > 0
        ? ("NeedsReview", all)
        : ("AutoOk", all);
}

static string RegistrableDomain(string host)
{
    if (string.IsNullOrEmpty(host)) return host;
    var parts = host.ToLowerInvariant().Split('.');
    if (parts.Length <= 2) return string.Join('.', parts);
    var last = parts[^1];
    var second = parts[^2];
    var krSecondLevel = last == "kr" && second is "co" or "go" or "or" or "ac" or "re" or "ne" or "pe" or "mil" or "es" or "hs" or "ms" or "sc" or "kg" or "seoul" or "busan" or "daegu" or "incheon" or "gwangju" or "daejeon" or "ulsan" or "gyeonggi" or "gangwon" or "chungbuk" or "chungnam" or "jeonbuk" or "jeonnam" or "gyeongbuk" or "gyeongnam" or "jeju";
    return krSecondLevel
        ? string.Join('.', parts.TakeLast(3))
        : string.Join('.', parts.TakeLast(2));
}

static void WriteTierBadge(string tier)
{
    var (text, color) = tier switch
    {
        "AutoOk" => ("OK", ConsoleColor.Green),
        "NeedsReview" => ("??", ConsoleColor.Yellow),
        "AutoDead" => ("XX", ConsoleColor.Red),
        _ => ("--", ConsoleColor.Gray),
    };
    Console.ForegroundColor = color;
    Console.Write(text);
    Console.ResetColor();
}

static async Task<int> RunTriageAsync(string[] args)
{
    if (args.Length < 1)
    {
        Console.Error.WriteLine("triage: <report-dir> is required");
        return 2;
    }

    var reportDir = args[0];
    var walkAll = args.Contains("--all", StringComparer.OrdinalIgnoreCase);
    var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);

    if (!Directory.Exists(reportDir))
    {
        Console.Error.WriteLine($"triage: report directory not found: {reportDir}");
        return 2;
    }

    var reportPath = Path.Combine(reportDir, "report.json");
    if (!File.Exists(reportPath))
    {
        Console.Error.WriteLine($"triage: report.json not found in {reportDir}");
        return 2;
    }

    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    var report = JsonSerializer.Deserialize<ProbeReport>(await File.ReadAllTextAsync(reportPath), jsonOptions)!;
    var triagePath = Path.Combine(reportDir, "triage.json");

    TriageState state;
    if (File.Exists(triagePath))
    {
        state = JsonSerializer.Deserialize<TriageState>(await File.ReadAllTextAsync(triagePath), jsonOptions)!;
        var existingIds = new HashSet<string>(state.Entries.Select(e => e.Probe.Id));
        foreach (var probe in report.Entries)
            if (!existingIds.Contains(probe.Id))
                state.Entries.Add(new TriageEntry(probe, null));
        Console.WriteLine($"[triage] resuming — existing verdicts: {state.Entries.Count(e => e.Verdict is not null)}");
    }
    else
    {
        state = new TriageState(
            SchemaVersion: 1,
            CatalogRoot: report.CatalogRoot,
            ProbedAt: report.ProbedAt,
            UpdatedAt: DateTimeOffset.UtcNow,
            Entries: report.Entries.Select(e => new TriageEntry(e, null)).ToList());
    }

    var queue = state.Entries
        .Where(e => e.Verdict is null)
        .Where(e => walkAll || e.Probe.Tier == "NeedsReview")
        .ToList();

    Console.WriteLine($"[triage] report dir : {Path.GetFullPath(reportDir)}");
    Console.WriteLine($"[triage] mode       : {(walkAll ? "all entries" : "NeedsReview only")}");
    Console.WriteLine($"[triage] queue      : {queue.Count} entries to review");
    Console.WriteLine();

    if (queue.Count == 0)
    {
        Console.WriteLine("[triage] nothing to do — exiting");
        return 0;
    }

    Console.WriteLine("Verdict keys: [o]k / [d]ead / [c]hanged-url / [m]erged-or-renamed / [u]nsure / [s]kip / [q]uit");
    Console.WriteLine("After verdict, you can add a free-form note. Empty input on the prompt skips the entry.");
    Console.WriteLine();

    var quit = false;
    for (var i = 0; i < queue.Count && !quit; i++)
    {
        var entry = queue[i];
        var p = entry.Probe;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"=== [{i + 1}/{queue.Count}] {p.Id} — {p.DisplayName} ===");
        Console.ResetColor();
        Console.WriteLine($"  카테고리   : {p.Category} ({p.Source})");
        Console.Write("  Tier      : ");
        WriteTierBadge(p.Tier);
        Console.WriteLine($"  ({p.Tier})");
        if (p.Signals.Length > 0)
            Console.WriteLine($"  Signals   : {string.Join(", ", p.Signals)}");
        Console.WriteLine($"  원래 URL  : {p.Url}");
        if (!string.Equals(p.Url, p.FinalUrl, StringComparison.Ordinal))
            Console.WriteLine($"  최종 URL  : {p.FinalUrl}");
        if (p.Error is not null)
            Console.WriteLine($"  Error     : {p.Error}");
        else
            Console.WriteLine($"  Status    : HTTP {p.HttpStatus}");
        if (!string.IsNullOrWhiteSpace(p.Title))
            Console.WriteLine($"  Title     : {p.Title}");
        if (p.ScreenshotPath is not null)
            Console.WriteLine($"  Screenshot: {Path.Combine(reportDir, p.ScreenshotPath)}");
        Console.WriteLine();

        if (!noBrowser)
        {
            Console.WriteLine("  Live URL을 시스템 브라우저로 엽니다 (직접 클릭해서 확인하세요)...");
            OpenInSystemBrowser(p.Url);
        }
        else
        {
            Console.WriteLine($"  Live URL: {p.Url}  (--no-browser, 직접 열어서 확인하세요)");
        }
        Console.WriteLine();

        Console.Write("  Verdict [o/d/c/m/u/s/q]: ");
        var input = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("  (skipped — no input)");
            Console.WriteLine();
            continue;
        }

        var key = char.ToLowerInvariant(input[0]);
        if (key == 'q') { quit = true; break; }
        if (key == 's')
        {
            Console.WriteLine("  (skipped)");
            Console.WriteLine();
            continue;
        }

        string? kind = key switch
        {
            'o' => "healthy",
            'd' => "dead",
            'c' => "url-changed",
            'm' => "merged-or-renamed",
            'u' => "unsure",
            _ => null,
        };

        if (kind is null)
        {
            Console.WriteLine($"  (unknown key '{key}' — skipped)");
            Console.WriteLine();
            continue;
        }

        string? newUrl = null;
        if (kind == "url-changed")
        {
            Console.Write("  새 URL: ");
            newUrl = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(newUrl))
            {
                Console.WriteLine("  (URL 없음 — verdict 취소, skipped)");
                Console.WriteLine();
                continue;
            }
        }

        string? successorId = null;
        if (kind == "merged-or-renamed")
        {
            Console.Write("  후속 기관 Id (선택, 엔터로 건너뛰기): ");
            successorId = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(successorId)) successorId = null;
        }

        Console.Write("  메모 (선택, 엔터로 건너뛰기): ");
        var notes = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(notes)) notes = null;

        var verdict = new TriageVerdict(
            Kind: kind,
            NewUrl: newUrl,
            SuccessorId: successorId,
            Notes: notes,
            VerdictAt: DateTimeOffset.UtcNow);

        var idx = state.Entries.FindIndex(e => e.Probe.Id == p.Id);
        state.Entries[idx] = entry with { Verdict = verdict };
        state = state with { UpdatedAt = DateTimeOffset.UtcNow };

        await File.WriteAllTextAsync(triagePath, JsonSerializer.Serialize(state, jsonOptions));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✔ saved ({kind})");
        Console.ResetColor();
        Console.WriteLine();
    }

    var verdicted = state.Entries.Count(e => e.Verdict is not null);
    var pending = state.Entries.Count(e => e.Verdict is null);
    Console.WriteLine($"[triage] complete — verdicts: {verdicted}, pending: {pending}");
    Console.WriteLine($"[triage] file: {triagePath}");
    return 0;
}

static async Task<int> RunIssueAsync(string[] args)
{
    if (args.Length < 1)
    {
        Console.Error.WriteLine("issue: <report-dir> is required");
        return 2;
    }

    var reportDir = args[0];
    var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
    var repo = ParseOption(args, "--repo");
    var label = ParseOption(args, "--label");

    if (!Directory.Exists(reportDir))
    {
        Console.Error.WriteLine($"issue: report directory not found: {reportDir}");
        return 2;
    }

    var triagePath = Path.Combine(reportDir, "triage.json");
    if (!File.Exists(triagePath))
    {
        Console.Error.WriteLine($"issue: triage.json not found in {reportDir} — run 'triage' first");
        return 2;
    }

    if (!dryRun)
    {
        var auth = await RunProcessAsync("gh", ["auth", "status"]);
        if (auth.ExitCode != 0)
        {
            Console.Error.WriteLine("issue: gh CLI is not authenticated. Run 'gh auth login' first.");
            Console.Error.WriteLine(auth.Stderr);
            return 2;
        }
    }

    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    var state = JsonSerializer.Deserialize<TriageState>(await File.ReadAllTextAsync(triagePath), jsonOptions)!;

    var qualifying = state.Entries.Where(ShouldFileIssue).ToList();
    var alreadyFiled = qualifying.Where(e => !string.IsNullOrEmpty(e.Verdict?.IssueUrl)).ToList();
    var pending = qualifying.Where(e => string.IsNullOrEmpty(e.Verdict?.IssueUrl)).ToList();

    Console.WriteLine($"[issue] report dir       : {Path.GetFullPath(reportDir)}");
    Console.WriteLine($"[issue] qualifying       : {qualifying.Count}");
    Console.WriteLine($"[issue] already filed    : {alreadyFiled.Count}");
    Console.WriteLine($"[issue] to file          : {pending.Count}");
    if (dryRun)
        Console.WriteLine($"[issue] mode             : dry-run (no issues will be created)");
    if (!string.IsNullOrEmpty(repo))
        Console.WriteLine($"[issue] target repo      : {repo}");
    if (!string.IsNullOrEmpty(label))
        Console.WriteLine($"[issue] label            : {label}");
    Console.WriteLine();

    if (pending.Count == 0)
    {
        Console.WriteLine("[issue] nothing to file — exiting");
        return 0;
    }

    foreach (var entry in pending)
    {
        var p = entry.Probe;
        var title = BuildIssueTitle(entry);
        var body = BuildIssueBody(entry, reportDir);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"=== {p.Id} ===");
        Console.ResetColor();
        Console.WriteLine($"  Title : {title}");

        if (dryRun)
        {
            Console.WriteLine($"  Body  :");
            foreach (var line in body.Split('\n'))
                Console.WriteLine($"    {line.TrimEnd('\r')}");
            Console.WriteLine();
            continue;
        }

        var ghArgs = new List<string> { "issue", "create", "--title", title, "--body-file", "-" };
        if (!string.IsNullOrEmpty(repo)) { ghArgs.Add("--repo"); ghArgs.Add(repo); }
        if (!string.IsNullOrEmpty(label)) { ghArgs.Add("--label"); ghArgs.Add(label); }

        var result = await RunProcessAsync("gh", ghArgs.ToArray(), stdin: body);
        if (result.ExitCode != 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ✗ failed (exit {result.ExitCode})");
            Console.ResetColor();
            if (!string.IsNullOrWhiteSpace(result.Stderr)) Console.Error.WriteLine($"    {result.Stderr.Trim()}");
            continue;
        }
        var url = result.Stdout.Trim();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✔ {url}");
        Console.ResetColor();

        var idx = state.Entries.FindIndex(e => e.Probe.Id == p.Id);
        var newVerdict = entry.Verdict! with { IssueUrl = url };
        state.Entries[idx] = entry with { Verdict = newVerdict };
        state = state with { UpdatedAt = DateTimeOffset.UtcNow };
        await File.WriteAllTextAsync(triagePath, JsonSerializer.Serialize(state, jsonOptions));
    }

    Console.WriteLine();
    Console.WriteLine($"[issue] complete");
    return 0;
}

static bool ShouldFileIssue(TriageEntry e)
{
    if (e.Verdict is null) return false;
    return e.Verdict.Kind switch
    {
        "dead" or "merged-or-renamed" or "unsure" => true,
        "url-changed" => IsCrossDomainChange(e),
        _ => false,
    };
}

static bool IsCrossDomainChange(TriageEntry e)
{
    var v = e.Verdict;
    if (v is null || string.IsNullOrEmpty(v.NewUrl)) return false;
    if (!Uri.TryCreate(e.Probe.Url, UriKind.Absolute, out var orig)) return false;
    if (!Uri.TryCreate(v.NewUrl, UriKind.Absolute, out var nw)) return false;
    return !string.Equals(RegistrableDomain(orig.Host), RegistrableDomain(nw.Host), StringComparison.OrdinalIgnoreCase);
}

static string BuildIssueTitle(TriageEntry e)
{
    var label = e.Verdict!.Kind switch
    {
        "dead" => "사이트 폐쇄 의심",
        "merged-or-renamed" => "기관 통폐합/명칭변경 의심",
        "url-changed" => "URL 변경 (도메인 변경)",
        "unsure" => "추가 조사 필요",
        _ => e.Verdict.Kind,
    };
    return $"[사이트 헬스] {e.Probe.Id} — {label}";
}

static string BuildIssueBody(TriageEntry e, string reportDir)
{
    var sb = new StringBuilder();
    var p = e.Probe;
    var v = e.Verdict!;

    sb.AppendLine("## 카탈로그 항목");
    sb.AppendLine();
    sb.AppendLine($"- **Id**: `{p.Id}`");
    sb.AppendLine($"- **DisplayName**: {p.DisplayName}");
    sb.AppendLine($"- **Category**: {p.Category}");
    sb.AppendLine($"- **Source**: `{p.Source}`");
    sb.AppendLine($"- **원래 URL**: {p.Url}");
    if (!string.Equals(p.Url, p.FinalUrl, StringComparison.Ordinal))
        sb.AppendLine($"- **최종 URL**: {p.FinalUrl}");
    if (p.Error is not null)
        sb.AppendLine($"- **Error**: `{p.Error}`");
    else
        sb.AppendLine($"- **HTTP Status**: `{p.HttpStatus}`");
    if (!string.IsNullOrWhiteSpace(p.Title))
        sb.AppendLine($"- **Title**: {p.Title}");
    sb.AppendLine($"- **Tier**: `{p.Tier}`");
    if (p.Signals.Length > 0)
        sb.AppendLine($"- **Signals**: `{string.Join("`, `", p.Signals)}`");
    sb.AppendLine();

    sb.AppendLine("## 사람 판정");
    sb.AppendLine();
    sb.AppendLine($"- **Verdict**: `{v.Kind}`");
    if (!string.IsNullOrEmpty(v.NewUrl))
        sb.AppendLine($"- **NewUrl**: {v.NewUrl}");
    if (!string.IsNullOrEmpty(v.SuccessorId))
        sb.AppendLine($"- **SuccessorId**: `{v.SuccessorId}`");
    if (!string.IsNullOrEmpty(v.Notes))
        sb.AppendLine($"- **Notes**: {v.Notes}");
    sb.AppendLine($"- **VerdictAt**: `{v.VerdictAt:yyyy-MM-ddTHH:mm:ssZ}`");
    sb.AppendLine();

    sb.AppendLine("## 권장 조치");
    sb.AppendLine();
    sb.AppendLine(v.Kind switch
    {
        "dead" => "이 항목은 카탈로그에서 삭제할 후보입니다. 추가 검증 후 PR 제출.",
        "merged-or-renamed" => "후속 기관 정보를 조사한 뒤 (1) URL 교체 (2) 항목 삭제·재등록 (3) 메타데이터(DisplayName/Id) 갱신 중 적절한 방향으로 PR 제출.",
        "url-changed" => "`Url` 속성을 새 URL로 갱신하는 PR을 제출. 도메인이 변경되었으니 같은 기관이 운영하는 사이트가 맞는지 한 번 더 확인.",
        "unsure" => "추가 조사 후 verdict를 갱신하거나 수동으로 처리.",
        _ => "—",
    });
    sb.AppendLine();

    sb.AppendLine("## 출처");
    sb.AppendLine();
    sb.AppendLine($"- ProbedAt: `{p.ProbedAt:yyyy-MM-ddTHH:mm:ssZ}`");
    sb.AppendLine($"- triage.json: `{Path.Combine(reportDir, "triage.json")}`");
    if (p.ScreenshotPath is not null)
        sb.AppendLine($"- Screenshot: `{Path.Combine(reportDir, p.ScreenshotPath)}`");
    sb.AppendLine();
    sb.AppendLine("---");
    sb.AppendLine("*generated by `src/checksites.cs issue`*");

    return sb.ToString();
}

static async Task<ProcessResult> RunProcessAsync(string fileName, string[] args, string? stdin = null)
{
    var psi = new System.Diagnostics.ProcessStartInfo(fileName)
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        RedirectStandardInput = stdin is not null,
        UseShellExecute = false,
    };
    foreach (var arg in args) psi.ArgumentList.Add(arg);

    using var proc = System.Diagnostics.Process.Start(psi)!;
    var stdoutTask = proc.StandardOutput.ReadToEndAsync();
    var stderrTask = proc.StandardError.ReadToEndAsync();
    if (stdin is not null)
    {
        await proc.StandardInput.WriteAsync(stdin);
        proc.StandardInput.Close();
    }
    await proc.WaitForExitAsync();
    return new ProcessResult(proc.ExitCode, await stdoutTask, await stderrTask);
}

static void OpenInSystemBrowser(string url)
{
    try
    {
        if (OperatingSystem.IsMacOS())
            System.Diagnostics.Process.Start("open", url);
        else if (OperatingSystem.IsLinux())
            System.Diagnostics.Process.Start("xdg-open", url);
        else if (OperatingSystem.IsWindows())
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true });
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"  (failed to open browser: {ex.Message})");
    }
}

static async Task<int> EnsurePlaywrightAsync()
{
    Console.WriteLine("[playwright] ensuring Microsoft Edge (stable) is installed (one-time per machine)...");
    var exitCode = await Task.Run(() => Microsoft.Playwright.Program.Main(["install", "msedge"]));
    if (exitCode != 0)
        Console.Error.WriteLine($"[playwright] install failed with exit code {exitCode}");
    return exitCode;
}

static string? ParseOption(string[] args, string name)
{
    for (var i = 0; i < args.Length - 1; i++)
        if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            return args[i + 1];
    return null;
}

static List<CatalogEntry> LoadCatalog(string catalogDir, string[]? onlyIds)
{
    var entries = new List<CatalogEntry>();
    var catalogPath = Path.Combine(catalogDir, "Catalog.xml");
    var sitesPath = Path.Combine(catalogDir, "sites.xml");

    if (File.Exists(catalogPath))
    {
        var doc = XDocument.Load(catalogPath);
        foreach (var s in doc.Descendants("Service"))
        {
            var id = (string?)s.Attribute("Id");
            var url = (string?)s.Attribute("Url");
            var name = (string?)s.Attribute("DisplayName") ?? id ?? "";
            var category = (string?)s.Attribute("Category") ?? "";
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(url))
            {
                Console.Error.WriteLine($"[catalog] skipped Service with missing Id/Url: Id='{id}' Url='{url}'");
                continue;
            }
            entries.Add(new CatalogEntry(id!.Trim(), name.Trim(), category.Trim(), url!.Trim(), "Catalog.xml"));
        }
    }
    else
    {
        Console.Error.WriteLine($"[catalog] Catalog.xml not found at {catalogPath}");
    }

    if (File.Exists(sitesPath))
    {
        var doc = XDocument.Load(sitesPath);
        foreach (var s in doc.Descendants("site"))
        {
            var host = ((string?)s.Attribute("url"))?.Trim();
            if (string.IsNullOrWhiteSpace(host)) continue;
            var id = "IEMode_" + host.Replace('.', '_').Replace('/', '_').Replace(':', '_');
            var url = host.Contains("://") ? host : $"https://{host}";
            entries.Add(new CatalogEntry(id, host, "IEMode", url, "sites.xml"));
        }
    }
    else
    {
        Console.Error.WriteLine($"[catalog] sites.xml not found at {sitesPath} (skipping)");
    }

    Console.WriteLine($"[catalog] loaded     : {entries.Count} (Catalog.xml: {entries.Count(e => e.Source == "Catalog.xml")}, sites.xml: {entries.Count(e => e.Source == "sites.xml")})");

    if (onlyIds is { Length: > 0 })
    {
        var set = new HashSet<string>(onlyIds, StringComparer.OrdinalIgnoreCase);
        var filtered = entries.Where(e => set.Contains(e.Id)).ToList();
        var matched = new HashSet<string>(filtered.Select(e => e.Id), StringComparer.OrdinalIgnoreCase);
        var missing = onlyIds.Where(id => !matched.Contains(id)).ToList();
        if (missing.Count > 0)
            Console.Error.WriteLine($"[catalog] --only ids not found: {string.Join(", ", missing)}");
        return filtered;
    }

    return entries;
}

public sealed record CatalogEntry(
    string Id,
    string DisplayName,
    string Category,
    string Url,
    string Source);

public sealed record ProbeResult(
    string Id,
    string DisplayName,
    string Category,
    string Source,
    string Url,
    string FinalUrl,
    int? HttpStatus,
    string? Title,
    string? Error,
    string? ScreenshotPath,
    string Tier,
    string[] Signals,
    DateTimeOffset ProbedAt);

public sealed record ProbeSummary(int Total, int AutoOk, int NeedsReview, int AutoDead);

public sealed record ProbeReport(
    int SchemaVersion,
    DateTimeOffset ProbedAt,
    string CatalogRoot,
    ProbeSummary Summary,
    List<ProbeResult> Entries);

public sealed record TriageVerdict(
    string Kind,
    string? NewUrl,
    string? SuccessorId,
    string? Notes,
    DateTimeOffset VerdictAt,
    string? IssueUrl = null);

public sealed record ProcessResult(int ExitCode, string Stdout, string Stderr);

public sealed record TriageEntry(
    ProbeResult Probe,
    TriageVerdict? Verdict);

public sealed record TriageState(
    int SchemaVersion,
    string CatalogRoot,
    DateTimeOffset ProbedAt,
    DateTimeOffset UpdatedAt,
    List<TriageEntry> Entries);
