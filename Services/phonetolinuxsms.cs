using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace phonetolinux.Services;

public class PhonetoLinuxSMS
{
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            // Poprawiono endpoint na /send_sms oraz przekazywanie danych w QueryString (zgodnie z Android Server)
            string encodedNumber = Uri.EscapeDataString(phoneNumber);
            string encodedMessage = Uri.EscapeDataString(message);
            string url = $"{PhoneConfig.GetBaseUrl()}/send_sms?number={encodedNumber}&message={encodedMessage}";

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Błąd serwera SMS: {(int)response.StatusCode}, Odpowiedź: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) 
        { 
            Console.WriteLine($"Błąd sieciowy podczas wysyłania SMS: {ex.Message}");
            return false; 
        }
    }
}