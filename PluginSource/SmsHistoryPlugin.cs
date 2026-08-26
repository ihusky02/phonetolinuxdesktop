using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using phonetolinux.Models;
using PhoneToLinux.Core;
using PhoneToLinux.Plugins;

namespace phonetolinux.Services
{
    /// <summary>
    /// Plugin responsible for fetching SMS conversation history for a specific phone number from the mobile server.
    /// </summary>
    public class SmsHistoryPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient = new();

        /// <inheritdoc />
        public string Endpoint => "/chathistory";

        /// <summary>
        /// Executes the plugin operation for a given query.
        /// </summary>
        /// <param name="queryParams">Query parameters.</param>
        /// <returns>Response in JSON format.</returns>
        public string Execute(string queryParams)
        {
            return "{\"status\":\"SmsHistoryPlugin active\"}";
        }

        /// <summary>
        /// Asynchronously fetches chat history from the phone server for the specified phone number.
        /// </summary>
        /// <param name="phoneNumber">Phone number to fetch history for.</param>
        /// <returns>List of chat message items.</returns>
        public async Task<List<ChatMessageItem>> GetChatHistoryFromServerAsync(string phoneNumber)
        {
            try
            {
                // Guard check to prevent invalid URI if phone IP is not yet set
                if (string.IsNullOrEmpty(PhoneConfig.PhoneIp))
                {
                    return new List<ChatMessageItem>();
                }

                string url = $"{PhoneConfig.GetBaseUrl()}/chathistory?number={Uri.EscapeDataString(phoneNumber)}";
                string json = await _httpClient.GetStringAsync(url);
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var messages = JsonSerializer.Deserialize<List<ChatMessageItem>>(json, options) ?? new List<ChatMessageItem>();

                return messages;
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"[HISTORY ERROR] {ex.Message}");
                return new List<ChatMessageItem>(); 
            }
        }
    }
}