using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using phonetolinux.ViewModels;

namespace phonetolinux.Services;

public class phonetolinuxsmshistory
{
    private readonly HttpClient _httpClient = new();

    public async Task<List<ChatMessageItem>> GetChatHistoryFromServerAsync(string phoneNumber)
    {
        try
        {
            string url = $"{PhoneConfig.GetBaseUrl()}/chathistory?number={Uri.EscapeDataString(phoneNumber)}";
            string json = await _httpClient.GetStringAsync(url);
            
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var messages = JsonSerializer.Deserialize<List<ChatMessageItem>>(json, options) ?? new List<ChatMessageItem>();

            // DEBUG: Wypiszmy w konsoli co dokładnie przychodzi z serwera
            Console.WriteLine($"[DEBUG HISTORY] Pobrano {messages.Count} wiadomości dla {phoneNumber}");
            foreach (var msg in messages)
            {
                string kierunek = msg.IsOutgoing ? "Wychodząca (Ja)" : "Przychodząca (Ktoś)";
                Console.WriteLine($" -> [{kierunek}]: {msg.Text}");
            }

            return messages;
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"[DEBUG HISTORY ERROR] {ex.Message}");
            return new List<ChatMessageItem>(); 
        }
    }
}