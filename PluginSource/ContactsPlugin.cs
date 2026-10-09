using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// via an HTTP request to the server, deserializing the JSON response, and aggregating multiple numbers per contact.
    /// </summary>
    public class ContactsPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactsPlugin"/> class.
        /// </summary>
        /// <param name="httpClient">Optional custom HttpClient instance. Uses a default client if none provided.</param>
        public ContactsPlugin(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

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
        /// Asynchronously fetches the list of contacts from the phone server and aggregates all phone numbers per unique contact name.
        /// </summary>
        /// <returns>A list of contact item objects with aggregated phone numbers.</returns>
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
                var rawContacts = JsonSerializer.Deserialize<List<ContactItem>>(json, options) 
                                ?? new List<ContactItem>();

                // Group by contact name and collect all unique phone numbers for each contact
                var groupedContacts = rawContacts
                    .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                    .GroupBy(c => c.Name.Trim().ToLowerInvariant());

                var result = new List<ContactItem>();
                foreach (var g in groupedContacts)
                {
                    var contactName = g.First().Name.Trim();
                    var numbers = g
                        .Select(c => c.PhoneNumber ?? "")
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Distinct()
                        .ToList();

                    if (numbers.Count == 0) continue;

                    var item = new ContactItem
                    {
                        Name = contactName,
                        PhoneNumber = numbers.First(),
                        PhoneNumbers = new ObservableCollection<string>(numbers),
                        SelectedPhoneNumber = numbers.First()
                    };
                    result.Add(item);
                }

                return result.OrderBy(c => c.Name).ToList();
            }
            catch
            {
                return new List<ContactItem>();
            }
        }

        /// <summary>
        /// Normalizes phone numbers to standard 9-digit format for comparison.
        /// </summary>
        private static string NormalizePhoneNumber(string number)
        {
            var digits = new string(number.Where(char.IsDigit).ToArray());
            return digits.Length > 9 ? digits.Substring(digits.Length - 9) : digits;
        }
    }
}
