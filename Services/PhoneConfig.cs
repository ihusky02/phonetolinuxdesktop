using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace phonetolinux.Services
{
    public static class PhoneConfig
    {
        private static readonly string ConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".phonetolinux");
        private static readonly string ConfigFilePath = Path.Combine(ConfigDir, "config.json");

        public class ConfigModel
        {
            [JsonPropertyName("phoneIp")]
            public string PhoneIp { get; set; } = "";
        }

        public static string PhoneIp { get; set; } = LoadSavedIp();
        public const int Port = 5000;

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

            // Default fallback if file is missing or an error occurs
            return ""; 
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

        public static string GetBaseUrl() => $"http://{PhoneIp}:{Port}";
    }
}