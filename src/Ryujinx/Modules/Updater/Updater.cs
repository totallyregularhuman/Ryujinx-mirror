using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ryujinx.Modules
{
    internal static partial class Updater
    {
        private const string GitHubApiUrl = "https://api.github.com";

        private static readonly HttpClient _httpClient = new HttpClient
        {
            DefaultRequestHeaders =
            {
                { "User-Agent", "Ryujinx-Updater/1.0.0" }
            }
        };

        private static readonly string _updateDir = Path.Combine(Path.GetTempPath(), "Ryujinx", "update");

        private static bool _running;

        public static async Task BeginParse(Window mainWindow, bool showVersionUpToDate)
        {
            if (_running)
            {
                return;
            }

            _running = true;

            DetectPlatform();

            Version currentVersion = await GetCurrentVersion();
            if (currentVersion == null)
            {
                return;
            }

            //string buildInfoUrl = $"{GitHubApiUrl}/repos/{ReleaseInformation.ReleaseChannelOwner}/{ReleaseInformation.ReleaseChannelRepo}/releases/latest";
            string buildInfoUrl = $"{GitHubApiUrl}/repos/Ryujinx/release-channel-master/releases/latest"; // Temporary code, will revert back
            if (!await TryUpdateVersionInfo(buildInfoUrl, showVersionUpToDate))
            {
                return;
            }

            if (!await HandleVersionComparison(currentVersion, showVersionUpToDate))
            {
                return;
            }

            await FetchBuildSizeInfo();

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await ShowUpdateDialogAndExecute(mainWindow);
            });
        }
    }
}
