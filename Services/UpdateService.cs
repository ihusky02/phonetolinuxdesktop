using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace phonetolinux.Services;
public class UpdateInfo
{
    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; }

    [JsonPropertyName("changelog")]
    public string Changelog { get; set; }
}

public static class UpdateService
{
    // URL to the version.json file hosted publicly on Google Drive
    private const string VersionJsonUrl = "https://drive.google.com/uc?export=download&id=1ec4QizAwpoDG-YcZm58tkyjdJl4_9IGG";

    public static async Task<(bool hasUpdate, string newVersion, string changelog, string downloadUrl)> CheckForUpdatesAsync()
    {
        try
        {
            using var client = new HttpClient();
            string json = await client.GetStringAsync(VersionJsonUrl);
            
            var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(json);
            if (updateInfo == null) return (false, null, null, null);

            // Get the current version of the application from the assembly
            Version currentVersion = typeof(UpdateService).Assembly.GetName().Version;
            Version latestVersion = new Version(updateInfo.Version);

            if (latestVersion > currentVersion)
            {
                return (true, updateInfo.Version, updateInfo.Changelog, updateInfo.DownloadUrl);
            }
        }
        catch (Exception)
        {
            // Silently ignore network or parsing errors during startup
        }

        return (false, null, null, null);
    }

    public static async Task DownloadAndInstallUpdateAsync(string downloadUrl)
    {
        try
        {
            using var client = new HttpClient();
            byte[] fileBytes = await client.GetByteArrayAsync(downloadUrl);

            string tempFilePath = Path.Combine(Path.GetTempPath(), "phonetolinux_update.deb");
            await File.WriteAllBytesAsync(tempFilePath, fileBytes);

            // Run the .deb package installation with root privileges via pkexec (prompts the user for a password)
            var startInfo = new ProcessStartInfo
            {
                FileName = "pkexec",
                Arguments = $"apt install -y {tempFilePath}",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during update installation: {ex.Message}");
        }
    }
}