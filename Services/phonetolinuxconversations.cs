using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace phonetolinux.Services
{
    public class ConversationItemDto
    {
        public string contactName { get; set; } = "";
        public string lastMessage { get; set; } = "";
        public string phoneNumber { get; set; } = "";
    }

    public class phonetolinuxconversations
    {
        private readonly HttpClient _httpClient;

        public phonetolinuxconversations()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        private string GetBaseUrl() => PhoneConfig.GetBaseUrl();

        public async Task<List<ConversationItemDto>> GetConversationsFromServerAsync()
        {
            try
            {
                string url = $"{GetBaseUrl()}/conversations";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<ConversationItemDto>>(jsonResponse, options) ?? new List<ConversationItemDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd pobierania konwersacji: {ex.Message}");
            }

            return new List<ConversationItemDto>();
        }
    }
}