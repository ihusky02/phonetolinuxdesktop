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
        public string contactName { get; set; } = "";

        /// <summary>PascalCase property alias for C# codebase consistency.</summary>
        [JsonIgnore]
        public string ContactName 
        { 
            get => contactName; 
            set => contactName = value; 
        }

        /// <summary>Primary phone number or sender address associated with the conversation.</summary>
        [JsonPropertyName("phoneNumber")]
        public string phoneNumber { get; set; } = "";

        /// <summary>PascalCase property alias for C# codebase consistency.</summary>
        [JsonIgnore]
        public string PhoneNumber 
        { 
            get => phoneNumber; 
            set => phoneNumber = value; 
        }

        /// <summary>Fallback field for phone number from Android payload.</summary>
        [JsonPropertyName("number")]
        public string Number 
        { 
            set => phoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? value : phoneNumber; 
        }

        /// <summary>Fallback field for address from Android payload.</summary>
        [JsonPropertyName("address")]
        public string Address 
        { 
            set => phoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? value : phoneNumber; 
        }

        /// <summary>Content of the last message in the conversation.</summary>
        [JsonPropertyName("lastMessage")]
        public string lastMessage { get; set; } = "";

        /// <summary>Timestamp of the last message in milliseconds.</summary>
        [JsonPropertyName("date")]
        public long date { get; set; }

        /// <summary>Indicates whether the last message has been read.</summary>
        [JsonPropertyName("isRead")]
        public bool isRead { get; set; } = true;
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
        /// Asynchronously fetches the list of conversations from the phone server and deduplicates threads.
        /// Properly handles numeric phone numbers as well as alphanumeric sender IDs (e.g., mObywatel, Kaufland).
        /// </summary>
        /// <returns>A list of deduplicated ConversationDto objects.</returns>
        public async Task<List<ConversationDto>> GetConversationsFromServerAsync()
        {
            try
            {
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

                // Ensure fallback value from ContactName if PhoneNumber/Address is empty
                foreach (var conv in conversations)
                {
                    if (string.IsNullOrWhiteSpace(conv.phoneNumber) && !string.IsNullOrWhiteSpace(conv.contactName))
                    {
                        conv.phoneNumber = conv.contactName;
                    }
                }

                // Deduplicate conversation entries safely supporting both numeric and text senders
                return conversations
                    .Where(c => !string.IsNullOrWhiteSpace(c.phoneNumber) || !string.IsNullOrWhiteSpace(c.contactName))
                    .DistinctBy(c => NormalizeSenderKey(c.phoneNumber))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONVERSATIONS ERROR] Failed to fetch conversations from server: {ex.Message}");
                return new List<ConversationDto>();
            }
        }

        /// <summary>
        /// Normalizes sender keys for deduplication. Extracts last 9 digits for phone numbers,
        /// or retains lower-cased verbatim strings for alphanumeric sender IDs.
        /// </summary>
        private static string NormalizeSenderKey(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return string.Empty;

            // Alphanumeric Sender IDs (e.g. Kaufland, Globania, mObywatel)
            if (identifier.Any(char.IsLetter))
            {
                return identifier.Trim().ToLowerInvariant();
            }

            // Standard numeric phone numbers
            var digits = new string(identifier.Where(char.IsDigit).ToArray());
            return digits.Length > 9 ? digits.Substring(digits.Length - 9) : digits;
        }
    }
}