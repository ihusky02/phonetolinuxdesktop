using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneToLinux.Security;

namespace phonetolinux.ViewModels
{
    /// <summary>
    /// ViewModel responsible for managing initial device handshake and PIN generation logic.
    /// </summary>
    public partial class PairingViewModel : ObservableObject
    {
        private readonly DevicePairingService _pairingService;

        [ObservableProperty]
        private string _pairingPin = "000 000";

        [ObservableProperty]
        private string _statusMessage = "Enter this PIN in your mobile app";

        [ObservableProperty]
        private string _ipAddress = string.Empty;

        // Flag enabling the UI button to generate a new PIN code
        [ObservableProperty]
        private bool _canGeneratePin = true;

        public PairingViewModel()
        {
            _pairingService = new DevicePairingService();
            Dispatcher.UIThread.Post(() => GeneratePairingPinCode(), DispatcherPriority.Render);
        }

        /// <summary>
        /// Generates a fresh 6-digit PIN code and calculates the active network endpoint.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanGeneratePin))]
        public void GeneratePairingPinCode()
        {
            try
            {
                IpAddress = GetActiveLocalIpAddress();
                int defaultPort = 5000;

                // Generate secure 6-digit PIN and payload
                PairingPin = _pairingService.GeneratePairingPin();
                string payload = _pairingService.GeneratePairingPayload(IpAddress, defaultPort, PairingPin);

                StatusMessage = $"Waiting for connection at {IpAddress}:{defaultPort}";
                Console.WriteLine($"[PAIRING] Secure PIN generated: {PairingPin} for {IpAddress}:{defaultPort}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PAIRING ERROR] {ex.Message}");
                StatusMessage = $"Initialization error: {ex.Message}";
                PairingPin = "ERROR";
            }
        }

        /// <summary>
        /// Resolves the primary local IPv4 address of the host machine.
        /// </summary>
        private string GetActiveLocalIpAddress()
        {
            try
            {
                var activeInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(nic => 
                        nic.OperationalStatus == OperationalStatus.Up &&
                        nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

                if (activeInterface != null)
                {
                    var ipProperties = activeInterface.GetIPProperties();
                    var ipv4Address = ipProperties.UnicastAddresses
                        .FirstOrDefault(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork);

                    if (ipv4Address != null)
                    {
                        return ipv4Address.Address.ToString();
                    }
                }
            }
            catch { }

            return "127.0.0.1";
        }
    }
}