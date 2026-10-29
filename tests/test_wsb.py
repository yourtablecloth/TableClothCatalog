"""WSB generation checks do not download or execute catalog installers."""

import base64
import gzip
import importlib.util
import json
from pathlib import Path
import re
import shutil
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
    script = base64.b64decode(command.split("-EncodedCommand ", 1)[1]).decode("utf-16-le")
    payload = re.search(r"\$encodedData = '([A-Za-z0-9+/=]+)'", script).group(1)
    return root, command, script, json.loads(gzip.decompress(base64.b64decode(payload)))


class WsbTests(unittest.TestCase):
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
