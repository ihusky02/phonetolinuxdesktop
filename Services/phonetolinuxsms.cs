using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace phonetolinux.Services;

public class PhonetoLinuxSMS
{
    private readonly HttpClient _httpClient = new();

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            string url = $"{PhoneConfig.GetBaseUrl()}/sendsms";
            var payload = new { phoneNumber, message };
            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}