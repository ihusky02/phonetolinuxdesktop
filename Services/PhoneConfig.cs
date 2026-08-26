using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace phonetolinux.Services
{
    /// <summary>
    /// Manages application configuration persistence, including the target phone IP and communication port.
    /// Stores settings securely in a local JSON configuration file.
    /// </summary>
    public static class PhoneConfig
    {
        private static readonly string ConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".phonetolinux");
        private static readonly string ConfigFilePath = Path.Combine(ConfigDir, "config.json");

        /// <summary>
        /// Configuration data model representing fields persisted in the JSON file.
        /// </summary>
        public class ConfigModel
        {
            [JsonPropertyName("phoneIp")]
            public string PhoneIp { get; set; } = "";

            [JsonPropertyName("port")]
            public int Port { get; set; } = 5000;
        }

        public static string PhoneIp { get; set; } = LoadSavedIp();
        public static int Port { get; set; } = LoadSavedPort();

        /// <summary>
        /// Loads the saved phone IP address from the configuration file on startup.
        /// </summary>
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

        /// <summary>
        /// Loads the saved communication port from the configuration file on startup.
        /// </summary>
        private static int LoadSavedPort()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var config = JsonSerializer.Deserialize<ConfigModel>(json, options);
                    if (config?.Port > 0)
                    {
                        return config.Port;
                    }
                }
            }
            catch (Exception) { }

            // Default fallback port if configuration is missing
            return 5000;
        }

        /// <summary>
        /// Persists a new phone IP address while keeping the existing port configuration.
        /// </summary>
        public static void SaveIp(string newIp)
        {
            try
            {
                PhoneIp = newIp;
                SaveConfig(newIp, Port);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Persists a new communication port while keeping the existing phone IP configuration.
        /// </summary>
        public static void SavePort(int newPort)
        {
            try
            {
                Port = newPort;
                SaveConfig(PhoneIp, newPort);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Helper method to serialize and write the full configuration model to disk.
        /// </summary>
        private static void SaveConfig(string ip, int port)
        {
            Directory.CreateDirectory(ConfigDir);
            var config = new ConfigModel { PhoneIp = ip, Port = port };
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(ConfigFilePath, json);
        }

        /// <summary>
        /// Generates the dynamic base URL using the current phone IP and port.
        /// </summary>
        public static string GetBaseUrl() => $"http://{PhoneIp}:{Port}";
    }
}