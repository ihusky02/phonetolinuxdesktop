using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PhoneToLinux.Core;
using PhoneToLinux.Plugins;

namespace phonetolinux.Services
{
    /// <summary>
    /// Obiekt DTO reprezentujący dane pojedynczej konwersacji pobieranej z serwera.
    /// </summary>
    public class ConversationDto
    {
        /// <summary>Nazwa lub identyfikator kontaktu.</summary>
        public string contactName { get; set; } = "";

        /// <summary>Numer telefonu powiązany z konwersacją.</summary>
        public string phoneNumber { get; set; } = "";

        /// <summary>Treść ostatniej wiadomości w konwersacji.</summary>
        public string lastMessage { get; set; } = "";
    }

    /// <summary>
    /// Wtyczka odpowiedzialna za pobieranie listy ostatnich konwersacji z urządzenia mobilnego
    /// za pośrednictwem żądania HTTP do serwera.
    /// </summary>
    public class ConversationsPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient = new();

        /// <inheritdoc />
        public string Endpoint => "/conversations";

        /// <summary>
        /// Wykonuje operację wtyczki dla zadanego zapytania.
        /// </summary>
        /// <param name="queryParams">Parametry zapytania.</param>
        /// <returns>Odpowiedź w formacie JSON.</returns>
        public string Execute(string queryParams)
        {
            return "{\"status\":\"ConversationsPlugin active\"}";
        }

        /// <summary>
        /// Asynchronicznie pobiera listę konwersacji z serwera telefonu.
        /// </summary>
        /// <returns>Lista obiektów ConversationDto reprezentujących konwersacje.</returns>
        public async Task<List<ConversationDto>> GetConversationsFromServerAsync()
        {
            try
            {
                string url = $"{PhoneConfigPlugin.GetBaseUrl()}/conversations";
                Console.WriteLine($"[DEBUG CONVERSATIONS] Pobieram z: {url}");
                
                string json = await _httpClient.GetStringAsync(url);
                Console.WriteLine($"[DEBUG CONVERSATIONS] Odpowiedź JSON: {json}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<ConversationDto>>(json, options) ?? new List<ConversationDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONVERSATIONS ERROR] Nie udało się pobrać konwersacji z serwera: {ex.Message}");
                return new List<ConversationDto>();
            }
        }
    }
}