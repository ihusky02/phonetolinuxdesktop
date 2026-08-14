using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using phonetolinux.ViewModels;

namespace phonetolinux.Services
{
    public class phonetolinuxsmshistory
    {
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        private string GetBaseUrl() => PhoneConfig.GetBaseUrl();

        public async Task<List<ChatMessageItem>> GetChatHistoryFromServerAsync(string phoneNumber)
        {
            try
            {
                string url = $"{GetBaseUrl()}/chat-history";
                var payload = JsonSerializer.Serialize(new { phone = phoneNumber });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<ChatMessageItem>>(json, options) ?? new List<ChatMessageItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd pobierania historii SMS z telefonu: {ex.Message}");
            }

            return new List<ChatMessageItem>();
        }
    }
}