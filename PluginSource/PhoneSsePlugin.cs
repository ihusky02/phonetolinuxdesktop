using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace phonetolinux.Plugins;

/// <summary>
/// Data Transfer Object representing incoming events from the Android SSE stream.
/// </summary>
public class PhoneEventDto
{
    [JsonPropertyName("event")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("sender")]
    public string Sender { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Plugin implementation responsible for maintaining the Server-Sent Events (SSE) stream 
/// connection with the Android device and bridging calls and messages.
/// </summary>
public class PhoneSsePlugin : IPhonetolinuxPlugin
{
    public string Name => "PhoneBridge SSE Plugin";
    public string Version => "1.0.0";

    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromMilliseconds(-1) };
    private readonly HttpClient _commandClient = new();
    private CancellationTokenSource? _cts;

    private string _targetIp = "127.0.0.1";
    private int _targetPort = 5000;

    public event Action<string, string>? OnCallReceived;
    public event Action? OnCallEnded;
    public event Action<string, string>? OnSmsReceived;

    /// <summary>
    /// Initializes the SSE background listener loop connecting to the phone.
    /// </summary>
    public void Initialize(string phoneIp, int port = 5000)
    {
        _targetIp = phoneIp;
        _targetPort = port;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        string sseUrl = $"http://{_targetIp}:{_targetPort}/sms_stream";
        Console.WriteLine($"[SSE PLUGIN] Connecting to SSE stream at: {sseUrl}");
        Task.Run(() => ReadSseStreamAsync(sseUrl, _cts.Token));
    }

    /// <summary>
    /// Continuously reads the SSE stream from the Android server with automatic reconnection handling.
    /// </summary>
    private async Task ReadSseStreamAsync(string url, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

                response.EnsureSuccessStatusCode();
                Console.WriteLine("[SSE PLUGIN] Successfully connected to Android SSE stream.");

                using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var reader = new StreamReader(stream);

                while (!reader.EndOfStream && !ct.IsCancellationRequested)
                {
                    string? line = await reader.ReadLineAsync(ct);
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Log raw lines for debugging incoming events
                    Console.WriteLine($"[SSE RAW] {line}");

                    if (line.StartsWith("data:"))
                    {
                        string jsonPayload = line[5..].Trim();
                        try
                        {
                            var phoneEvent = JsonSerializer.Deserialize<PhoneEventDto>(jsonPayload);
                            if (phoneEvent != null)
                            {
                                Console.WriteLine($"[SSE PARSED] Event: '{phoneEvent.EventType}', Number: '{phoneEvent.Number}', Sender: '{phoneEvent.Sender}'");
                                DispatchEvent(phoneEvent);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[SSE PARSE ERROR] Failed to deserialize JSON: '{jsonPayload}' -> {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                Console.WriteLine($"[SSE CONNECTION ERROR] {ex.Message}. Reconnecting in 3 seconds...");
                // Wait before attempting to reconnect after a network interruption
                await Task.Delay(3000, ct);
            }
        }
    }

    /// <summary>
    /// Dispatches incoming parsed events to their respective registered event handlers.
    /// </summary>
    private void DispatchEvent(PhoneEventDto ev)
    {
        switch (ev.EventType)
        {
            case "incoming_call":
                OnCallReceived?.Invoke(ev.Number, ev.Sender);
                break;

            case "call_ended":
                OnCallEnded?.Invoke();
                break;

            case "incoming_sms":
                OnSmsReceived?.Invoke(ev.Sender, ev.Message);
                break;
        }
    }

    /// <summary>
    /// Sends an HTTP POST command to the phone to answer the active incoming call.
    /// </summary>
    public async Task<bool> AnswerCallAsync()
    {
        try
        {
            var res = await _commandClient.PostAsync($"http://{_targetIp}:{_targetPort}/call/answer", null);
            return res.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    /// <summary>
    /// Sends an HTTP POST command to the phone to reject the active incoming call.
    /// </summary>
    public async Task<bool> RejectCallAsync()
    {
        try
        {
            var res = await _commandClient.PostAsync($"http://{_targetIp}:{_targetPort}/call/reject", null);
            return res.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    /// <summary>
    /// Shuts down the background listener and cancels active network tasks.
    /// </summary>
    public void Shutdown()
    {
        _cts?.Cancel();
    }
}