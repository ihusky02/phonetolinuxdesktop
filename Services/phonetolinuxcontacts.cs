using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace phonetolinux.Services
{
    public class PhoneContact
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        // Poprawiono klucz na "phone", który zwraca serwer telefonu
        [JsonPropertyName("phone")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class PhonetoLinuxContacts
    {
        private readonly HttpClient _httpClient;

        public PhonetoLinuxContacts()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        private string GetBaseUrl() => PhoneConfig.GetBaseUrl();

        // Pobieranie listy kontaktów z serwera na Androidzie
        public async Task<List<PhoneContact>> GetContactsAsync()
        {
            try
            {
                string url = $"{GetBaseUrl()}/contacts";
                string jsonResponse = await _httpClient.GetStringAsync(url);

                // WYŚWIETLAMY PIERWSZE 500 ZNAKÓW JSON-A W KONSOLI TERMINALA
                Console.WriteLine($"[DEBUG RAW JSON]: {jsonResponse.Substring(0, Math.Min(jsonResponse.Length, 500))}");

                var contacts = JsonSerializer.Deserialize<List<PhoneContact>>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return contacts ?? new List<PhoneContact>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas pobierania kontaktów: {ex.Message}");
                return new List<PhoneContact>();
            }
        }
    }
}