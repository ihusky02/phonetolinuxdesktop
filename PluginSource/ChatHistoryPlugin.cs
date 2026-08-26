using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using phonetolinux.Models;
using PhoneToLinux.Core;

namespace phonetolinux.Services
{
    /// <summary>
    /// Plugin responsible for local management of chat history (reading and writing message data to JSON files)
    /// within the user's home directory.
    /// </summary>
    public class ChatHistoryPlugin : IPhonePlugin
    {
        /// <summary>Target directory storing local chat history files.</summary>
        private static readonly string StorageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".phonetolinux", "chats");

        /// <inheritdoc />
        public string Endpoint => "/chathistory";

        /// <summary>
        /// Executes the main plugin task for a given query.
        /// </summary>
        /// <param name="queryParams">Query parameters.</param>
        /// <returns>Response in JSON format.</returns>
        public string Execute(string queryParams)
        {
            return "{\"status\":\"ChatHistoryPlugin active\"}";
        }

        /// <summary>
        /// Asynchronously loads message history for the specified identifier (contact name or phone number).
        /// </summary>
        /// <param name="identifier">Contact name or phone number.</param>
        /// <returns>A list of chat message items.</returns>
        public async Task<List<ChatMessageItem>> LoadHistoryAsync(string identifier)
        {
            try
            {
                string safeName = string.Join("_", identifier.Split(Path.GetInvalidFileNameChars()));
                string filePath = Path.Combine(StorageDir, $"{safeName}.json");

                if (File.Exists(filePath))
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<ChatMessageItem>>(json, options) ?? new List<ChatMessageItem>();
                }
            }
            catch { }
            return new List<ChatMessageItem>();
        }

        /// <summary>
        /// Asynchronously saves message history for the specified identifier to a JSON file.
        /// </summary>
        /// <param name="identifier">Contact name or phone number.</param>
        /// <param name="messages">Collection of messages to save.</param>
        public async Task SaveHistoryAsync(string identifier, IEnumerable<ChatMessageItem> messages)
        {
            try
            {
                Directory.CreateDirectory(StorageDir);
                string safeName = string.Join("_", identifier.Split(Path.GetInvalidFileNameChars()));
                string filePath = Path.Combine(StorageDir, $"{safeName}.json");

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(messages, options);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch { }
        }
    }
}