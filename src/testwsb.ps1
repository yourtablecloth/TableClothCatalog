param(
    [Parameter(Mandatory = $true)]
    [string] $Directory
)

$ErrorActionPreference = 'Stop'
$files = @(Get-ChildItem -LiteralPath $Directory -Filter '*.wsb' -File)
if ($files.Count -eq 0) { throw 'No Windows Sandbox launchers found.' }

foreach ($file in $files) {
    $xml = New-Object System.Xml.XmlDocument
    $xml.XmlResolver = $null
    $xml.Load($file.FullName)
    $command = [string] $xml.Configuration.LogonCommand.Command
    if ($command -notmatch '^powershell\.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -EncodedCommand ([A-Za-z0-9+/=]+)$') {
        throw "Unexpected LogonCommand in $($file.Name)"
    }
    $script = [Text.Encoding]::Unicode.GetString([Convert]::FromBase64String($Matches[1]))
    $tokens = $null
    $parseErrors = $null
    $null = [System.Management.Automation.Language.Parser]::ParseInput($script, [ref] $tokens, [ref] $parseErrors)
    if ($parseErrors.Count -gt 0) {
        throw "$($file.Name): $($parseErrors.Message -join '; ')"
    }
    if ($script -notmatch '\$encodedData = ''([A-Za-z0-9+/=]+)''') {
        throw "Missing embedded catalog data in $($file.Name)"
    }
    $memory = New-Object IO.MemoryStream(,[Convert]::FromBase64String($Matches[1]))
    $gzip = New-Object IO.Compression.GZipStream($memory,[IO.Compression.CompressionMode]::Decompress)
    $reader = New-Object IO.StreamReader($gzip,[Text.Encoding]::UTF8)
    try { $site = $reader.ReadToEnd() | ConvertFrom-Json }
    finally { $reader.Dispose(); $memory.Dispose() }
    if ($site.Id -ne $file.BaseName) { throw "Embedded site ID mismatch in $($file.Name)" }
    if ($site.Bootstrap) {
        $null = [System.Management.Automation.Language.Parser]::ParseInput(
            $site.Bootstrap, [ref] $tokens, [ref] $parseErrors)
        if ($parseErrors.Count -gt 0) {
            throw "$($file.Name) custom bootstrap: $($parseErrors.Message -join '; ')"
        }
    }
}
Write-Output "Parsed $($files.Count) guest bootstrap scripts with PowerShell $($PSVersionTable.PSVersion). No guest code was executed."
