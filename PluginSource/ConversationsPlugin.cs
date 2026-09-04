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
        [JsonPropertyName("contactName")]
        public string contactName { get; set; } = "";

        [JsonIgnore]
        public string ContactName 
        { 
            get => contactName; 
            set => contactName = value; 
        }

        [JsonPropertyName("phoneNumber")]
        public string phoneNumber { get; set; } = "";

        [JsonIgnore]
        public string PhoneNumber 
        { 
            get => phoneNumber; 
            set => phoneNumber = value; 
        }

        [JsonPropertyName("number")]
        public string Number 
        { 
            set => phoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? value : phoneNumber; 
        }

        [JsonPropertyName("address")]
        public string Address 
        { 
            set => phoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? value : phoneNumber; 
        }

        [JsonPropertyName("lastMessage")]
        public string lastMessage { get; set; } = "";

        [JsonPropertyName("date")]
        public long date { get; set; }

        [JsonPropertyName("isRead")]
        public bool isRead { get; set; } = true;
    }

    /// <summary>
    /// Plugin responsible for fetching and managing recent conversations from the mobile device
    /// via HTTP requests, deserializing JSON responses, and handling deletion commands.
    /// </summary>
    public class ConversationsPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient;

        public ConversationsPlugin(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public string Endpoint => "/conversations";

        public string Execute(string queryParams)
        {
            return "{\"status\":\"ConversationsPlugin active\"}";
        }

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

                foreach (var conv in conversations)
                {
                    if (string.IsNullOrWhiteSpace(conv.phoneNumber) && !string.IsNullOrWhiteSpace(conv.contactName))
                    {
                        conv.phoneNumber = conv.contactName;
                    }
                }

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
        /// Asynchronously sends an HTTP DELETE request to the Android server to remove an SMS thread by address.
        /// </summary>
        public async Task<bool> DeleteConversationAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrEmpty(PhoneConfig.PhoneIp))
            {
                Console.WriteLine("[DELETE ERROR] Missing address or phone IP configuration.");
                return false;
            }

            try
            {
                string encodedAddress = Uri.EscapeDataString(address.Trim());
                
                // Poprawiona ścieżka kierująca bezpośrednio do nowego endpointu usuwania na Androidzie
                string url = $"{PhoneConfig.GetBaseUrl()}/delete_conversation?address={encodedAddress}";
                
                Console.WriteLine($"[DELETE DEBUG] Dispatching DELETE request to URL: {url}");

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DELETE DEBUG] Response status: {(int)response.StatusCode}, Body: {responseBody}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONVERSATIONS ERROR] Failed to delete conversation '{address}': {ex.Message}");
                return false;
            }
        }

        private static string NormalizeSenderKey(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return string.Empty;

            if (identifier.Any(char.IsLetter))
            {
                return identifier.Trim().ToLowerInvariant();
            }

            var digits = new string(identifier.Where(char.IsDigit).ToArray());
            return digits.Length > 9 ? digits.Substring(digits.Length - 9) : digits;
        }
    }
}