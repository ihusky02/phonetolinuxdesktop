using System;
using System.Net.Http;
using System.Threading.Tasks;
using PhoneToLinux.Core;
using PhoneToLinux.Plugins;

namespace phonetolinux.Services
{
    /// <summary>
    /// Plugin responsible for managing phone calls (initiating, answering, and ending calls)
    /// via HTTP POST requests to the mobile device server.
    /// </summary>
    public class PhoneCallPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneCallPlugin"/> class.
        /// </summary>
        /// <param name="httpClient">Optional custom HttpClient instance. Uses a default client if none provided.</param>
        public PhoneCallPlugin(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <inheritdoc />
        public string Endpoint => "/call";

        /// <summary>
        /// Executes the plugin action based on provided query parameters.
        /// </summary>
        /// <param name="queryParams">Query parameters (e.g., "action=start&amp;number=+48500100200").</param>
        /// <returns>Response in JSON format indicating result.</returns>
        public string Execute(string queryParams)
        {
            if (string.IsNullOrWhiteSpace(queryParams))
            {
                return "{\"status\":\"PhoneCallPlugin active\"}";
            }

            try
            {
                if (queryParams.Contains("action=answer", StringComparison.OrdinalIgnoreCase))
                {
                    bool result = Task.Run(AnswerCallAsync).GetAwaiter().GetResult();
                    return $"{{\"status\":\"{(result ? "success" : "error")}\", \"action\":\"answercall\"}}";
                }

                if (queryParams.Contains("action=end", StringComparison.OrdinalIgnoreCase))
                {
                    bool result = Task.Run(EndCallAsync).GetAwaiter().GetResult();
                    return $"{{\"status\":\"{(result ? "success" : "error")}\", \"action\":\"endcall\"}}";
                }

                return $"{{\"status\":\"PhoneCallPlugin active\", \"query\":\"{queryParams}\"}}";
            }
            catch (Exception ex)
            {
                return $"{{\"status\":\"error\", \"message\":\"{ex.Message}\"}}";
            }
        }

        /// <summary>
        /// Asynchronously initiates a new phone call to the specified destination number.
        /// </summary>
        /// <param name="phoneNumber">Destination phone number.</param>
        /// <returns>True if the request succeeded; otherwise false.</returns>
        public async Task<bool> StartCallAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrEmpty(PhoneConfig.PhoneIp)) 
                return false;

            try
            {
                string url = $"{PhoneConfig.GetBaseUrl()}/call?number={Uri.EscapeDataString(phoneNumber.Trim())}";
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                using var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PhoneCallPlugin Error] Failed to start call: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously ends or rejects an active phone call.
        /// </summary>
        /// <returns>True if the request succeeded; otherwise false.</returns>
        public async Task<bool> EndCallAsync()
        {
            if (string.IsNullOrEmpty(PhoneConfig.PhoneIp)) 
                return false;

            try
            {
                string url = $"{PhoneConfig.GetBaseUrl()}/call?action=end";
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                using var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PhoneCallPlugin Error] Failed to end call: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously answers an incoming phone call.
        /// </summary>
        /// <returns>True if the request succeeded; otherwise false.</returns>
        public async Task<bool> AnswerCallAsync()
        {
            if (string.IsNullOrEmpty(PhoneConfig.PhoneIp)) 
                return false;

            try
            {
                string url = $"{PhoneConfig.GetBaseUrl()}/call?action=answer";
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                using var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PhoneCallPlugin Error] Failed to answer call: {ex.Message}");
                return false;
            }
        }
    }
}