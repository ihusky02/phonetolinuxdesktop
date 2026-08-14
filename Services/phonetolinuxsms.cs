using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace phonetolinux.Services
{
    public class IncomingSmsMessage
    {
        public string sender { get; set; }
        public string text { get; set; }
    }

    public class PhonetoLinuxSMS
    {
        private readonly HttpClient _httpClient;

        public PhonetoLinuxSMS()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        private string GetBaseUrl() => PhoneConfig.GetBaseUrl();

        // Wysyłanie wiadomości SMS
        public async Task<bool> SendSmsAsync(string phoneNumber, string messageText)
        {
            try
            {
                string url = $"{GetBaseUrl()}/send-sms";
                
                var payload = new 
                { 
                    phone = phoneNumber, 
                    message = messageText 
                };
                
                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[SMS ERROR] Status: {response.StatusCode}, Odpowiedź: {errorDetails}, URL: {url}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas wysyłania SMS-a: {ex.Message}");
                return false;
            }
        }

        // Pobieranie przychodzących SMS-ów z telefonu (dla widoku desktopowego w Avalonia)
        public async Task<List<IncomingSmsMessage>> GetIncomingSmsAsync()
        {
            try
            {
                string url = $"{GetBaseUrl()}/incoming-sms";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<IncomingSmsMessage>>(jsonResponse, options) ?? new List<IncomingSmsMessage>();
                }
            }
            catch (Exception)
            {
                // Ignorujemy chwilowe błędy sieciowe lub stan IDLE w tle, gdy telefon jest nieaktywny
            }

            return new List<IncomingSmsMessage>();
        }
    }
}