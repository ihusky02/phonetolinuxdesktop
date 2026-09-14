using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace phonetolinux.Security;

/// <summary>
/// Authenticated IP discovery service protecting against IP injection/spoofing.
/// </summary>
public class AutochangeIP
{
    private const int DiscoveryPort = 8889;

    /// <summary>
    /// Sends a signed UDP broadcast request to locate the Android device safely.
    /// </summary>
    public async Task<string?> DiscoverPhoneIpAsync(string pairingSecret, int timeoutMs = 2000, CancellationToken cancellationToken = default)
    {
        using var udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;

        // Generate a random nonce to prevent replay attacks
        string nonce = Guid.NewGuid().ToString("N");
        string requestMessage = $"DISCOVER_PHONETOLINUX_REQUEST:{nonce}";
        
        var requestData = Encoding.UTF8.GetBytes(requestMessage);
        var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);

        try
        {
            await udpClient.SendAsync(requestData, requestData.Length, broadcastEndpoint);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeoutMs);

            var receiveTask = udpClient.ReceiveAsync();
            var completedTask = await Task.WhenAny(receiveTask, Task.Delay(timeoutMs, cts.Token));

            if (completedTask == receiveTask)
            {
                var result = await receiveTask;
                var responseText = Encoding.UTF8.GetString(result.Buffer).Trim();

                // Expected response format: PHONETOLINUX_RESPONSE:<SIGNATURE>
                var parts = responseText.Split(':');
                if (parts.Length == 2 && parts[0] == "PHONETOLINUX_RESPONSE")
                {
                    string receivedSignature = parts[1];
                    string expectedSignature = ComputeHmacSha256(nonce, pairingSecret);

                    // Validate HMAC signature to ensure authenticity
                    if (CryptographicOperations.FixedTimeEquals(
                        Encoding.UTF8.GetBytes(receivedSignature), 
                        Encoding.UTF8.GetBytes(expectedSignature)))
                    {
                        string discoveredIp = result.RemoteEndPoint.Address.ToString();
                        Console.WriteLine($"[AUTO-CHANGE IP] Authenticated device IP verified: {discoveredIp}");
                        return discoveredIp;
                    }
                    else
                    {
                        Console.WriteLine("[SECURITY WARNING] Invalid signature received! Potential IP injection attempt.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTO-CHANGE IP ERROR] Discovery failed: {ex.Message}");
        }

        return null;
    }

    private string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}