param(
    [Parameter(Mandatory = $true)]
    [string] $Directory
)

$ErrorActionPreference = 'Stop'
$files = @(Get-ChildItem -LiteralPath $Directory -Filter '*.wsb' -File)
if ($files.Count -eq 0) { throw 'No Windows Sandbox launchers found.' }

function Assert-PowerShellSyntax([string] $Source, [string] $Name) {
    $tokens = $null
    $parseErrors = $null
    $null = [System.Management.Automation.Language.Parser]::ParseInput($Source, [ref] $tokens, [ref] $parseErrors)
    if ($parseErrors.Count -gt 0) { throw "${Name}: $($parseErrors.Message -join '; ')" }
}

function Read-GzipBase64([string] $Encoded) {
    $memory = New-Object IO.MemoryStream(,[Convert]::FromBase64String($Encoded))
    $gzip = New-Object IO.Compression.GZipStream($memory,[IO.Compression.CompressionMode]::Decompress)
    $reader = New-Object IO.StreamReader($gzip,[Text.Encoding]::UTF8)
    try { return $reader.ReadToEnd() }
    finally { $reader.Dispose(); $memory.Dispose() }
}

foreach ($file in $files) {
    $xml = New-Object System.Xml.XmlDocument
    $xml.XmlResolver = $null
    $xml.Load($file.FullName)
    $command = [string] $xml.Configuration.LogonCommand.Command
    if ($command.Length -gt 8000 -or $command -notmatch '^powershell\.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -Command "([^\r\n]+)"$') {
        throw "Unexpected LogonCommand in $($file.Name)"
    }
    $loader = $Matches[1]
    Assert-PowerShellSyntax $loader "$($file.Name) loader"
    if ($loader -notmatch 'FromBase64String\(''([A-Za-z0-9+/=]+)''\)') {
        throw "Missing compressed bootstrap in $($file.Name)"
    }
    $script = Read-GzipBase64 $Matches[1]
    Assert-PowerShellSyntax $script "$($file.Name) bootstrap"
    if ($script -notmatch '\$encodedData = ''([A-Za-z0-9+/=]+)''') {
        throw "Missing embedded catalog data in $($file.Name)"
    }
    $site = (Read-GzipBase64 $Matches[1]) | ConvertFrom-Json
    if ($site.Id -ne $file.BaseName) { throw "Embedded site ID mismatch in $($file.Name)" }
    if ($site.Bootstrap) {
        Assert-PowerShellSyntax $site.Bootstrap "$($file.Name) custom bootstrap"
    }
}
Write-Output "Parsed $($files.Count) loaders, restored guest bootstraps and custom bootstraps with PowerShell $($PSVersionTable.PSVersion). All commands fit within 8000 characters. No guest code was executed."
