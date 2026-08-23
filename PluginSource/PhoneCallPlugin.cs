using System;
using System.Net.Http;
using System.Threading.Tasks;
using PhoneToLinux.Core;
using PhoneToLinux.Plugins;

namespace phonetolinux.Services
{
    /// <summary>
    /// Plugin responsible for managing phone calls (initiating, answering, and ending calls)
    /// via HTTP requests to the server running on the mobile device.
    /// </summary>
    public class PhoneCallPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient = new();

        /// <inheritdoc />
        public string Endpoint => "/call";

        /// <summary>
        /// Executes the plugin action based on provided parameters or handles the default call request.
        /// </summary>
        /// <param name="queryParams">Phone number or request parameters.</param>
        /// <returns>Response in JSON format or operation result.</returns>
        public string Execute(string queryParams)
        {
            // Placeholder for handling the main plugin endpoint
            return "{\"status\":\"PhoneCallPlugin active\"}";
        }

        /// <summary>
        /// Asynchronously initiates a new phone call to the specified number.
        /// </summary>
        /// <param name="phoneNumber">Destination phone number.</param>
        /// <returns>True if the request succeeded; otherwise false.</returns>
        public async Task<bool> StartCallAsync(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(PhoneConfig.PhoneIp)) return false;

                string url = $"{PhoneConfig.GetBaseUrl()}/call?number={Uri.EscapeDataString(phoneNumber)}";
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        /// <summary>
        /// Asynchronously ends the active phone call.
        /// </summary>
        /// <returns>True if the request succeeded; otherwise false.</returns>
        public async Task<bool> EndCallAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(PhoneConfig.PhoneIp)) return false;

                string url = $"{PhoneConfig.GetBaseUrl()}/endcall";
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        /// <summary>
        /// Asynchronously answers an incoming phone call.
        /// </summary>
        /// <returns>True if the request succeeded; otherwise false.</returns>
        public async Task<bool> AnswerCallAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(PhoneConfig.PhoneIp)) return false;

                string url = $"{PhoneConfig.GetBaseUrl()}/answercall";
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}