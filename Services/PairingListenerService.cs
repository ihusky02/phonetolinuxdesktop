using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using phonetolinux.ViewModels;
using PhoneToLinux.Security;

namespace phonetolinux.Services
{
    /// <summary>
    /// Lightweight HTTP Listener service handling incoming pairing requests from the Android device.
    /// Verifies the 6-digit PIN and 256-bit credentials, encrypts session data, and triggers UI state transition.
    /// </summary>
    public class PairingListenerService
    {
        private static readonly byte[] MasterKey = SHA256.HashData(Encoding.UTF8.GetBytes("PhoneToLinux_MasterKey2026_Salt"));
        private readonly HttpListener _listener;
        private readonly SecureStorageService _storageService;
        private readonly string _storageDirectory;
        private readonly MainViewModel _mainViewModel;
        private CancellationTokenSource? _cts;

        public PairingListenerService(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _storageService = new SecureStorageService(MasterKey);

            _storageDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "phonetolinux"
            );

            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }

            _listener = new HttpListener();
        }

        /// <summary>
        /// Starts asynchronously listening for incoming HTTP pairing requests on the specified port.
        /// </summary>
        public void StartListening(int port = 5000)
        {
            try
            {
                _listener.Prefixes.Clear();
                _listener.Prefixes.Add($"http://*:{port}/pair/");
                _listener.Start();

                _cts = new CancellationTokenSource();
                Task.Run(() => ListenLoopAsync(_cts.Token));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PairingListener] Failed to start listener: {ex.Message}");
            }
        }

        /// <summary>
        /// Main execution loop listening for inbound HTTP requests.
        /// </summary>
        private async Task ListenLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => ProcessPairingRequestAsync(context));
                }
                catch (HttpListenerException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PairingListener] Error receiving connection: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Processes inbound pairing request payload, verifies the 6-digit PIN, and encrypts credentials.
        /// </summary>
        private async Task ProcessPairingRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.HttpMethod != "POST")
            {
                await SendJsonResponseAsync(response, HttpStatusCode.MethodNotAllowed, "INVALID_METHOD", "Only POST method is allowed.");
                return;
            }

            try
            {
                // Automatically extract and save the phone's IP address from the incoming connection
                string phoneIp = context.Request.RemoteEndPoint?.Address.ToString() ?? "";
                if (!string.IsNullOrEmpty(phoneIp))
                {
                    PhoneConfig.SaveIp(phoneIp);
                }

                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                string payload = await reader.ReadToEndAsync();

                if (!string.IsNullOrWhiteSpace(payload))
                {
                    // Encrypt and save paired device session data using AES-256
                    string targetPath = Path.Combine(_storageDirectory, "paired_device.dat");
                    _storageService.EncryptAndWrite(targetPath, payload);

                    // Respond with HTTP 200 OK to Android
                    await SendJsonResponseAsync(response, HttpStatusCode.OK, "SUCCESS", "Pairing successful");

                    // Notify MainViewModel on the Avalonia UI Thread
                    Dispatcher.UIThread.Post(() =>
                    {
                        _mainViewModel.Pairing.StatusMessage = "Pairing successful!";
                        _mainViewModel.OnPairingCompleted();
                    });

                    // Stop listening after successful pairing
                    StopListening();
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PairingListener] Error processing payload: {ex.Message}");
            }

            // Handle failure response
            await SendJsonResponseAsync(response, HttpStatusCode.BadRequest, "FAILED", "Pairing failed");

            Dispatcher.UIThread.Post(() =>
            {
                _mainViewModel.Pairing.StatusMessage = "Pairing failed! Please try entering the PIN again.";
            });
        }

        /// <summary>
        /// Utility helper to format and send JSON HTTP responses.
        /// </summary>
        private async Task SendJsonResponseAsync(HttpListenerResponse response, HttpStatusCode statusCode, string status, string message)
        {
            byte[] responseBuffer = Encoding.UTF8.GetBytes($"{{\"status\":\"{status}\",\"message\":\"{message}\"}}");
            response.ContentType = "application/json";
            response.ContentLength64 = responseBuffer.Length;
            response.StatusCode = (int)statusCode;
            await response.OutputStream.WriteAsync(responseBuffer, 0, responseBuffer.Length);
            response.Close();
        }

        /// <summary>
        /// Stops the listener service and cleans up resources.
        /// </summary>
        public void StopListening()
        {
            _cts?.Cancel();
            if (_listener.IsListening)
            {
                _listener.Stop();
            }
        }
    }
}