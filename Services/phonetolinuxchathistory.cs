using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using phonetolinux.ViewModels;

namespace phonetolinux.Services
{
    public class phonetolinuxchathistory
    {
        private readonly string _storageDir;

        public phonetolinuxchathistory()
        {
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _storageDir = Path.Combine(homeDir, ".phonetolinux", "chats");
            Directory.CreateDirectory(_storageDir);
        }

        private string GetFilePath(string contactName)
        {
            string safeName = string.Concat(contactName.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_storageDir, $"{safeName}.json");
        }

        public async Task<List<ChatMessageItem>> LoadHistoryAsync(string contactName)
        {
            try
            {
                string filePath = GetFilePath(contactName);
                if (!File.Exists(filePath)) return new List<ChatMessageItem>();

                string json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<List<ChatMessageItem>>(json) ?? new List<ChatMessageItem>();
            }
            catch (Exception)
            {
                return new List<ChatMessageItem>();
            }
        }

        public async Task SaveHistoryAsync(string contactName, IEnumerable<ChatMessageItem> messages)
        {
            try
            {
                string filePath = GetFilePath(contactName);
                string json = JsonSerializer.Serialize(messages, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception)
            {
                // Ignoruj błędy zapisu
            }
        }
    }
}