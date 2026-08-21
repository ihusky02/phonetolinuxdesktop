using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace PhoneToLinux.Security
{
    /// <summary>
    /// Service responsible for establishing a secure pairing mechanism between Desktop and Android.
    /// Generates QR code payloads and derives a unique 256-bit AES master key based on hardware MAC addresses.
    /// </summary>
    public class DevicePairingService
    {
        /// <summary>
        /// Retrieves the MAC address of the first operational network interface on the Linux desktop.
        /// </summary>
        public string GetDesktopMacAddress()
        {
            var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(nic => nic.OperationalStatus == OperationalStatus.Up && 
                                       nic.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            if (networkInterface == null)
            {
                throw new InvalidOperationException("No active network interface found to retrieve MAC address.");
            }

            var macBytes = networkInterface.GetPhysicalAddress().GetAddressBytes();
            return string.Join(":", macBytes.Select(b => b.ToString("X2")));
        }

        /// <summary>
        /// Generates the connection string payload to be encoded into a QR Code for the Android app.
        /// </summary>
        /// <param name="desktopIpAddress">The local IP address of the Linux desktop.</param>
        /// <param name="port">The port number the desktop app is listening on.</param>
        public string GenerateQrCodePayload(string desktopIpAddress, int port)
        {
            string desktopMac = GetDesktopMacAddress();
            
            // Format: phonetolinux://pair?ip=192.168.1.10&port=5000&mac=AA:BB:CC:DD:EE:FF
            return $"phonetolinux://pair?ip={desktopIpAddress}&port={port}&mac={desktopMac}";
        }

        /// <summary>
        /// Derives a 256-bit (32-byte) AES key by combining the desktop and Android MAC addresses.
        /// Uses SHA-256 to ensure the resulting key is exactly 256 bits long.
        /// </summary>
        /// <param name="androidMacAddress">The MAC address received from the Android device during the pairing handshake.</param>
        public byte[] DeriveAesKey(string androidMacAddress)
        {
            string desktopMac = GetDesktopMacAddress();
            
            // Normalize MACs to ensure consistent hashing regardless of formatting (e.g., lowercase vs uppercase)
            string normalizedDesktop = desktopMac.Replace(":", "").ToUpperInvariant();
            string normalizedAndroid = androidMacAddress.Replace(":", "").ToUpperInvariant();

            // Combine both MACs with an internal salt to prevent rainbow table attacks
            string combinedHardwareIdentifiers = $"{normalizedDesktop}_{normalizedAndroid}_PhoneToLinux_Salt2026";

            using (var sha256 = SHA256.Create())
            {
                // SHA-256 outputs exactly 256 bits (32 bytes), which is required for AES-256
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedHardwareIdentifiers));
            }
        }
    }
}