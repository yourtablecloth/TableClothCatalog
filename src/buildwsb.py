#!/usr/bin/env python3
"""Build self-contained Windows Sandbox launchers; never execute catalog code here."""

from __future__ import annotations

import argparse
import base64
import gzip
import json
import re
from pathlib import Path
from urllib.parse import urlsplit
import xml.etree.ElementTree as ET

MAX_XML_BYTES = 4 * 1024 * 1024
MAX_COMMAND_CHARS = 32766  # CreateProcessW includes a terminating null in its limit.
ID_PATTERN = re.compile(r"[A-Za-z0-9][A-Za-z0-9_-]{0,95}\Z")
RESERVED_NAMES = {"CON", "PRN", "AUX", "NUL"} | {
    f"{prefix}{number}" for prefix in ("COM", "LPT") for number in range(1, 10)
}

# Catalog data is compressed JSON, not PowerShell syntax. CustomBootstrap alone
# is deliberately executable catalog content, written to a separate guest file.
# Keep this compatible with Windows PowerShell 5.1 in the Sandbox image.
BOOTSTRAP = r"""$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$encodedData = '__PAYLOAD__'
$memory = New-Object IO.MemoryStream(,[Convert]::FromBase64String($encodedData))
$gzip = New-Object IO.Compression.GZipStream($memory,[IO.Compression.CompressionMode]::Decompress)
$reader = New-Object IO.StreamReader($gzip,[Text.Encoding]::UTF8)
try { $site = $reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose(); $memory.Dispose() }
$work = Join-Path ([Environment]::GetFolderPath('Desktop')) ('TableCloth-' + $site.Id)
[IO.Directory]::CreateDirectory($work) | Out-Null
$log = Join-Path $work 'bootstrap.log'
function Log([string]$message) {
    $line = ('{0:o} {1}' -f [DateTime]::UtcNow,$message)
    Add-Content -LiteralPath $log -Value $line -Encoding UTF8
    Write-Host $message
}
function Run([string]$file,[string]$arguments) {
    $info = New-Object Diagnostics.ProcessStartInfo
    $info.FileName = $file
    $info.Arguments = $arguments
    $info.UseShellExecute = $false
    $process = [Diagnostics.Process]::Start($info)
    try {
        if (-not $process.WaitForExit(900000)) { $process.Kill(); throw 'Installer exceeded 15 minutes.' }
        if ($process.ExitCode -notin @(0,1641,3010)) { throw ('Process exit code: ' + $process.ExitCode) }
        if ($process.ExitCode -ne 0) { Log 'Installer requested a reboot; this launcher does not reboot the Sandbox.' }
    } finally { $process.Dispose() }
}
function AssertInstallerHttps([uri]$uri) {
    if (-not $uri.IsAbsoluteUri -or $uri.Scheme -ne 'https' -or $uri.UserInfo) { throw ('Installer downloads require HTTPS: ' + $uri) }
}
function Download([string]$url,[string]$path) {
    $current = [uri]$url
    for ($redirect = 0; $redirect -le 5; $redirect++) {
        AssertInstallerHttps $current
        $request = [Net.HttpWebRequest]::Create($current)
        $request.AllowAutoRedirect = $false
        $request.Timeout = 120000
        $request.ReadWriteTimeout = 120000
        $request.UserAgent = 'TableClothCatalog-WSB/1.0'
        $response = $request.GetResponse()
        try {
            $status = [int]$response.StatusCode
            if ($status -in @(301,302,303,307,308)) {
                $location = $response.Headers['Location']
                if ($redirect -eq 5 -or -not $location) { throw 'Invalid or excessive installer redirects.' }
                $next = [uri]::new($current,$location)
                AssertInstallerHttps $next
                $current = $next
                continue
            }
            if ($status -lt 200 -or $status -ge 300) { throw ('Unexpected download status: ' + $status) }
            if ($response.ContentLength -gt 536870912) { throw 'Installer exceeds 512 MiB.' }
            $inputStream = $response.GetResponseStream()
            $outputStream = [IO.File]::Create($path)
            try {
                $buffer = New-Object byte[] 65536
                $total = 0
                while (($read = $inputStream.Read($buffer,0,$buffer.Length)) -gt 0) {
                    $total += $read
                    if ($total -gt 536870912) { throw 'Installer exceeds 512 MiB.' }
                    $outputStream.Write($buffer,0,$read)
                }
            } finally { $outputStream.Dispose(); $inputStream.Dispose() }
            return
        } finally { $response.Close() }
    }
}
function FindEdge {
    @("${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe","$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe") | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}
try {
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Log ('Preparing ' + $site.Name)
    Log 'This session uses networking. No host folders, certificates, clipboard, microphone, camera or printers are shared.'
    if ($site.Notes) { Log $site.Notes }
    $site | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $work 'catalog-entry.json') -Encoding UTF8
    foreach ($package in $site.Packages) { AssertInstallerHttps ([uri]$package.Url) }
    $number = 0
    foreach ($package in $site.Packages) {
        $number++
        Log ('Downloading ' + $package.Name + ': ' + $package.Url)
        $download = Join-Path $work ($number.ToString() + '.download')
        Download $package.Url $download
        $header = New-Object byte[] 8
        $stream = [IO.File]::OpenRead($download)
        try { $count = $stream.Read($header,0,8) } finally { $stream.Dispose() }
        $signature = [BitConverter]::ToString($header)
        if ($count -ge 2 -and $header[0] -eq 77 -and $header[1] -eq 90) { $extension = '.exe' }
        elseif ($signature -eq 'D0-CF-11-E0-A1-B1-1A-E1') { $extension = '.msi' }
        else { throw ('Expected an EXE/MSI installer, received another format: ' + $package.Name) }
        $installer = [IO.Path]::ChangeExtension($download,$extension)
        Move-Item -LiteralPath $download -Destination $installer
        Log ('Installing ' + $package.Name + '. Interactive installers may ask for input.')
        if ($extension -eq '.msi') { Run "$env:SystemRoot\System32\msiexec.exe" ('/i "' + $installer + '" ' + $package.Arguments) }
        else { Run $installer $package.Arguments }
    }
    $number = 0
    $extensionLinks = @()
    foreach ($extension in $site.Extensions) {
        $number++
        $key = 'HKLM:\SOFTWARE\Policies\Microsoft\Edge\ExtensionInstallForcelist'
        if (-not (Test-Path -LiteralPath $key)) { New-Item -Path $key -Force | Out-Null }
        New-ItemProperty -Path $key -Name $number.ToString() -Value ($extension.Id + ';' + $extension.Url) -PropertyType String -Force | Out-Null
        Log ('Registered Edge extension policy: ' + $extension.Id + '. Third-party force-install may be blocked in a non-domain Sandbox. Use the extension setup tab to complete installation.')
        if ($extension.Url -eq 'https://clients2.google.com/service/update2/crx') {
            $extensionLinks += '<li><a href="https://chromewebstore.google.com/detail/' + $extension.Id + '">' + $extension.Id + '</a></li>'
        } else {
            $extensionLinks += '<li>' + $extension.Id + ': consult the catalog provider for manual installation. Update URL: ' + [Security.SecurityElement]::Escape($extension.Url) + '</li>'
        }
    }
    $help = $null
    if ($extensionLinks.Count -gt 0) {
        $help = Join-Path $work 'extension-setup.html'
        $html = '<!doctype html><html lang="en"><meta charset="utf-8"><title>Edge extension setup</title><body><h1>Edge extension setup</h1><p>Windows Sandbox is not domain joined. Edge may block automatic third-party extension installation. Open each store link, allow other stores if Edge asks, and review the extension permissions before installing.</p><ul>' + ($extensionLinks -join '') + '</ul><p>Catalog policy registration does not prove installation. Check edge://extensions and edge://policy before using a site that depends on these extensions.</p></body></html>'
        [IO.File]::WriteAllText($help,$html,[Text.Encoding]::UTF8)
    }
    if ($site.Bootstrap) {
        Log 'Running the catalog custom bootstrap in the Sandbox.'
        $custom = Join-Path $work 'catalog-bootstrap.ps1'
        $content = '$ErrorActionPreference = ''Stop''; $LASTEXITCODE = 0' + "`r`n" + $site.Bootstrap + "`r`n" + 'if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }'
        [IO.File]::WriteAllText($custom,$content,[Text.Encoding]::Unicode)
        Run "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" ('-NoLogo -NoProfile -ExecutionPolicy Bypass -File "' + $custom + '"')
    }
    $edge = FindEdge
    if (-not $edge) { throw 'Microsoft Edge was not found in this Sandbox image.' }
    Log 'Preparation finished. Opening the site in Microsoft Edge. Site compatibility is not guaranteed.'
    $info = New-Object Diagnostics.ProcessStartInfo
    $info.FileName = $edge
    $info.Arguments = '--new-window "' + $site.Url + '"'
    if ($help) { $info.Arguments += ' "' + $help + '"' }
    $info.UseShellExecute = $false
    [Diagnostics.Process]::Start($info) | Out-Null
} catch {
    Log ('FAILED: ' + $_.Exception.Message)
    Log 'Preparation stopped. See this log and catalog-entry.json on the Sandbox desktop. Diagnostics remain inside this Sandbox.'
    $shown = $false
    $notepad = "$env:SystemRoot\System32\notepad.exe"
    if (Test-Path -LiteralPath $notepad) {
        try { Start-Process -FilePath $notepad -ArgumentList ('"' + $log + '"'); $shown = $true }
        catch { Write-Host 'Notepad could not open the log. Trying another viewer.' }
    }
    if (-not $shown) {
        $edge = FindEdge
        if ($edge) {
            try {
                $failure = Join-Path $work 'bootstrap-error.html'
                $details = [Security.SecurityElement]::Escape([IO.File]::ReadAllText($log))
                [IO.File]::WriteAllText($failure,('<!doctype html><html lang="en"><meta charset="utf-8"><title>TableCloth preparation error</title><body><h1>Preparation stopped</h1><p>Diagnostics remain in the TableCloth folder on this Sandbox desktop.</p><pre style="white-space:pre-wrap">' + $details + '</pre></body></html>'),[Text.Encoding]::UTF8)
                $info = New-Object Diagnostics.ProcessStartInfo
                $info.FileName = $edge
                $info.Arguments = '--new-window "' + $failure + '"'
                $info.UseShellExecute = $false
                [Diagnostics.Process]::Start($info) | Out-Null
                $shown = $true
            } catch { Write-Host 'The browser could not open the diagnostic page.' }
        }
    }
    if (-not $shown) { $null = Read-Host 'Press Enter to close this error console' }
    exit 1
}
"""


def checked_url(value: str) -> str:
    """Allow web URLs only; disallow characters unsafe in native quoted arguments."""
    parts = urlsplit(value)
    if (parts.scheme not in {"http", "https"} or not parts.hostname
            or parts.username is not None or parts.password is not None
            or any(ord(char) < 32 or char in '\\"' for char in value)):
        raise ValueError(f"Unsafe catalog URL: {value!r}")
    return value


def service_data(service: ET.Element) -> dict:
    identifier = service.get("Id", "")
    if not ID_PATTERN.fullmatch(identifier) or identifier.upper() in RESERVED_NAMES:
        raise ValueError(f"Unsafe service Id: {identifier!r}")
    extensions = []
    for extension in service.findall("./EdgeExtensions/EdgeExtension"):
        extension_id = extension.get("ExtensionId", "")
        if not re.fullmatch(r"[a-p]{32}", extension_id):
            raise ValueError(f"Invalid Edge extension Id: {extension_id!r}")
        update_url = checked_url(extension.get("CrxUrl", ""))
        if urlsplit(update_url).path.lower().endswith(".crx") or ";" in update_url:
            raise ValueError("Edge extension policy requires an update URL, not a direct CRX package.")
        extensions.append({"Id": extension_id, "Url": update_url})
    return {
        "Id": identifier,
        "Name": service.get("DisplayName", identifier),
        "Url": checked_url(service.get("Url", "")),
        "Notes": service.findtext("CompatNotes", ""),
        "Packages": [
            {"Name": package.get("Name", ""), "Url": checked_url(package.get("Url", "")),
             "Arguments": package.get("Arguments", "")}
            for package in service.findall("./Packages/Package")
        ],
        "Extensions": extensions,
        "Bootstrap": service.findtext("CustomBootstrap", ""),
    }


def make_wsb(data: dict) -> bytes:
    payload = base64.b64encode(gzip.compress(
        json.dumps(data, ensure_ascii=False, separators=(",", ":")).encode("utf-8"), mtime=0
    )).decode("ascii")
    script = BOOTSTRAP.replace("__PAYLOAD__", payload)
    encoded = base64.b64encode(script.encode("utf-16-le")).decode("ascii")
    command = "powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -EncodedCommand " + encoded
    if len(command) > MAX_COMMAND_CHARS:
        raise ValueError(f"Launcher exceeds Windows command-line limit: {data['Id']}")
    root = ET.Element("Configuration")
    root.append(ET.Comment("Generated from the TableCloth catalog. Runs installers only inside Windows Sandbox."))
    for name, value in {
        "VGpu": "Disable", "Networking": "Enable", "AudioInput": "Disable",
        "VideoInput": "Disable", "ProtectedClient": "Enable",
        "PrinterRedirection": "Disable", "ClipboardRedirection": "Disable",
    }.items():
        ET.SubElement(root, name).text = value
    ET.SubElement(ET.SubElement(root, "LogonCommand"), "Command").text = command
    ET.indent(root, space="  ")
    return ET.tostring(root, encoding="utf-8", xml_declaration=True) + b"\n"


def build(catalog: Path, output: Path) -> int:
    raw = catalog.read_bytes()
    if len(raw) > MAX_XML_BYTES or b"<!DOCTYPE" in raw.upper() or b"<!ENTITY" in raw.upper():
        raise ValueError("Catalog exceeds size limit or contains a DTD/entity declaration.")
    if raw.startswith((b"\xff\xfe", b"\xfe\xff")) or b"\0" in raw:
        raise ValueError("Catalog must use UTF-8 encoding.")
    root = ET.fromstring(raw)
    if root.tag != "TableClothCatalog":
        raise ValueError("Expected TableClothCatalog root element.")
    launchers: dict[str, bytes] = {}
    seen: set[str] = set()
    for service in root.findall("./InternetServices/Service"):
        data = service_data(service)
        identifier = data["Id"]
        if identifier.casefold() in seen:
            raise ValueError(f"Duplicate service Id: {identifier}")
        seen.add(identifier.casefold())
        launchers[identifier + ".wsb"] = make_wsb(data)
    output.mkdir(parents=True, exist_ok=True)
    # Remove only old generated launcher files, never an arbitrary directory tree.
    for stale in output.glob("*.wsb"):
        if stale.name not in launchers:
            stale.unlink()
    for name, content in launchers.items():
        destination = output / name
        if destination.is_symlink():
            raise ValueError(f"Refusing to overwrite a symlink: {destination}")
        destination.write_bytes(content)
    return len(launchers)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("catalog", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    count = build(args.catalog, args.output)
    print(f"Generated {count} self-contained WSB launchers in {args.output}")


if __name__ == "__main__":
    main()
