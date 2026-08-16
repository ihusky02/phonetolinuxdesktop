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

                        // Odrzucamy nagłówki HTTP na początku odpowiedzi serwera (200 OK, Content-Type itp.)
                        string? responseLine;
                        while (!string.IsNullOrEmpty(responseLine = await reader.ReadLineAsync()))
                        {
                            Console.WriteLine($"[STREAM HEADER] {responseLine}");
                        }

                        Console.WriteLine("[STREAM] Nagłówki pominięte. Nasłuchuję zdarzeń SMS...");

                        while (!_cts.Token.IsCancellationRequested)
                        {
                            try
                            {
                                string? line = await reader.ReadLineAsync();
                                if (string.IsNullOrEmpty(line)) continue;

                                // Pomijamy piny serwerowe SSE
                                if (line.StartsWith(":")) continue;

                                // Jeśli linijka zaczyna się od "data: ", wycinamy ten prefiks, aby odsłonić czysty JSON
                                if (line.StartsWith("data: "))
                                {
                                    line = line.Substring(6);
                                }

                                // Diagnostyka: logujemy każdą surową linię odebraną ze strumienia
                                Console.WriteLine($"[DEBUG STREAM] Odebrano zdarzenie: {line}");

                                // Proste parsowanie przychodzącego JSON-a: {"event":"incoming_sms","sender":"...","message":"..."}
                                if (line.Contains("incoming_sms"))
                                {
                                    string sender = ExtractJsonField(line, "sender");
                                    string message = ExtractJsonField(line, "message");

                                    Console.WriteLine($"[PARSER DEBUG] Nadawca: '{sender}' | Wiadomość: '{message}'");

                                    if (!string.IsNullOrEmpty(message))
                                    {
                                        Console.WriteLine($"[STREAM SMS] Od: {sender} | Treść: {message}");
                                        onSmsReceived(sender, message);
                                    }
                                }
                            }
                            catch (Exception readEx)
                            {
                                // Zabezpieczenie pojedynczej iteracji odczytu
                                Console.WriteLine($"[STREAM READ BŁĄD]: {readEx.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[STREAM BŁĄD] Utracono połączenie lub błąd: {ex.Message}. Natychmiastowa ponowna próba...");
                        // Natychmiastowe ponowienie połączenia po 0.5 sekundy
                        await Task.Delay(500, _cts.Token);
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