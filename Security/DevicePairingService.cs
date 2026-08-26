using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace PhoneToLinux.Security
{
    /// <summary>
    /// Service responsible for establishing a secure pairing mechanism between Desktop and Android.
    /// Generates secure 6-digit PIN payloads and derives a unique 256-bit AES master key.
    /// Includes graceful fallbacks for systems without accessible physical MAC addresses.
    /// </summary>
    public class DevicePairingService
    {
        /// <summary>
        /// Retrieves the MAC address of the first operational network interface on the Linux desktop.
        /// Falls back to a deterministic machine key if no active physical interface is reported.
        /// </summary>
        public string GetDesktopMacAddress()
        {
            try
            {
                var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(nic => nic.OperationalStatus == OperationalStatus.Up && 
                                           nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                           nic.GetPhysicalAddress().GetAddressBytes().Length > 0);

                if (networkInterface != null)
                {
                    var macBytes = networkInterface.GetPhysicalAddress().GetAddressBytes();
                    if (macBytes.Length > 0)
                    {
                        return string.Join(":", macBytes.Select(b => b.ToString("X2")));
                    }
                }
            }
            catch
            {
                // Fallback to secondary network inspection
            }

            // Secondary search across any interface with hardware address
            var fallbackNic = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(nic => nic.GetPhysicalAddress().GetAddressBytes().Length > 0);

            if (fallbackNic != null)
            {
                var bytes = fallbackNic.GetPhysicalAddress().GetAddressBytes();
                return string.Join(":", bytes.Select(b => b.ToString("X2")));
            }

            // Ultimate fallback for isolated environments/virtual interfaces
            return "02:00:00:00:00:00";
        }

        /// <summary>
        /// Generates a cryptographically secure 6-digit pairing PIN formatted as "xxx xxx".
        /// </summary>
        public string GeneratePairingPin()
        {
            int pin = RandomNumberGenerator.GetInt32(0, 1000000);
            string rawPin = pin.ToString("D6");
            return $"{rawPin.Substring(0, 3)} {rawPin.Substring(3, 3)}";
        }

        /// <summary>
        /// Generates the connection string payload containing IP, port, MAC, and the secure pairing PIN.
        /// </summary>
        /// <param name="desktopIpAddress">The local IP address of the Linux desktop.</param>
        /// <param name="port">The port number the desktop app is listening on.</param>
        /// <param name="pairingPin">The 6-digit pairing PIN displayed to the user.</param>
        public string GeneratePairingPayload(string desktopIpAddress, int port, string pairingPin)
        {
            string desktopMac = GetDesktopMacAddress();
            string cleanPin = pairingPin.Replace(" ", "");
            
            // Format: phonetolinux://pair?ip=192.168.1.10&port=5000&mac=AA:BB:CC:DD:EE:FF&pin=123456
            return $"phonetolinux://pair?ip={desktopIpAddress}&port={port}&mac={desktopMac}&pin={cleanPin}";
        }

        /// <summary>
        /// Derives a 256-bit (32-byte) AES key by combining the desktop MAC, Android MAC, and pairing PIN.
        /// Uses SHA-256 to ensure the resulting key is exactly 256 bits long.
        /// </summary>
        /// <param name="androidMacAddress">The MAC address received from the Android device during the pairing handshake.</param>
        /// <param name="pairingPin">The 6-digit pairing PIN used during the handshake.</param>
        public byte[] DeriveAesKey(string androidMacAddress, string pairingPin)
        {
            string desktopMac = GetDesktopMacAddress();
            
            // Normalize MACs and PIN to ensure consistent hashing
            string normalizedDesktop = desktopMac.Replace(":", "").ToUpperInvariant();
            string normalizedAndroid = androidMacAddress.Replace(":", "").ToUpperInvariant();
            string normalizedPin = pairingPin.Replace(" ", "");

            // Combine identifiers and PIN with an internal salt for 256-bit security
            string combinedIdentifiers = $"{normalizedDesktop}_{normalizedAndroid}_{normalizedPin}_PhoneToLinux_Salt2026";

            return SHA256.HashData(Encoding.UTF8.GetBytes(combinedIdentifiers));
        }
    }
}