using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace phonetolinux.Services;

public class PhonetoLinuxCall
{
    private readonly HttpClient _httpClient = new();

    public async Task<bool> StartCallAsync(string phoneNumber)
    {
        try
        {
            string url = $"{PhoneConfig.GetBaseUrl()}/call?number={Uri.EscapeDataString(phoneNumber)}";
            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> EndCallAsync()
    {
        try
        {
            string url = $"{PhoneConfig.GetBaseUrl()}/endcall";
            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> AnswerCallAsync()
    {
        try
        {
            string url = $"{PhoneConfig.GetBaseUrl()}/answercall";
            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}