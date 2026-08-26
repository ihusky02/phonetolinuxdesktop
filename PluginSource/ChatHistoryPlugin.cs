using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using phonetolinux.Models;
using PhoneToLinux.Core;

namespace phonetolinux.Services
{
    /// <summary>
    /// Wtyczka odpowiedzialna za lokalne zarządzanie historią konwersacji (odczyt oraz zapis wiadomości do plików JSON)
    /// w katalogu domowym użytkownika.
    /// </summary>
    public class ChatHistoryPlugin : IPhonePlugin
    {
        /// <summary>Katalog docelowy przechowujący lokalne pliki historii czatów.</summary>
        private static readonly string StorageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".phonetolinux", "chats");

        /// <inheritdoc />
        public string Endpoint => "/chathistory";

        /// <summary>
        /// Wykonuje główne zadanie wtyczki dla zadanego zapytania.
        /// </summary>
        /// <param name="queryParams">Parametry zapytania.</param>
        /// <returns>Odpowiedź w formacie JSON.</returns>
        public string Execute(string queryParams)
        {
            return "{\"status\":\"ChatHistoryPlugin active\"}";
        }

        /// <summary>
        /// Asynchronicznie wczytuje historię wiadomości dla wskazanego identyfikatora (kontaktu lub numeru telefonu).
        /// </summary>
        /// <param name="identifier">Nazwa kontaktu lub numer telefonu.</param>
        /// <returns>Lista elementów wiadomości czatu.</returns>
        public async Task<List<ChatMessageItem>> LoadHistoryAsync(string identifier)
        {
            try
            {
                string safeName = string.Join("_", identifier.Split(Path.GetInvalidFileNameChars()));
                string filePath = Path.Combine(StorageDir, $"{safeName}.json");

                if (File.Exists(filePath))
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<ChatMessageItem>>(json, options) ?? new List<ChatMessageItem>();
                }
            }
            catch { }
            return new List<ChatMessageItem>();
        }

        /// <summary>
        /// Asynchronicznie zapisuje historię wiadomości dla wskazanego identyfikatora do pliku JSON.
        /// </summary>
        /// <param name="identifier">Nazwa kontaktu lub numer telefonu.</param>
        /// <param name="messages">Kolekcja wiadomości do zapisu.</param>
        public async Task SaveHistoryAsync(string identifier, IEnumerable<ChatMessageItem> messages)
        {
            try
            {
                Directory.CreateDirectory(StorageDir);
                string safeName = string.Join("_", identifier.Split(Path.GetInvalidFileNameChars()));
                string filePath = Path.Combine(StorageDir, $"{safeName}.json");

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(messages, options);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch { }
        }
    }
}