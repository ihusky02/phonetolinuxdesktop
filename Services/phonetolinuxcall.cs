using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace phonetolinux.Services
{
    public class PhonetoLinuxCall
    {
        private readonly HttpClient _httpClient;

        public PhonetoLinuxCall()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        // Korzystamy z centralnej konfiguracji adresu IP
        private string GetBaseUrl() => PhoneConfig.GetBaseUrl();

        // Wywoływanie połączenia wychodzącego
        public async Task<bool> StartCallAsync(string phoneNumber)
        {
            try
            {
                string url = $"{GetBaseUrl()}/call";
                
                var payload = new { phone = phoneNumber };
                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas nawiązywania połączenia: {ex.Message}");
                return false;
            }
        }

        // Odbieranie połączenia przychodzącego
        public async Task<bool> AnswerCallAsync()
        {
            try
            {
                string url = $"{GetBaseUrl()}/answer-call";
                HttpResponseMessage response = await _httpClient.PostAsync(url, null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas odbierania połączenia: {ex.Message}");
                return false;
            }
        }

        // Kończenie / odrzucanie połączenia
        public async Task<bool> EndCallAsync()
        {
            try
            {
                string url = $"{GetBaseUrl()}/end-call";
                HttpResponseMessage response = await _httpClient.PostAsync(url, null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas kończenia połączenia: {ex.Message}");
                return false;
            }
        }
    }
}