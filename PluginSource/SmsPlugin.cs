using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PhoneToLinux.Core;
using PhoneToLinux.Plugins;

namespace phonetolinux.Services
{
    /// <summary>
    /// Plugin responsible for dispatching outgoing SMS messages via HTTP GET requests 
    /// to the mobile device's endpoint.
    /// </summary>
    public class SmsPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsPlugin"/> class.
        /// </summary>
        /// <param name="httpClient">Optional custom HttpClient instance. Uses a default client with 5s timeout if none provided.</param>
        public SmsPlugin(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        }

        /// <inheritdoc />
        public string Endpoint => "/send_sms";

        /// <summary>
        /// Executes the plugin operation for a given query parameter.
        /// </summary>
        /// <param name="queryParams">Query parameters.</param>
        /// <returns>Response in JSON format indicating execution status.</returns>
        public string Execute(string queryParams)
        {
            if (string.IsNullOrWhiteSpace(queryParams))
            {
                return "{\"status\":\"SmsPlugin active\"}";
            }

            try
            {
                return $"{{\"status\":\"SmsPlugin active\", \"query\":\"{queryParams}\"}}";
            }
            catch (Exception ex)
            {
                return $"{{\"status\":\"error\", \"message\":\"{ex.Message}\"}}";
            }
        }

        /// <summary>
        /// Asynchronously sends an SMS message to the specified destination number via HTTP GET.
        /// </summary>
        /// <param name="phoneNumber">Recipient's phone number.</param>
        /// <param name="message">SMS message text content.</param>
        /// <returns>True if the message was dispatched successfully by the mobile device; otherwise, false.</returns>
        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message) || string.IsNullOrEmpty(PhoneConfig.PhoneIp))
            {
                Console.WriteLine("[SMS ERROR] Missing phone number, message text, or phone IP configuration.");
                return false;
            }

            try
            {
                string cleanNumber = phoneNumber.Trim();
                string cleanMessage = message.Trim();

                string encodedNumber = Uri.EscapeDataString(cleanNumber);
                string encodedMessage = Uri.EscapeDataString(cleanMessage);
                
                string url = $"{PhoneConfig.GetBaseUrl()}/send_sms?number={encodedNumber}&message={encodedMessage}";
                Console.WriteLine($"[SMS] Dispatching request to phone: {url}");

                using var response = await _httpClient.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[SMS] Response status: {(int)response.StatusCode}, Body: {responseBody}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[SMS ERROR] HTTP Server Error: {(int)response.StatusCode}");
                    return false;
                }

                // Verify the JSON response returned by Android SendSmsEndpoint ("{"status":"success"}")
                using var jsonDoc = JsonDocument.Parse(responseBody);
                if (jsonDoc.RootElement.TryGetProperty("status", out var statusProp))
                {
                    string? statusVal = statusProp.GetString();
                    return statusVal != null && statusVal.Equals("success", StringComparison.OrdinalIgnoreCase);
                }

                return true;
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"[SMS ERROR] Network exception while sending SMS: {ex.Message}");
                return false; 
            }
        }
    }
}