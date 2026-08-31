using System;
using System.Net.Http;
using System.Threading.Tasks;
using PhoneToLinux.Core;
using PhoneToLinux.Plugins;

namespace phonetolinux.Services
{
    public class CallsPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient;

        public CallsPlugin(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public string Endpoint => "/call";

        public string Execute(string queryParams)
        {
            return "{\"status\":\"CallsPlugin active\"}";
        }

        public async Task<bool> MakeCallAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrEmpty(PhoneConfig.PhoneIp))
                return false;

            try
            {
                string encodedNumber = Uri.EscapeDataString(phoneNumber.Trim());
                string url = $"{PhoneConfig.GetBaseUrl()}/call?number={encodedNumber}&action=dial";

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CALLS ERROR] Failed to initiate call: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EndCallAsync()
        {
            if (string.IsNullOrEmpty(PhoneConfig.PhoneIp))
                return false;

            try
            {
                string url = $"{PhoneConfig.GetBaseUrl()}/call?action=end";

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CALLS ERROR] Failed to end call: {ex.Message}");
                return false;
            }
        }
    }
}