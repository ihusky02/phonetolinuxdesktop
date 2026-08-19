using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PhoneToLinux.Core;

namespace PhoneToLinux.Plugins
{
    /// <summary>
    /// Wtyczka odpowiedzialna za zarządzanie konfiguracją połączenia z telefonem (adres IP i port),
    /// w tym odczyt i zapis stanu do pliku konfiguracyjnego w katalogu użytkownika.
    /// </summary>
    public class PhoneConfigPlugin : IPhonePlugin
    {
        /// <summary>Ścieżka do katalogu konfiguracyjnego w profilu użytkownika.</summary>
        private static readonly string ConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".phonetolinux");
        
        /// <summary>Pełna ścieżka do pliku konfiguracyjnego config.json.</summary>
        private static readonly string ConfigFilePath = Path.Combine(ConfigDir, "config.json");

        /// <summary>Numer portu nasłuchiwania serwera na telefonie.</summary>
        public const int Port = 5000;

        /// <summary>Aktualny adres IP telefonu.</summary>
        public static string PhoneIp { get; private set; } = LoadSavedIp();

        /// <inheritdoc />
        public string Endpoint => "/config";

        /// <summary>
        /// Model danych dla deserializacji i serializacji konfiguracji JSON.
        /// </summary>
        public class ConfigModel
        {
            [JsonPropertyName("phoneIp")]
            public string PhoneIp { get; set; } = "";
        }

        /// <summary>
        /// Wykonuje operacje wtyczki w zależności od przekazanych parametrów (np. pobranie lub ustawienie IP).
        /// </summary>
        /// <param name="queryParams">Parametry zapytania (np. akcja lub nowe IP).</param>
        /// <returns>Odpowiedź w formacie JSON.</returns>
        public string Execute(string queryParams)
        {
            // Możemy obsłużyć zapytania HTTP do wtyczki, np. zwrócenie aktualnego bazowego URL
            return JsonSerializer.Serialize(new { phoneIp = PhoneIp, port = Port, baseUrl = GetBaseUrl() });
        }

        /// <summary>
        /// Zwraca pełny adres bazowy URL do komunikacji z serwerem na telefonie.
        /// </summary>
        public static string GetBaseUrl() => $"http://{PhoneIp}:{Port}";

        /// <summary>
        /// Łagodzi i wczytuje zapisany adres IP z pliku konfiguracyjnego na dysku.
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

            // Domyślny fallback, jeśli brak pliku lub wystąpił błąd
            return "";
        }

        /// <summary>
        /// Zapisuje nowy adres IP telefonu do pliku konfiguracyjnego JSON.
        /// </summary>
        /// <param name="newIp">Nowy adres IP.</param>
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
    }
}