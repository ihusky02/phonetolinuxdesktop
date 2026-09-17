using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace phonetolinux.Services;

public class UpdateInfo
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; } = "";

    [JsonPropertyName("changelog")]
    public string Changelog { get; set; } = "";
}

public static class UpdateService
{
    // Direct raw URL to version.json hosted on GitHub repository
    private const string VersionJsonUrl = "https://raw.githubusercontent.com/ihusky02/phonetolinuxdesktop/refs/heads/main/version.json";

    public static async Task<(bool hasUpdate, string newVersion, string changelog, string downloadUrl)> CheckForUpdatesAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "phonetolinux-updater");

            // Fetch the raw string from GitHub
            string json = await client.GetStringAsync(VersionJsonUrl);
            json = json.Trim();

            // If the response accidentally starts with "JSON", clean it up to extract the valid JSON object
            if (!json.StartsWith("{"))
            {
                int firstBrace = json.IndexOf('{');
                if (firstBrace != -1)
                {
                    json = json.Substring(firstBrace);
                }
            }

            Console.WriteLine($"[UPDATE DEBUG] Cleaned response: {json}");

            // Deserialize JSON string into UpdateInfo object
            var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(json);
            if (updateInfo == null || string.IsNullOrEmpty(updateInfo.Version))
            {
                return (false, null, null, null);
            }

            // Retrieve current assembly version and parse target version
            Version currentVersion = typeof(UpdateService).Assembly.GetName().Version ?? new Version(1, 0, 0, 0);
            Version latestVersion = new Version(updateInfo.Version);

            Console.WriteLine($"[UPDATE DEBUG] Current version: {currentVersion} | Latest version: {latestVersion}");

            if (latestVersion > currentVersion)
            {
                return (true, updateInfo.Version, updateInfo.Changelog, updateInfo.DownloadUrl);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UPDATE SERVICE ERROR] Exception caught during update check: {ex.Message}");
        }

        return (false, null, null, null);
    }

    public static async Task DownloadAndInstallUpdateAsync(string downloadUrl)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "phonetolinux-updater");

            Console.WriteLine($"[UPDATE DEBUG] Downloading binary package from: {downloadUrl}");
            byte[] fileBytes = await client.GetByteArrayAsync(downloadUrl);

            string tempFilePath = Path.Combine(Path.GetTempPath(), "phonetolinux_update.deb");
            await File.WriteAllBytesAsync(tempFilePath, fileBytes);

            Console.WriteLine($"[UPDATE DEBUG] Saved .deb to {tempFilePath}. Launching pkexec apt...");

            var startInfo = new ProcessStartInfo
            {
                FileName = "pkexec",
                Arguments = $"apt install -y \"{tempFilePath}\"",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UPDATE ERROR] Error during update installation: {ex.Message}");
        }
    }
}