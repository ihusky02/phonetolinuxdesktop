using System;
using System.Net.Http;
using System.Threading.Tasks;
using PhoneToLinux.Core;
using PhoneToLinux.Plugins;

namespace phonetolinux.Services
{
    /// <summary>
    /// Wtyczka odpowiedzialna za zarządzanie połączeniami telefonicznymi (inicjowanie, odebranie oraz kończenie rozmowy)
    /// za pośrednictwem żądań HTTP do serwera działającego na urządzeniu mobilnym.
    /// </summary>
    public class PhoneCallPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient = new();

        /// <inheritdoc />
        public string Endpoint => "/call";

        /// <summary>
        /// Wykonuje akcję wtyczki na podstawie przekazanych parametrów lub obsługuje domyślne żądanie połączenia.
        /// </summary>
        /// <param name="queryParams">Numer telefonu lub parametry żądania.</param>
        /// <returns>Odpowiedź w formacie JSON lub wynik operacji.</returns>
        public string Execute(string queryParams)
        {
            // Możemy obsłużyć główny endpoint wtyczki
            return "{\"status\":\"PhoneCallPlugin active\"}";
        }

        /// <summary>
        /// Asynchronicznie inicjuje nowe połączenie telefoniczne na podany numer.
        /// </summary>
        /// <param name="phoneNumber">Numer telefonu docelowego.</param>
        /// <returns>True, jeśli żądanie powiodło się; w przeciwnym razie false.</returns>
        public async Task<bool> StartCallAsync(string phoneNumber)
        {
            try
            {
                string url = $"{PhoneConfigPlugin.GetBaseUrl()}/call?number={Uri.EscapeDataString(phoneNumber)}";
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        /// <summary>
        /// Asynchronicznie kończy aktywne połączenie telefoniczne.
        /// </summary>
        /// <returns>True, jeśli żądanie powiodło się; w przeciwnym razie false.</returns>
        public async Task<bool> EndCallAsync()
        {
            try
            {
                string url = $"{PhoneConfigPlugin.GetBaseUrl()}/endcall";
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        /// <summary>
        /// Asynchronicznie odbiera przychodzące połączenie telefoniczne.
        /// </summary>
        /// <returns>True, jeśli żądanie powiodło się; w przeciwnym razie false.</returns>
        public async Task<bool> AnswerCallAsync()
        {
            try
            {
                string url = $"{PhoneConfigPlugin.GetBaseUrl()}/answercall";
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}