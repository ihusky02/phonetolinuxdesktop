using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PhoneToLinux.Security;

namespace phonetolinux.Services;

/// <summary>
/// Service providing dynamic auto-detection and resolution of the phone's IP address on the local network.
/// </summary>
public static class DeviceIpResolver
{
    private static readonly byte[] MasterKey = SHA256.HashData(Encoding.UTF8.GetBytes("PhoneToLinux_MasterKey2026_Salt"));

    /// <summary>
    /// Attempts to automatically resolve the phone's IP address.
    /// Checks cached config, paired device credentials, and scans local subnets for ports 5001 (WebDAV) or 5000 (API).
    /// </summary>
    public static async Task<string> ResolvePhoneIpAsync()
    {
        // 1. Check cached IP in PhoneConfig
        string cachedIp = PhoneConfig.PhoneIp;
        if (!string.IsNullOrWhiteSpace(cachedIp) && (await TestPortAsync(cachedIp, 5001, 300) || await TestPortAsync(cachedIp, 5000, 300)))
        {
            return cachedIp;
        }

        // 2. Check IP from encrypted paired_device.dat file
        string pairedIp = LoadPairedDeviceIp();
        if (!string.IsNullOrWhiteSpace(pairedIp) && pairedIp != cachedIp)
        {
            if (await TestPortAsync(pairedIp, 5001, 400) || await TestPortAsync(pairedIp, 5000, 400))
            {
                PhoneConfig.SaveIp(pairedIp);
                return pairedIp;
            }
        }

        // 3. Perform fast subnet auto-scan on port 5001 (WebDAV) and port 5000 (API)
        string scannedIp = await ScanSubnetForPhoneAsync();
        if (!string.IsNullOrWhiteSpace(scannedIp))
        {
            PhoneConfig.SaveIp(scannedIp);
            return scannedIp;
        }

        // Fallback to cached/paired IP if scan could not verify a live socket
        if (!string.IsNullOrWhiteSpace(cachedIp)) return cachedIp;
        if (!string.IsNullOrWhiteSpace(pairedIp)) return pairedIp;

        return string.Empty;
    }

    /// <summary>
    /// Reads and decrypts paired_device.dat to extract phoneIp if available.
    /// </summary>
    private static string LoadPairedDeviceIp()
    {
        try
        {
            string storageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "phonetolinux");
            string targetPath = Path.Combine(storageDir, "paired_device.dat");

            if (File.Exists(targetPath))
            {
                var storageService = new SecureStorageService(MasterKey);
                string decryptedPayload = storageService.ReadAndDecrypt(targetPath);
                using var jsonDoc = JsonDocument.Parse(decryptedPayload);
                if (jsonDoc.RootElement.TryGetProperty("phoneIp", out var ipProp))
                {
                    return ipProp.GetString() ?? "";
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IpResolver] Could not read paired device IP: {ex.Message}");
        }
        return string.Empty;
    }

    /// <summary>
    /// Tests TCP connection to host on specified port with a timeout.
    /// </summary>
    public static async Task<bool> TestPortAsync(string ip, int port, int timeoutMs = 300)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(ip, port);
            var delayTask = Task.Delay(timeoutMs);

            var completedTask = await Task.WhenAny(connectTask, delayTask);
            if (completedTask == connectTask && client.Connected)
            {
                return true;
            }
        }
        catch { }
        return false;
    }

    /// <summary>
    /// Scans local IPv4 network subnets (/24) concurrently for port 5001 (WebDAV) or 5000 (API).
    /// </summary>
    private static async Task<string> ScanSubnetForPhoneAsync()
    {
        try
        {
            var localIps = GetLocalIpv4Addresses();
            if (localIps.Count == 0) return string.Empty;

            var tasks = new List<Task<string>>();

            foreach (var localIp in localIps)
            {
                string[] parts = localIp.Split('.');
                if (parts.Length != 4) continue;
                string prefix = $"{parts[0]}.{parts[1]}.{parts[2]}";

                // Concurrently probe all IPs 1..254 in the local subnet
                for (int i = 1; i <= 254; i++)
                {
                    string targetIp = $"{prefix}.{i}";
                    if (targetIp == localIp) continue; // skip localhost address

                    tasks.Add(Task.Run(async () =>
                    {
                        if (await TestPortAsync(targetIp, 5001, 300) || await TestPortAsync(targetIp, 5000, 300))
                        {
                            return targetIp;
                        }
                        return string.Empty;
                    }));
                }
            }

            while (tasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(tasks);
                tasks.Remove(finishedTask);
                string resultIp = await finishedTask;
                if (!string.IsNullOrEmpty(resultIp))
                {
                    Console.WriteLine($"[IpResolver] Discovered phone via subnet auto-scan: {resultIp}");
                    return resultIp;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IpResolver] Subnet scan error: {ex.Message}");
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets active local IPv4 addresses of non-loopback network interfaces.
    /// </summary>
    private static List<string> GetLocalIpv4Addresses()
    {
        var list = new List<string>();
        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                var ipProps = ni.GetIPProperties();
                foreach (var addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        string ipStr = addr.Address.ToString();
                        if (!ipStr.StartsWith("127."))
                        {
                            list.Add(ipStr);
                        }
                    }
                }
            }
        }
        catch { }
        return list;
    }
}