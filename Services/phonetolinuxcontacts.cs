using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using phonetolinux.Models;

namespace phonetolinux.Services;

public class PhonetoLinuxContacts
{
    private readonly HttpClient _httpClient = new();

    public async Task<List<ContactItem>> GetContactsAsync()
    {
        try
        {
            string url = $"{PhoneConfig.GetBaseUrl()}/contacts";
            string json = await _httpClient.GetStringAsync(url);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<ContactItem>>(json, options) ?? new List<ContactItem>();
        }
        catch { return new List<ContactItem>(); }
    }
}