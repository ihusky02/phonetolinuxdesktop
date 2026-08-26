using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using phonetolinux.Models;
using PhoneToLinux.Core;
using PhoneToLinux.Plugins;

namespace phonetolinux.Services
{
    /// <summary>
    /// Plugin responsible for fetching SMS conversation history for a specific phone number 
    /// from the mobile server via HTTP requests.
    /// </summary>
    public class SmsHistoryPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsHistoryPlugin"/> class.
        /// </summary>
        /// <param name="httpClient">Optional custom HttpClient instance. Uses a default client if none provided.</param>
        public SmsHistoryPlugin(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <inheritdoc />
        public string Endpoint => "/chathistory";

        /// <summary>
        /// Executes the plugin operation for a given query parameter.
        /// </summary>
        /// <param name="queryParams">Query parameters.</param>
        /// <returns>Response in JSON format.</returns>
        public string Execute(string queryParams)
        {
            return $"{{\"status\":\"SmsHistoryPlugin active\", \"query\":\"{queryParams}\"}}";
        }

        /// <summary>
        /// Asynchronously fetches chat history from the phone server for the specified phone number
        /// and removes potential duplicate entries.
        /// </summary>
        /// <param name="phoneNumber">Target phone number to fetch history for.</param>
        /// <returns>A deduplicated list of chat message items.</returns>
        public async Task<List<ChatMessageItem>> GetChatHistoryFromServerAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrEmpty(PhoneConfig.PhoneIp))
            {
                return new List<ChatMessageItem>();
            }

            try
            {
                string encodedNumber = Uri.EscapeDataString(phoneNumber);
                string url = $"{PhoneConfig.GetBaseUrl()}/chathistory?number={encodedNumber}";
                
                string json = await _httpClient.GetStringAsync(url);
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var messages = JsonSerializer.Deserialize<List<ChatMessageItem>>(json, options) 
                               ?? new List<ChatMessageItem>();

                // Deduplicate messages based on text content and direction flag
                return messages
                    .DistinctBy(m => new { m.Text, m.IsOutgoing })
                    .ToList();
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"[HISTORY ERROR] Failed to fetch history for '{phoneNumber}': {ex.Message}");
                return new List<ChatMessageItem>(); 
            }
        }
    }
}