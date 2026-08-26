using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PhoneToLinux.Core;
using PhoneToLinux.Plugins;

namespace phonetolinux.Services
{
    /// <summary>
    /// DTO object representing data for a single conversation fetched from the server.
    /// </summary>
    public class ConversationDto
    {
        /// <summary>Contact name or identifier.</summary>
        public string contactName { get; set; } = "";

        /// <summary>Phone number associated with the conversation.</summary>
        public string phoneNumber { get; set; } = "";

        /// <summary>Content of the last message in the conversation.</summary>
        public string lastMessage { get; set; } = "";
    }

    /// <summary>
    /// Plugin responsible for fetching the list of recent conversations from the mobile device
    /// via an HTTP request to the server.
    /// </summary>
    public class ConversationsPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient = new();

        /// <inheritdoc />
        public string Endpoint => "/conversations";

        /// <summary>
        /// Executes the plugin operation for a given query.
        /// </summary>
        /// <param name="queryParams">Query parameters.</param>
        /// <returns>Response in JSON format.</returns>
        public string Execute(string queryParams)
        {
            return "{\"status\":\"ConversationsPlugin active\"}";
        }

        /// <summary>
        /// Asynchronously fetches the list of conversations from the phone server.
        /// </summary>
        /// <returns>A list of ConversationDto objects representing the conversations.</returns>
        public async Task<List<ConversationDto>> GetConversationsFromServerAsync()
        {
            try
            {
                // Guard check to prevent invalid URI if phone IP is not yet set
                if (string.IsNullOrEmpty(PhoneConfig.PhoneIp))
                {
                    return new List<ConversationDto>();
                }

                string url = $"{PhoneConfig.GetBaseUrl()}/conversations";
                Console.WriteLine($"[DEBUG CONVERSATIONS] Fetching from: {url}");
                
                string json = await _httpClient.GetStringAsync(url);
                Console.WriteLine($"[DEBUG CONVERSATIONS] JSON response: {json}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<ConversationDto>>(json, options) ?? new List<ConversationDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONVERSATIONS ERROR] Failed to fetch conversations from server: {ex.Message}");
                return new List<ConversationDto>();
            }
        }
    }
}