#!/usr/bin/env dotnet

// Copyright 2025 Alexandru Avadanii
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Converted from AutoIt to C# Console Application.
// Converted and developed by Jung-Hyun Nam.
// Original project: https://github.com/alexandruavadanii/USSF

using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

ReadOnlyDictionary<int, (string Extension, string Type, string Usage, string Notes)> Messages = new ReadOnlyDictionary<int, (string, string, string, string)>(new Dictionary<int, (string, string, string, string)>
{
    {1, (".inf", "Information or Installation file", "rundll32.exe setupapi,InstallHinfSection DefaultInstall 132 {filename}", "N/A")},
    {2, (".reg", "Registry file", "regedit.exe /s \"{filename}\"", "")},
    {3, (".exe", "NSIS Package", "\"{filename}\" /S", "")},
    {4, (".exe", "Inno Setup Package", "\"{filename}\" /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-", "")},
    {5, (".exe", "Installshield AFW Package", "", "Extract the installation file with UniExtract or another unpacker. The unpacked archive should be either .CAB or .MSI based. Next only for .CAB based files: Create an installation with this command: {filename} /r /f1\"X:\\setup.iss\" Now you can perform a silent installation using the ISS file: {filename} /s /f1\"X:\\setup.iss\" Next only for .MSI based files: msiexec.exe /i setup.msi /qb")},
    {6, (".exe", "InstallShield 2003 Package", "\"{filename}\" /s /v\"/qb\"", "You can also try to get the .MSI file from the Temp directory during installation, then install with: msiexec.exe /i setup.msi /qb")},
    {7, (".exe", "Wise Installer Package", "\"{filename}\" /s", "")},
    {8, (".exe", "Self-Extracting RAR Archive", "\"{filename}\" /s", "The RAR comment contains the installation script.")},
    {9, (".exe", "Self-Extracting Microsoft CAB Archive", "", "Extract the archive with UniExtract or another unpacker.")},
    {10, (".exe", "Self-Extracting ZIP Archive", "\"{filename}\" /s", "")},
    {11, (".exe", "7-Zip Installer Package", "\"{filename}\" /s", "")},
    {12, (".exe", "Unknown 7-Zip Archive", "", "Extract with 7-Zip")},
    {13, (".exe", "Unknown ZIP Archive", "", "Extract with unzip")},
    {14, (".exe", "Self-Extracting WinZip Archive", "\"{filename}\" /s", "")},
    {15, (".exe", "UPX Packed", "", "Unpack with UPX")},
    {16, (".msi", "MSI File", "msiexec.exe /i \"{filename}\" /qb", "")}
});

bool jsonOutput = false;
bool showHelp = false;
string? filePath = null;

foreach (var arg in args)
{
    if (arg == "--json")
        jsonOutput = true;
    else if (arg is "--help" or "-h" or "-?")
        showHelp = true;
    else if (filePath == null)
        filePath = arg;
}

if (showHelp)
{
    Console.WriteLine("ussfc - Universal Silent Setup Finder / Console");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  ussfc [options] <file_path>");
    Console.WriteLine("  <data> | ussfc [options]");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  <file_path>   Path to the installer or file to analyze.");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --json        Output result as JSON instead of human-readable text.");
    Console.WriteLine("  --help, -h    Show this help message.");
    Console.WriteLine();
    Console.WriteLine("Supported file types:");
    Console.WriteLine("  .inf   Information / Installation file");
    Console.WriteLine("  .reg   Registry file");
    Console.WriteLine("  .msi   Windows Installer package");
    Console.WriteLine("  .exe   Executables (NSIS, Inno Setup, InstallShield, Wise, 7-Zip, WinZip, UPX, etc.)");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  ussfc setup.exe");
    Console.WriteLine("  ussfc --json setup.msi");
    Console.WriteLine("  cat setup.exe | ussfc --json");
    return;
}

bool stdinMode = filePath == null && Console.IsInputRedirected;

if (filePath == null && !stdinMode)
{
    Console.WriteLine("Usage: ussfc [--json] [--help] <file_path>");
    return;
}

string displayName;
string? extension;
byte[] headerBuffer;

if (stdinMode)
{
    displayName = "<stdin>";
    extension = null;
    using var stdin = Console.OpenStandardInput();
    headerBuffer = ReadHeaderBytes(stdin, 512);
}
else
{
    if (!File.Exists(filePath!))
    {
        if (jsonOutput)
            WriteJsonError(filePath!, null, "File does not exist.");
        else
            Console.WriteLine("File does not exist.");
        return;
    }
    displayName = Path.GetFileName(filePath!);
    extension = Path.GetExtension(filePath!).ToLower();
    using var fs = new FileStream(filePath!, FileMode.Open, FileAccess.Read);
    headerBuffer = ReadHeaderBytes(fs, 512);
}

int messageNumber = DetectFromBytes(headerBuffer, extension);
if (messageNumber > 0 && Messages.ContainsKey(messageNumber))
{
    var msg = Messages[messageNumber];
    if (jsonOutput)
    {
        using var stdout = Console.OpenStandardOutput();
        using var writer = new Utf8JsonWriter(stdout, new JsonWriterOptions { Indented = true });
        writer.WriteStartObject();
        writer.WriteString("file", stdinMode ? "<stdin>" : filePath);
        if (extension != null)
            writer.WriteString("extension", extension);
        writer.WriteString("type", msg.Type);
        writer.WriteString("usage", msg.Usage.Replace("{filename}", displayName));
        writer.WriteString("notes", msg.Notes.Replace("{filename}", displayName));
        writer.WriteEndObject();
        writer.Flush();
        stdout.WriteByte((byte)'\n');
    }
    else
    {
        Console.WriteLine($"File: {(stdinMode ? "<stdin>" : filePath)}");
        if (extension != null)
            Console.WriteLine($"Extension: {extension}");
        Console.WriteLine($"Type: {msg.Type}");
        Console.WriteLine($"Usage: {msg.Usage.Replace("{filename}", displayName)}");
        if (!string.IsNullOrEmpty(msg.Notes))
            Console.WriteLine($"Notes: {msg.Notes.Replace("{filename}", displayName)}");
    }
}
else
{
    string errorMsg = messageNumber switch
    {
        -1 => "Corrupted EXE: invalid MZ header.",
        -2 => "Corrupted MSI: invalid header.",
        -3 => "Invalid REG file format.",
        _ => "Unknown or unsupported file type."
    };

    if (jsonOutput)
        WriteJsonError(stdinMode ? "<stdin>" : filePath!, extension, errorMsg);
    else
        Console.WriteLine($"Error: {errorMsg}");
}

static void WriteJsonError(string filePath, string? extension, string error)
{
    using var stdout = Console.OpenStandardOutput();
    using var writer = new Utf8JsonWriter(stdout, new JsonWriterOptions { Indented = true });
    writer.WriteStartObject();
    writer.WriteString("file", filePath);
    if (extension != null)
        writer.WriteString("extension", extension);
    writer.WriteString("error", error);
    writer.WriteEndObject();
    writer.Flush();
    stdout.WriteByte((byte)'\n');
}

static byte[] ReadHeaderBytes(Stream stream, int count)
{
    byte[] buffer = new byte[count];
    int offset = 0;
    while (offset < count)
    {
        int read = stream.Read(buffer, offset, count - offset);
        if (read == 0) break;
        offset += read;
    }
    if (offset < count)
        return buffer[..offset];
    return buffer;
}

static bool HeaderMatches(byte[] header, string expectedHex)
{
    int length = expectedHex.Length / 2;
    if (header.Length < length)
        return false;
    string hex = BitConverter.ToString(header, 0, length).Replace("-", "");
    return hex.Equals(expectedHex, StringComparison.OrdinalIgnoreCase);
}

static bool IsRegContent(byte[] header)
{
    // UTF-16 LE BOM (FF FE)
    if (header.Length >= 2 && header[0] == 0xFF && header[1] == 0xFE)
    {
        string text = Encoding.Unicode.GetString(header).TrimStart('\uFEFF');
        return text.StartsWith("Windows Registry Editor Version 5.00") ||
               text.StartsWith("REGEDIT4");
    }
    // UTF-8 (with or without BOM)
    string utf8 = Encoding.UTF8.GetString(header).TrimStart('\uFEFF');
    return utf8.StartsWith("Windows Registry Editor Version 5.00") ||
           utf8.StartsWith("REGEDIT4");
}

static bool IsInfContent(byte[] header)
{
    string text = Encoding.UTF8.GetString(header).TrimStart('\uFEFF').TrimStart();
    return text.StartsWith("[");
}

static int DetectFromBytes(byte[] header, string? extension)
{
    // Binary signature detection (extension-agnostic)
    if (HeaderMatches(header, "D0CF11E0A1B11AE1000000000000000000000000000000003E000300FEFF090006"))
        return 16; // MSI (OLE Compound Document)

    if (HeaderMatches(header, "4D5A")) // MZ -> EXE
        return 3; // Assume NSIS (extend for further detection)

    // Text-based detection
    if (IsRegContent(header))
        return 2;

    // Extension fallback
    switch (extension)
    {
        case ".inf": return 1;
        case ".msi": return -2; // MSI without valid header -> corrupted
        case ".exe": return -1; // EXE without MZ header -> corrupted
        case ".reg": return -3; // REG without valid header -> invalid
    }

    // Heuristic: INF-like text (no extension available)
    if (IsInfContent(header))
        return 1;

    return -6; // Not supported
}

