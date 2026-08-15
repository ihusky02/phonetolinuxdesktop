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
            return JsonSerializer.Deserialize<List<ChatMessageItem>>(json, options) ?? new List<ChatMessageItem>();
        }
        catch { return new List<ChatMessageItem>(); }
    }
}