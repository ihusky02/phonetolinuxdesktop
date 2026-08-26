using System;
using System.Net.Http;
using System.Threading.Tasks;
using PhoneToLinux.Core;
using PhoneToLinux.Plugins;

namespace phonetolinux.Services
{
    /// <summary>
    /// Plugin responsible for sending SMS messages via HTTP requests to the mobile device server.
    /// </summary>
    public class SmsPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };

        /// <inheritdoc />
        public string Endpoint => "/send_sms";

        /// <summary>
        /// Executes the plugin operation for a given query.
        /// </summary>
        /// <param name="queryParams">Query parameters.</param>
        /// <returns>Response in JSON format.</returns>
        public string Execute(string queryParams)
        {
            return "{\"status\":\"SmsPlugin active\"}";
        }

        /// <summary>
        /// Asynchronously sends an SMS message to the specified phone number.
        /// </summary>
        /// <param name="phoneNumber">Recipient's phone number.</param>
        /// <param name="message">SMS message body.</param>
        /// <returns>True if the message was sent successfully; otherwise, false.</returns>
        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                string encodedNumber = Uri.EscapeDataString(phoneNumber);
                string encodedMessage = Uri.EscapeDataString(message);
                string url = $"{PhoneConfigPlugin.GetBaseUrl()}/send_sms?number={encodedNumber}&message={encodedMessage}";

                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"SMS server error: {(int)response.StatusCode}, Response: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"Network error while sending SMS: {ex.Message}");
                return false; 
            }
        }
    }
}