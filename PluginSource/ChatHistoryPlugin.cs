using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using phonetolinux.Models;
using PhoneToLinux.Core;

namespace phonetolinux.Services
{
    /// <summary>
    /// Plugin responsible for fetching chat history from the phone server 
    /// and managing local persistence in JSON files.
    /// </summary>
    public class ChatHistoryPlugin : IPhonePlugin
    {
        private static readonly string StorageDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            ".phonetolinux", 
            "chats"
        );

        /// <inheritdoc />
        public string Endpoint => "/chathistory";

        /// <summary>
        /// Executes the main plugin task for a given query parameter.
        /// </summary>
        /// <param name="queryParams">Query parameters containing target phone number (e.g., "number=+48500100200").</param>
        /// <returns>Response in JSON format.</returns>
        public string Execute(string queryParams)
        {
            return $"{{\"status\":\"ChatHistoryPlugin active\", \"query\":\"{queryParams}\"}}";
        }

        /// <summary>
        /// Asynchronously fetches chat history from the remote phone server via HTTP endpoint.
        /// </summary>
        /// <param name="httpClient">Active HttpClient instance connected to the phone.</param>
        /// <param name="number">Phone number to retrieve messages for.</param>
        /// <returns>A list of deduplicated chat messages.</returns>
        public async Task<List<ChatMessageItem>> FetchRemoteHistoryAsync(HttpClient httpClient, string number)
        {
            if (httpClient == null || string.IsNullOrWhiteSpace(number))
                return new List<ChatMessageItem>();

            try
            {
                string encodedNumber = Uri.EscapeDataString(number);
                string responseJson = await httpClient.GetStringAsync($"{Endpoint}?number={encodedNumber}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var messages = JsonSerializer.Deserialize<List<ChatMessageItem>>(responseJson, options) 
                               ?? new List<ChatMessageItem>();

                // Filter out duplicate messages by text body and direction flag
                return messages
                    .DistinctBy(m => new { m.Text, m.IsOutgoing })
                    .ToList();
            }
            catch
            {
                return new List<ChatMessageItem>();
            }
        }

        /// <summary>
        /// Asynchronously loads message history for the specified identifier from local storage.
        /// </summary>
        /// <param name="identifier">Contact name or phone number.</param>
        /// <returns>A list of deduplicated chat message items.</returns>
        public async Task<List<ChatMessageItem>> LoadHistoryAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return new List<ChatMessageItem>();

            try
            {
                string safeName = string.Join("_", identifier.Split(Path.GetInvalidFileNameChars()));
                string filePath = Path.Combine(StorageDir, $"{safeName}.json");

                if (File.Exists(filePath))
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var loadedMessages = JsonSerializer.Deserialize<List<ChatMessageItem>>(json, options) 
                                         ?? new List<ChatMessageItem>();

                    // Filter out duplicate entries upon reading
                    return loadedMessages
                        .DistinctBy(m => new { m.Text, m.IsOutgoing })
                        .ToList();
                }
            }
            catch { }

            return new List<ChatMessageItem>();
        }

        /// <summary>
        /// Asynchronously saves message history for the specified identifier to a local JSON file.
        /// Deduplicates items before persisting.
        /// </summary>
        /// <param name="identifier">Contact name or phone number.</param>
        /// <param name="messages">Collection of messages to save.</param>
        public async Task SaveHistoryAsync(string identifier, IEnumerable<ChatMessageItem> messages)
        {
            if (string.IsNullOrWhiteSpace(identifier) || messages == null)
                return;

            try
            {
                Directory.CreateDirectory(StorageDir);
                string safeName = string.Join("_", identifier.Split(Path.GetInvalidFileNameChars()));
                string filePath = Path.Combine(StorageDir, $"{safeName}.json");

                // Ensure unique messages before serializing to disk
                var uniqueMessages = messages
                    .DistinctBy(m => new { m.Text, m.IsOutgoing })
                    .ToList();

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(uniqueMessages, options);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch { }
        }
    }
}