using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace phonetolinux.Services;

public static class PhoneConfig
{
    private static readonly string ConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".phonetolinux");
    private static readonly string ConfigFilePath = Path.Combine(ConfigDir, "config.json");
    private static readonly string SecretFilePath = Path.Combine(ConfigDir, "paired_device.dat");

    public const int Port = 5000;

    public class ConfigModel
    {
        [JsonPropertyName("phoneIp")]
        public string PhoneIp { get; set; } = "";
    }

    // Static properties initialized upon application load
    public static string PhoneIp { get; set; } = LoadSavedIp();
    public static string PairingSecret { get; set; } = LoadSavedSecret();

    private static string LoadSavedIp()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                string json = File.ReadAllText(ConfigFilePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var config = JsonSerializer.Deserialize<ConfigModel>(json, options);
                if (!string.IsNullOrEmpty(config?.PhoneIp))
                {
                    return config.PhoneIp;
                }
            }
        }
        catch (Exception) { }

        return string.Empty; 
    }

    private static string LoadSavedSecret()
    {
        try
        {
            if (File.Exists(SecretFilePath))
            {
                return File.ReadAllText(SecretFilePath).Trim();
            }
        }
        catch (Exception) { }

        return string.Empty;
    }

    public static void SaveIp(string newIp)
    {
        try
        {
            PhoneIp = newIp;
            Directory.CreateDirectory(ConfigDir);
            var config = new ConfigModel { PhoneIp = newIp };
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(ConfigFilePath, json);
        }
        catch (Exception) { }
    }

    public static void SaveSecret(string newSecret)
    {
        try
        {
            PairingSecret = newSecret;
            Directory.CreateDirectory(ConfigDir);
            File.WriteAllText(SecretFilePath, newSecret.Trim());
        }
        catch (Exception) { }
    }

    public static string GetBaseUrl() => $"http://{PhoneIp}:{Port}";
}