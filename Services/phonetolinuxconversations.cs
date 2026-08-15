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
            string json = await _httpClient.GetStringAsync(url);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<ConversationDto>>(json, options) ?? new List<ConversationDto>();
        }
        catch { return new List<ConversationDto>(); }
    }
}

public class ConversationDto
{
    public string contactName { get; set; } = "";
    public string phoneNumber { get; set; } = "";
    public string lastMessage { get; set; } = "";
}