using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PhoneToLinux.Core;
using PhoneToLinux.Plugins;

namespace phonetolinux.Services
{
    /// <summary>
    /// DTO object representing data for a single conversation fetched from the server.
    /// Supports mapping for multiple field names (number/address) emitted by the Android server.
    /// </summary>
    public class ConversationDto
    {
        /// <summary>Contact name or identifier.</summary>
        [JsonPropertyName("contactName")]
        public string ContactName { get; set; } = "";

        /// <summary>Primary phone number associated with the conversation.</summary>
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; } = "";

        /// <summary>Fallback field for phone number from Android payload.</summary>
        [JsonPropertyName("number")]
        public string Number { set => PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? value : PhoneNumber; }

        /// <summary>Fallback field for address from Android payload.</summary>
        [JsonPropertyName("address")]
        public string Address { set => PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? value : PhoneNumber; }

        /// <summary>Content of the last message in the conversation.</summary>
        [JsonPropertyName("lastMessage")]
        public string LastMessage { get; set; } = "";

        /// <summary>Timestamp of the last message in milliseconds.</summary>
        [JsonPropertyName("date")]
        public long Date { get; set; }

        /// <summary>Indicates whether the last message has been read.</summary>
        [JsonPropertyName("isRead")]
        public bool IsRead { get; set; } = true;
    }

    /// <summary>
    /// Plugin responsible for fetching the list of recent conversations from the mobile device
    /// via an HTTP request to the server, deserializing JSON responses, and deduplicating entries.
    /// </summary>
    public class ConversationsPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationsPlugin"/> class.
        /// </summary>
        /// <param name="httpClient">Optional custom HttpClient instance. Uses a default client if none provided.</param>
        public ConversationsPlugin(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

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
        /// Asynchronously fetches the list of conversations from the phone server and deduplicates threads by phone number.
        /// </summary>
        /// <returns>A list of deduplicated ConversationDto objects.</returns>
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
                var conversations = JsonSerializer.Deserialize<List<ConversationDto>>(json, options) 
                                    ?? new List<ConversationDto>();

                // Deduplicate conversation entries by normalized phone number
                return conversations
                    .Where(c => !string.IsNullOrWhiteSpace(c.PhoneNumber))
                    .DistinctBy(c => NormalizePhoneNumber(c.PhoneNumber))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONVERSATIONS ERROR] Failed to fetch conversations from server: {ex.Message}");
                return new List<ConversationDto>();
            }
        }

        /// <summary>
        /// Normalizes phone numbers to standard 9-digit format for robust comparison.
        /// </summary>
        private static string NormalizePhoneNumber(string number)
        {
            var digits = new string(number.Where(char.IsDigit).ToArray());
            return digits.Length > 9 ? digits.Substring(digits.Length - 9) : digits;
        }
    }
}