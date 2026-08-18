using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace phonetolinux.Services;

public class phonetolinuxconversations
{
    private readonly HttpClient _httpClient = new();

    public async Task<List<ConversationDto>> GetConversationsFromServerAsync()
    {
        try
        {
            string url = $"{PhoneConfig.GetBaseUrl()}/conversations";
            Console.WriteLine($"[DEBUG CONVERSATIONS] Pobieram z: {url}");
            
            string json = await _httpClient.GetStringAsync(url);
            Console.WriteLine($"[DEBUG CONVERSATIONS] Odpowiedź JSON: {json}");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<ConversationDto>>(json, options) ?? new List<ConversationDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CONVERSATIONS ERROR] Nie udało się pobrać konwersacji z serwera: {ex.Message}");
            return new List<ConversationDto>();
        }
    }
}

public class ConversationDto
{
    public string contactName { get; set; } = "";
    public string phoneNumber { get; set; } = "";
    public string lastMessage { get; set; } = "";
}