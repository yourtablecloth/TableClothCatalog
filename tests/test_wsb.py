"""WSB generation checks do not download or execute catalog installers."""

import base64
import gzip
import importlib.util
import json
import random
from pathlib import Path
import re
import shutil
import string
import subprocess
import tempfile
import unittest
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location("buildwsb", ROOT / "src" / "buildwsb.py")
wsb = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(wsb)


def decode(content):
    root = ET.fromstring(content)
    command = root.findtext("./LogonCommand/Command")
    loader = command[len(wsb.COMMAND_PREFIX) + 1:-1]
    pattern = re.escape(wsb.LOADER).replace("__SCRIPT__", r"([A-Za-z0-9+/=]+)")
    compressed = re.fullmatch(pattern, loader).group(1)
    script = gzip.decompress(base64.b64decode(compressed)).decode("utf-8")
    payload = re.search(r"\$encodedData = '([A-Za-z0-9+/=]+)'", script).group(1)
    return root, command, script, json.loads(gzip.decompress(base64.b64decode(payload)))


class WsbTests(unittest.TestCase):
    @unittest.skipUnless(shutil.which("cmd.exe") and shutil.which("powershell.exe"), "Windows command processors are unavailable")
    def test_compressed_loader_runs_through_cmd_with_only_a_benign_fixture(self):
        value = 'WSB_LOADER_OK: 한글 "quotes" & | % ! ^ $(literal)'
        expected = base64.b64encode(value.encode("utf-8")).decode("ascii")
        # Incompressible comments give the harmless fixture a command length
        # comparable to real launchers while testing cmd's quoting and UTF-8.
        generator = random.Random(7)
        comment = "".join(generator.choices(string.ascii_letters + string.digits, k=6500))
        fixture = "#" + comment + "\nif (-not $TableClothGuestSource -or $TableClothGuestSource.Length -lt 6500) { throw 'Source variable did not reach the child scriptblock.' }\n$value = '" + value + "'\nWrite-Output ([Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($value)))\n"
        command = wsb.make_command(fixture)
        self.assertGreater(len(command), 6500)
        self.assertLessEqual(len(command), 8000)
        self.assertTrue(command.isascii())
        with tempfile.TemporaryDirectory() as directory:
            batch = Path(directory) / "benign-loader.cmd"
            batch.write_text("@echo off\r\n" + command + "\r\n", encoding="ascii", newline="")
            result = subprocess.run(
                [shutil.which("cmd.exe"), "/d", "/c", str(batch)],
                text=True, capture_output=True, timeout=30,
            )
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn(expected, result.stdout)

    def test_compressed_loader_rejects_commands_above_the_cmd_budget(self):
        generator = random.Random(8)
        fixture = "#" + "".join(generator.choices(string.ascii_letters + string.digits, k=12000))
        with self.assertRaisesRegex(ValueError, "8000-character"):
            wsb.make_command(fixture)

    def run_guest_fragment(self, body):
        encoded_source = base64.b64encode(wsb.BOOTSTRAP.encode("utf-16-le")).decode("ascii")
        harness = r"""
$ErrorActionPreference = 'Stop'
$source = [Text.Encoding]::Unicode.GetString([Convert]::FromBase64String('__SOURCE__'))
$tokens = $null; $errors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseInput($source,[ref]$tokens,[ref]$errors)
if ($errors) { throw 'Generated script did not parse.' }
$main = $ast.EndBlock.Statements | Where-Object { $_ -is [System.Management.Automation.Language.TryStatementAst] } | Select-Object -Last 1
""".replace("__SOURCE__", encoded_source) + body
        with tempfile.TemporaryDirectory() as directory:
            test_script = Path(directory) / "fragment.ps1"
            test_script.write_text(harness, encoding="ascii")
            return subprocess.run(
                [shutil.which("powershell.exe"), "-NoLogo", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", str(test_script)],
                text=True, capture_output=True, timeout=30,
            )

    @unittest.skipUnless(shutil.which("powershell.exe"), "Windows PowerShell is unavailable")
    def test_guest_elevation_preserves_admin_path_and_writes_only_the_retry_source(self):
        # Only the elevation function runs. Start-Process is replaced before it
        # can request any host permission; the source fixture is never executed.
        result = self.run_guest_fragment(r"""
$function = $ast.EndBlock.Statements | Where-Object { $_ -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $_.Name -eq 'EnsureGuestAdministrator' }
. ([scriptblock]::Create($function.Extent.Text))
$script:admin = $true; $script:launches = 0; $script:retry = $null
function IsGuestAdministrator { return $script:admin }
function Log { param($message) }
function Start-Process {
    param($FilePath,$ArgumentList,$Verb,[switch]$Wait,[switch]$PassThru,$ErrorAction)
    $script:launches++
    if ($Verb -ne 'RunAs' -or -not $Wait -or -not $PassThru -or $ErrorAction -ne 'Stop') { throw 'Incorrect elevation options.' }
    if ($FilePath -ne "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe") { throw 'Incorrect guest PowerShell path.' }
    if ($ArgumentList -notmatch '-File "([^"]+)"$') { throw 'Retry source path was not quoted.' }
    $script:retry = $Matches[1]
    if ([IO.File]::ReadAllText($script:retry,[Text.Encoding]::Unicode) -cne $script:expectedSource) { throw 'Retry source changed.' }
    $bytes = [IO.File]::ReadAllBytes($script:retry)
    if ($bytes[0] -ne 255 -or $bytes[1] -ne 254) { throw 'Retry source is not UTF-16LE with BOM.' }
    return [pscustomobject]@{ExitCode=37}
}
if ($null -ne (EnsureGuestAdministrator $null) -or $script:launches -ne 0) { throw 'An elevated file required source or prompted again.' }
$script:admin = $false
$script:expectedSource = "Write-Output 'benign source fixture'"
if ((EnsureGuestAdministrator $script:expectedSource) -ne 37) { throw 'Elevated exit code was not returned.' }
if ($script:launches -ne 1 -or [IO.File]::Exists($script:retry)) { throw 'Retry source was not cleaned up.' }
Write-Output 'GUEST_ELEVATION_FUNCTION_PASSED'
""")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("GUEST_ELEVATION_FUNCTION_PASSED", result.stdout)

    def run_elevation_main_fixture(self, deny):
        mode = "throw 'UAC_DENIED_FIXTURE'" if deny else "return [pscustomobject]@{ExitCode=37}"
        return self.run_guest_fragment(r"""
$functions = $ast.EndBlock.Statements | Where-Object { $_ -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $_.Name -in @('EnsureGuestAdministrator','AssertInstallerHttps') }
foreach ($function in $functions) { . ([scriptblock]::Create($function.Extent.Text)) }
$script:retry = $null
function IsGuestAdministrator { return $false }
function Log { param($message) Write-Host $message }
function Start-Process {
    param($FilePath,$ArgumentList,$Verb,[switch]$Wait,[switch]$PassThru,$ErrorAction)
    if ($Verb -ne 'RunAs') { throw 'Unexpected app launch.' }
    if ($ArgumentList -notmatch '-File "([^"]+)"$') { throw 'Missing retry path.' }
    $script:retry = $Matches[1]
    if (-not [IO.File]::Exists($script:retry)) { throw 'Source was not written before elevation.' }
    __MODE__
}
function Test-Path { param($LiteralPath) return $false }
function FindEdge { return $null }
function Read-Host { param($Prompt) Write-Host 'ERROR_CONSOLE_REMAINED_OPEN'; return '' }
$site = [pscustomobject]@{Name='Elevation fixture'; Notes=''; Packages=@([pscustomobject]@{Url='https://example.test/never-downloaded.exe'})}
$work = $PSScriptRoot
function Set-Content { param($LiteralPath,$Encoding,$Value) }
function Download { throw 'An installer download must not run in an elevation fixture.' }
$TableClothGuestSource = $source
try { & ([scriptblock]::Create($main.Extent.Text)) }
finally {
    if (-not $script:retry -or [IO.File]::Exists($script:retry)) { Write-Host 'RETRY_SOURCE_CLEANUP_FAILED' }
    else { Write-Host 'RETRY_SOURCE_REMOVED' }
}
""".replace("__MODE__", mode))

    @unittest.skipUnless(shutil.which("powershell.exe"), "Windows PowerShell is unavailable")
    def test_elevated_exit_code_is_propagated_by_the_bootstrap(self):
        result = self.run_elevation_main_fixture(False)
        self.assertEqual(result.returncode, 37, result.stderr)
        self.assertIn("RETRY_SOURCE_REMOVED", result.stdout)
        self.assertNotIn("FAILED:", result.stdout)

    @unittest.skipUnless(shutil.which("powershell.exe"), "Windows PowerShell is unavailable")
    def test_guest_uac_denial_reaches_the_visible_error_handler(self):
        result = self.run_elevation_main_fixture(True)
        self.assertEqual(result.returncode, 1, result.stderr)
        self.assertIn("FAILED: UAC_DENIED_FIXTURE", result.stdout)
        self.assertIn("ERROR_CONSOLE_REMAINED_OPEN", result.stdout)
        self.assertIn("RETRY_SOURCE_REMOVED", result.stdout)

    @unittest.skipUnless(shutil.which("powershell.exe"), "Windows PowerShell is unavailable")
    def test_site_without_installation_work_does_not_request_elevation(self):
        result = self.run_guest_fragment(r"""
function EnsureGuestAdministrator { throw 'A plain site requested elevation.' }
$site = [pscustomobject]@{Packages=@(); Extensions=@(); Bootstrap=''}
$guard = $main.Body.Statements | Where-Object { $_ -is [System.Management.Automation.Language.IfStatementAst] -and $_.Extent.Text -like '*EnsureGuestAdministrator*' }
if (-not $guard) { throw 'Missing elevation guard.' }
& ([scriptblock]::Create($guard.Extent.Text))
Write-Output 'PLAIN_SITE_DID_NOT_ELEVATE'
""")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("PLAIN_SITE_DID_NOT_ELEVATE", result.stdout)

    @unittest.skipUnless(shutil.which("powershell.exe"), "Windows PowerShell is unavailable")
    def test_http_installer_preflight_stops_before_any_download(self):
        # Keep legacy data visible and preserve the generated site's launcher.
        service = ET.fromstring('<Service Id="Test" Url="http://example.test/"><Packages><Package Name="Legacy" Url="http://example.test/setup.exe"/></Packages></Service>')
        data = wsb.service_data(service)
        self.assertEqual(decode(wsb.make_wsb(data))[3]["Packages"][0]["Url"], "http://example.test/setup.exe")
        result = self.run_guest_fragment(r"""
$function = $ast.EndBlock.Statements | Where-Object { $_ -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $_.Name -eq 'AssertInstallerHttps' }
. ([scriptblock]::Create($function.Extent.Text))
$script:downloads = 0
function Log { param($message) }
function Set-Content { param($LiteralPath,$Encoding,$Value) }
function EnsureGuestAdministrator { throw 'Elevation must not precede installer preflight.' }
function Download { $script:downloads++; throw 'A download was attempted.' }
$work = $PSScriptRoot
$site = [pscustomobject]@{Name='Fixture'; Notes=''; Packages=@(
    [pscustomobject]@{Url='https://example.test/first.exe'},
    [pscustomobject]@{Url='http://127.0.0.1:1/blocked.exe'}
)}
$body = $main.Body.Extent.Text
try { & ([scriptblock]::Create($body.Substring(1,$body.Length - 2))); throw 'Preflight unexpectedly succeeded.' }
catch { if ($_.Exception.Message -notlike '*require HTTPS*') { throw } }
if ($script:downloads -ne 0) { throw 'Preflight allowed a download.' }
Write-Output 'HTTP_PREFLIGHT_BLOCKED'
""")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("HTTP_PREFLIGHT_BLOCKED", result.stdout)

    @unittest.skipUnless(shutil.which("powershell.exe"), "Windows PowerShell is unavailable")
    def test_https_redirects_are_checked_before_each_request(self):
        # An in-memory WebRequest provider replaces all HTTPS requests. Nothing
        # contacts the network, changes certificate policy, or runs an installer.
        result = self.run_guest_fragment(r"""
Add-Type -TypeDefinition @'
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
public class FixtureFactory : IWebRequestCreate {
    public static List<string> Seen = new List<string>();
    public static Queue<string> Redirects = new Queue<string>();
    public WebRequest Create(Uri uri) { Seen.Add(uri.ToString()); return new FixtureRequest(); }
}
public class FixtureRequest : WebRequest {
    public bool AllowAutoRedirect { get; set; }
    public int ReadWriteTimeout { get; set; }
    public string UserAgent { get; set; }
    public override int Timeout { get; set; }
    public FixtureRequest() { AllowAutoRedirect = true; }
    public override WebResponse GetResponse() {
        if (AllowAutoRedirect) throw new Exception("Automatic redirects were not disabled.");
        return new FixtureResponse(FixtureFactory.Redirects.Count == 0 ? null : FixtureFactory.Redirects.Dequeue());
    }
}
public class FixtureResponse : WebResponse {
    public HttpStatusCode StatusCode { get; private set; }
    private WebHeaderCollection headers = new WebHeaderCollection();
    public override WebHeaderCollection Headers { get { return headers; } }
    public override long ContentLength { get { return 2; } set {} }
    public FixtureResponse(string location) {
        StatusCode = location == null ? HttpStatusCode.OK : HttpStatusCode.Found;
        if (location != null) headers["Location"] = location;
    }
    public override Stream GetResponseStream() { return new MemoryStream(new byte[] {77,90}); }
    public override void Close() {}
}
'@
if (-not [Net.WebRequest]::RegisterPrefix('https://wsb-fixture.invalid/',(New-Object FixtureFactory))) { throw 'Fixture registration failed.' }
$functions = $ast.EndBlock.Statements | Where-Object { $_ -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $_.Name -in @('AssertInstallerHttps','Download') }
foreach ($function in $functions) { . ([scriptblock]::Create($function.Extent.Text)) }
$path = Join-Path $PSScriptRoot 'fixture.download'
try { Download 'http://127.0.0.1:1/never-requested' $path; throw 'Initial HTTP was accepted.' }
catch { if ($_.Exception.Message -notlike '*require HTTPS*') { throw } }
if ([FixtureFactory]::Seen.Count -ne 0 -or (Test-Path -LiteralPath $path)) { throw 'HTTP caused a request or file write.' }
[FixtureFactory]::Redirects.Enqueue('http://127.0.0.1:1/downgrade')
try { Download 'https://wsb-fixture.invalid/first' $path; throw 'Downgrade was accepted.' }
catch { if ($_.Exception.Message -notlike '*require HTTPS*') { throw } }
if ([FixtureFactory]::Seen.Count -ne 1 -or (Test-Path -LiteralPath $path)) { throw 'Downgrade caused another request or file write.' }
[FixtureFactory]::Seen.Clear()
[FixtureFactory]::Redirects.Enqueue('/relative-target')
Download 'https://wsb-fixture.invalid/first' $path
if ([FixtureFactory]::Seen.Count -ne 2 -or [FixtureFactory]::Seen[1] -ne 'https://wsb-fixture.invalid/relative-target') { throw 'Relative HTTPS redirect failed.' }
if ([IO.File]::ReadAllBytes($path).Length -ne 2) { throw 'Successful download body was not written.' }
[FixtureFactory]::Seen.Clear()
1..6 | ForEach-Object { [FixtureFactory]::Redirects.Enqueue('/again') }
try { Download 'https://wsb-fixture.invalid/first' $path; throw 'Excess redirects were accepted.' }
catch { if ($_.Exception.Message -notlike '*excessive installer redirects*') { throw } }
if ([FixtureFactory]::Seen.Count -ne 6) { throw 'Redirect count exceeded the initial request plus five redirects.' }
Write-Output 'HTTPS_REDIRECT_CHECKS_PASSED'
""")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("HTTPS_REDIRECT_CHECKS_PASSED", result.stdout)

    @unittest.skipUnless(shutil.which("powershell.exe"), "Windows PowerShell is unavailable")
    def test_extension_registration_preserves_multiple_and_existing_values(self):
        # Execute the generated loop against registry cmdlet stubs. New-Item
        # intentionally clears a key just as a force-created key could do.
        result = self.run_guest_fragment(r"""
function Log { param($message) }
$script:keys = @{}
$script:created = 0
function Test-Path { param($LiteralPath) return $script:keys.ContainsKey($LiteralPath) }
function New-Item { param($Path,[switch]$Force) $script:created++; $script:keys[$Path] = @{} }
function Get-ItemProperty {
    param($LiteralPath,$Name,$ErrorAction)
    if ($script:keys[$LiteralPath].ContainsKey([string]$Name)) { return [pscustomobject]@{Value=$script:keys[$LiteralPath][[string]$Name]} }
    return $null
}
function New-ItemProperty {
    param($Path,$Name,$Value,$PropertyType,[switch]$Force)
    if ($script:keys[$Path].ContainsKey([string]$Name) -and -not $Force) { throw 'An existing registry value would be overwritten.' }
    $script:keys[$Path][[string]$Name] = $Value
}
$site = [pscustomobject]@{Extensions=@(
    [pscustomobject]@{Id=('a' * 32); Url='https://clients2.google.com/service/update2/crx'},
    [pscustomobject]@{Id=('b' * 32); Url='https://clients2.google.com/service/update2/crx'}
)}
$loop = $main.Body.Statements | Where-Object { $_ -is [System.Management.Automation.Language.ForEachStatementAst] -and $_.Variable.VariablePath.UserPath -eq 'extension' }
$registryPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Edge\ExtensionInstallForcelist'
$number = 0; $extensionLinks = @()
. ([scriptblock]::Create($loop.Extent.Text))
if ($script:created -ne 1 -or $script:keys[$registryPath].Count -ne 2) { throw 'Multiple extensions did not survive key creation.' }
if ($script:keys[$registryPath]['1'] -notlike 'aaaaaaaa*' -or $script:keys[$registryPath]['2'] -notlike 'bbbbbbbb*') { throw 'An extension value was lost.' }
$script:keys[$registryPath] = @{'1'='existing-first'; '9'='existing-ninth'}
$number = 0; $extensionLinks = @()
. ([scriptblock]::Create($loop.Extent.Text))
if ($script:created -ne 1 -or $script:keys[$registryPath].Count -ne 4) { throw 'The existing registry key was reset.' }
if ($script:keys[$registryPath]['1'] -ne 'existing-first' -or $script:keys[$registryPath]['9'] -ne 'existing-ninth') { throw 'An existing registry value was overwritten.' }
if ($script:keys[$registryPath]['2'] -notlike 'aaaaaaaa*' -or $script:keys[$registryPath]['3'] -notlike 'bbbbbbbb*') { throw 'New extensions did not use the available value names.' }
Write-Output 'EXTENSION_VALUES_PRESERVED'
""")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("EXTENSION_VALUES_PRESERVED", result.stdout)

    @unittest.skipUnless(shutil.which("powershell.exe"), "Windows PowerShell is unavailable")
    def test_missing_viewers_leave_the_error_console_visible(self):
        # Execute only the generated error handler, with app/path/UI operations
        # replaced by local stubs. No installer or browser can run in this test.
        encoded_source = base64.b64encode(wsb.BOOTSTRAP.encode("utf-16-le")).decode("ascii")
        harness = r"""
$source = [Text.Encoding]::Unicode.GetString([Convert]::FromBase64String('__SOURCE__'))
$tokens = $null; $errors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseInput($source,[ref]$tokens,[ref]$errors)
if ($errors) { throw 'Generated script did not parse.' }
$main = $ast.EndBlock.Statements | Where-Object { $_ -is [System.Management.Automation.Language.TryStatementAst] } | Select-Object -Last 1
$handler = $main.CatchClauses[0].Body.Extent.Text
function Log { param($message) }
function Test-Path { param($LiteralPath) return $false }
function FindEdge { return $null }
function Read-Host { param($Prompt) Write-Host 'ERROR_CONSOLE_REMAINED_OPEN'; return '' }
& ([scriptblock]::Create($handler.Substring(1,$handler.Length - 2)))
""".replace("__SOURCE__", encoded_source)
        with tempfile.TemporaryDirectory() as directory:
            test_script = Path(directory) / "fallback.ps1"
            test_script.write_text(harness, encoding="ascii")
            result = subprocess.run(
                [shutil.which("powershell.exe"), "-NoLogo", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", str(test_script)],
                text=True, capture_output=True, timeout=30,
            )
        self.assertEqual(result.returncode, 1, result.stderr)
        # Read-Host's output is assigned to $null by the handler, so the stub
        # writes directly to the host stream to prove that it was invoked.
        self.assertIn("ERROR_CONSOLE_REMAINED_OPEN", result.stdout)

    def test_real_catalog_is_complete_deterministic_and_self_contained(self):
        services = ET.parse(ROOT / "docs" / "Catalog.xml").findall("./InternetServices/Service")
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / "wsb"
            self.assertEqual(wsb.build(ROOT / "docs" / "Catalog.xml", output), len(services))
            self.assertEqual({p.stem for p in output.glob("*.wsb")}, {s.get("Id") for s in services})
            for service in services:
                with self.subTest(service=service.get("Id")):
                    data = wsb.service_data(service)
                    actual = (output / (data["Id"] + ".wsb")).read_bytes()
                    self.assertEqual(actual, wsb.make_wsb(data))
                    root, command, script, embedded = decode(actual)
                    self.assertLessEqual(len(command), wsb.MAX_COMMAND_CHARS)
                    self.assertTrue(command.isascii())
                    self.assertEqual(command, wsb.make_command(script))
                    self.assertEqual(embedded, data)
                    self.assertIsNone(root.find("MappedFolders"))
                    self.assertEqual(root.findtext("Networking"), "Enable")
                    self.assertEqual(root.findtext("ProtectedClient"), "Enable")
                    for setting in ("VGpu", "AudioInput", "VideoInput", "ClipboardRedirection", "PrinterRedirection"):
                        self.assertEqual(root.findtext(setting), "Disable")
                    self.assertIn("UseShellExecute = $false", script)
                    self.assertIn("WaitForExit(900000)", script)
                    self.assertIn("FAILED:", script)
                    self.assertEqual(len(embedded["Packages"]), len(service.findall("./Packages/Package")))
                    self.assertEqual(embedded["Bootstrap"], service.findtext("CustomBootstrap", ""))

    def test_catalog_text_cannot_become_powershell_syntax(self):
        attack = "'; Write-Host INJECTED; # $(Start-Process calc) ` \" & |"
        service = ET.Element("Service", Id="Test", DisplayName=attack, Url="https://example.test/")
        ET.SubElement(ET.SubElement(service, "Packages"), "Package", Name=attack,
                      Url="https://example.test/setup.exe", Arguments=attack)
        data = wsb.service_data(service)
        root, command, script, embedded = decode(wsb.make_wsb(data))
        self.assertEqual(embedded["Name"], attack)
        self.assertEqual(embedded["Packages"][0]["Arguments"], attack)
        self.assertNotIn("INJECTED", script)
        self.assertNotIn("INJECTED", command)

    def test_reject_unsafe_identifiers_urls_and_extensions(self):
        for identifier in ("../escape", "A/B", "A\\B", ".", "NUL", "lpt9", "a.", "", "A:B"):
            with self.subTest(identifier=identifier), self.assertRaises(ValueError):
                wsb.service_data(ET.Element("Service", Id=identifier, Url="https://example.test/"))
        for url in ("javascript:alert(1)", "file:///c:/x", "https://user:pass@example.test/", 'https://example.test/" & calc', "https://example.test/\n", "https://example.test/\\x"):
            with self.subTest(url=url), self.assertRaises(ValueError):
                wsb.checked_url(url)
        service = ET.fromstring('<Service Id="Test" Url="https://example.test/"><EdgeExtensions><EdgeExtension ExtensionId="../bad" CrxUrl="https://example.test/"/></EdgeExtensions></Service>')
        with self.assertRaises(ValueError):
            wsb.service_data(service)
        extension = service.find("./EdgeExtensions/EdgeExtension")
        extension.set("ExtensionId", "a" * 32)
        extension.set("CrxUrl", "https://example.test/extension.crx")
        with self.assertRaises(ValueError):
            wsb.service_data(service)

    def test_extension_update_policies_and_manual_setup_are_preserved(self):
        service = ET.fromstring('<Service Id="Test" Url="https://example.test/"><EdgeExtensions><EdgeExtension ExtensionId="aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" CrxUrl="https://clients2.google.com/service/update2/crx"/></EdgeExtensions></Service>')
        root, command, script, data = decode(wsb.make_wsb(wsb.service_data(service)))
        self.assertEqual(data["Extensions"][0]["Url"], "https://clients2.google.com/service/update2/crx")
        self.assertIn("ExtensionInstallForcelist", script)
        self.assertIn("extension-setup.html", script)
        self.assertIn("https://chromewebstore.google.com/detail/", script)

    def test_reject_dtd_oversize_duplicate_ids_and_wrong_root(self):
        for content in (
            b'<!DOCTYPE x [<!ENTITY test "payload">]><TableClothCatalog/>',
            b"x" * (wsb.MAX_XML_BYTES + 1),
            b'<TableClothCatalog><InternetServices><Service Id="Test" Url="https://example.test/"/><Service Id="test" Url="https://example.test/"/></InternetServices></TableClothCatalog>',
            b"<WrongRoot/>",
            '<!DOCTYPE x [<!ENTITY test "payload">]><TableClothCatalog/>'.encode("utf-16"),
        ):
            with tempfile.TemporaryDirectory() as directory:
                catalog = Path(directory) / "catalog.xml"
                catalog.write_bytes(content)
                with self.assertRaises(ValueError):
                    wsb.build(catalog, Path(directory) / "output")
                self.assertFalse((Path(directory) / "output").exists())

    def test_refresh_removes_stale_wsb_only(self):
        with tempfile.TemporaryDirectory() as directory:
            catalog = Path(directory) / "catalog.xml"
            catalog.write_text('<TableClothCatalog><InternetServices><Service Id="Test" Url="https://example.test/"/></InternetServices></TableClothCatalog>')
            output = Path(directory) / "output"
            output.mkdir()
            (output / "Old.wsb").write_text("stale")
            (output / "keep.txt").write_text("keep")
            wsb.build(catalog, output)
            self.assertFalse((output / "Old.wsb").exists())
            self.assertTrue((output / "Test.wsb").is_file())
            self.assertEqual((output / "keep.txt").read_text(), "keep")


if __name__ == "__main__":
    unittest.main()
