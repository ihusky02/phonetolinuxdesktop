using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using phonetolinux.ViewModels;

namespace phonetolinux.Services;

public class phonetolinuxchathistory
{
    private static readonly string StorageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".phonetolinux", "chats");

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