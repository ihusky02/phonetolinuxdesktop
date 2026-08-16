using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace phonetolinux.Services
{
    public class PhonetoLinuxStream
    {
        private CancellationTokenSource? _cts;

        public void StartListening(string phoneIp, int port, Action<string, string> onSmsReceived)
        {
            _cts = new CancellationTokenSource();
            
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        Console.WriteLine($"[STREAM] Próbuję połączyć się z telefonem {phoneIp}:{port}...");
                        using var client = new TcpClient();
                        await client.ConnectAsync(phoneIp, port, _cts.Token);
                        Console.WriteLine("[STREAM] Połączono pomyślnie z telefonem! Nasłuchuję powiadomień...");
                        
                        using var stream = client.GetStream();
                        using var reader = new StreamReader(stream);
                        using var writer = new StreamWriter(stream) { AutoFlush = true };

                        // Wysyłamy zapytanie o strumień powiadomień
                        await writer.WriteLineAsync("GET /sms_stream HTTP/1.1");
                        await writer.WriteLineAsync($"Host: {phoneIp}:{port}");
                        await writer.WriteLineAsync("Connection: keep-alive");
                        await writer.WriteLineAsync();

                        while (!_cts.Token.IsCancellationRequested)
                        {
                            string? line = await reader.ReadLineAsync();
                            if (string.IsNullOrEmpty(line)) continue;

                            // Pomijamy piny serwerowe SSE, żeby nie śmiecić logów
                            if (line.StartsWith(":")) continue;

                            // Diagnostyka: logujemy każdą surową linię odebraną ze strumienia
                            Console.WriteLine($"[DEBUG STREAM] Odebrano surową linię: {line}");

                            // Proste parsowanie przychodzącego JSON-a: {"event":"incoming_sms","sender":"...","message":"..."}
                            if (line.Contains("incoming_sms"))
                            {
                                string sender = ExtractJsonField(line, "sender");
                                string message = ExtractJsonField(line, "message");

                                Console.WriteLine($"[PARSER DEBUG] Wyciągnięto -> Nadawca: '{sender}' | Wiadomość: '{message}'");

                                if (!string.IsNullOrEmpty(message))
                                {
                                    Console.WriteLine($"[STREAM SMS] Od: {sender} | Treść: {message}");
                                    onSmsReceived(sender, message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[STREAM BŁĄD] Utracono połączenie lub błąd: {ex.Message}. Ponowna próba za 3 sekundy...");
                        // Jeśli połączenie zerwie się, próbuje ponowić po 3 sekundach
                        await Task.Delay(3000, _cts.Token);
                    }
                }
            }, _cts.Token);
        }

        public void StopListening()
        {
            _cts?.Cancel();
            Console.WriteLine("[STREAM] Zatrzymano nasłuch.");
        }

        private string ExtractJsonField(string json, string fieldName)
        {
            try
            {
                string pattern = $"\"{fieldName}\":";
                int keyIndex = json.IndexOf(pattern);
                if (keyIndex == -1) return "";

                int startIndex = json.IndexOf('"', keyIndex + pattern.Length);
                if (startIndex == -1) return "";

                int endIndex = json.IndexOf('"', startIndex + 1);
                if (endIndex == -1) return "";

                return json.Substring(startIndex + 1, endIndex - startIndex - 1);
            }
            catch 
            { 
                return ""; 
            }
        }
    }
}