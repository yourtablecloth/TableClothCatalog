﻿using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TableCloth.Models.WindowsSandbox;
using TableCloth.Resources;

namespace TableCloth.Components
{
    public sealed class SandboxLauncher
    {
        public SandboxLauncher(
            AppMessageBox appMessageBox,
            ILogger<SandboxLauncher> logger)
        {
            _appMessageBox = appMessageBox;
            _logger = logger;
        }

        private readonly AppMessageBox _appMessageBox;
        private readonly ILogger _logger;

        private bool ValidateSandboxSpecFile(string wsbFilePath, out string reason)
        {
            try
            {
                if (!File.Exists(wsbFilePath))
                {
                    _logger.LogError(reason = $"WSB 파일 `{wsbFilePath}`이 생성되지 않았습니다.");
                    return false;
                }

                SandboxConfiguration content = null;

                using (var fileStream = File.OpenRead(wsbFilePath))
                {
                    var serializer = new XmlSerializer(typeof(SandboxConfiguration));
                    content = serializer.Deserialize(fileStream) as SandboxConfiguration;

                    if (content == null)
                    {
                        _logger.LogError(reason = $"WSB 파일을 개체로 다시 불러올 수 없습니다.");
                        return false;
                    }
                }

                foreach (var eachMappedFolder in content.MappedFolders)
                {
                    // HostFolder 태그에 들어가는 경로는 절대 경로만 사용되므로 상대 경로 처리를 하지 않아도 됨.
                    if (!Directory.Exists(eachMappedFolder.HostFolder))
                    {
                        _logger.LogError(reason = $"WSB 파일에 지정된 폴더 중 `{eachMappedFolder.HostFolder}` 디렉터리가 없습니다.");
                        return false;
                    }
                }

                reason = null;
                return true;
            }
            catch (Exception ex)
            {
                var actualException = ex;

                if (ex is AggregateException)
                    actualException = ex.InnerException ?? ex;

                _logger.LogError(actualException, reason = $"WSB 파일을 개체로 불러오는 도중 오류가 발생했습니다. {actualException.Message}");
                return false;
            }
        }

        public void RunSandbox(string wsbFilePath)
        {
            if (!ValidateSandboxSpecFile(wsbFilePath, out string reason))
            {
                _appMessageBox.DisplayError(reason, true);
                return;
            }

            var comSpecPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "cmd.exe");

            var process = new Process()
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo(comSpecPath, "/c start \"\" \"" + wsbFilePath + "\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            if (!process.Start())
            {
                process.Dispose();
                _appMessageBox.DisplayError(StringResources.Error_Windows_Sandbox_CanNotStart, true);
            }
        }

        public bool IsSandboxRunning()
            => Process.GetProcesses().Where(x => x.ProcessName.StartsWith("WindowsSandbox", StringComparison.OrdinalIgnoreCase)).Any();
    }
}
