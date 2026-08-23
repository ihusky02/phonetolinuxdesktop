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
    /// Plugin responsible for fetching the list of contacts from the mobile device
    /// via an HTTP request to the server and deserializing the JSON response.
    /// </summary>
    public class ContactsPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient = new();

        /// <inheritdoc />
        public string Endpoint => "/contacts";

        /// <summary>
        /// Executes the plugin operation for a given query.
        /// </summary>
        /// <param name="queryParams">Query parameters.</param>
        /// <returns>Response in JSON format.</returns>
        public string Execute(string queryParams)
        {
            return "{\"status\":\"ContactsPlugin active\"}";
        }

        /// <summary>
        /// Asynchronously fetches the list of contacts from the phone server.
        /// </summary>
        /// <returns>A list of contact item objects.</returns>
        public async Task<List<ContactItem>> GetContactsAsync()
        {
            try
            {
                // Guard check to prevent invalid URI if phone IP is not yet set
                if (string.IsNullOrEmpty(PhoneConfig.PhoneIp))
                {
                    return new List<ContactItem>();
                }

                string url = $"{PhoneConfig.GetBaseUrl()}/contacts";
                string json = await _httpClient.GetStringAsync(url);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<ContactItem>>(json, options) ?? new List<ContactItem>();
            }
            catch { return new List<ContactItem>(); }
        }
    }
}