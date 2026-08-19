using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using phonetolinux.Models;
using PhoneToLinux.Core;
using PhoneToLinux.Plugins;

namespace phonetolinux.Services
{
    /// <summary>
    /// Wtyczka odpowiedzialna za pobieranie listy kontaktów z urządzenia mobilnego
    /// za pośrednictwem żądania HTTP do serwera i deserializację odpowiedzi JSON.
    /// </summary>
    public class ContactsPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient = new();

        /// <inheritdoc />
        public string Endpoint => "/contacts";

        /// <summary>
        /// Wykonuje operację wtyczki dla zadanego zapytania.
        /// </summary>
        /// <param name="queryParams">Parametry zapytania.</param>
        /// <returns>Odpowiedź w formacie JSON.</returns>
        public string Execute(string queryParams)
        {
            return "{\"status\":\"ContactsPlugin active\"}";
        }

        /// <summary>
        /// Asynchronicznie pobiera listę kontaktów z serwera telefonu.
        /// </summary>
        /// <returns>Lista obiektów reprezentujących kontakty.</returns>
        public async Task<List<ContactItem>> GetContactsAsync()
        {
            try
            {
                string url = $"{PhoneConfigPlugin.GetBaseUrl()}/contacts";
                string json = await _httpClient.GetStringAsync(url);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<ContactItem>>(json, options) ?? new List<ContactItem>();
            }
            catch { return new List<ContactItem>(); }
        }
    }
}