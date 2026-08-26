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
        /// <param name="queryParams">Query parameters (e.g., "number=+48500100200&amp;message=Hello").</param>
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
        /// Asynchronously sends an SMS message to the specified destination number.
        /// </summary>
        /// <param name="phoneNumber">Recipient's phone number.</param>
        /// <param name="message">SMS message text content.</param>
        /// <returns>True if the message was dispatched successfully by the mobile device; otherwise, false.</returns>
        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message) || string.IsNullOrEmpty(PhoneConfig.PhoneIp))
            {
                return false;
            }

            try
            {
                string encodedNumber = Uri.EscapeDataString(phoneNumber);
                string encodedMessage = Uri.EscapeDataString(message);
                string url = $"{PhoneConfig.GetBaseUrl()}/send_sms?number={encodedNumber}&message={encodedMessage}";

                using var response = await _httpClient.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[SMS ERROR] HTTP Server Error: {(int)response.StatusCode}, Response: {responseBody}");
                    return false;
                }

                // Verify the JSON response returned by Android SendSmsEndpoint ("{"status":"success"}")
                using var jsonDoc = JsonDocument.Parse(responseBody);
                if (jsonDoc.RootElement.TryGetProperty("status", out var statusProp))
                {
                    return statusProp.GetString()?.Equals("success", StringComparison.OrdinalIgnoreCase) ?? false;
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