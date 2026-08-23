using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using phonetolinux.Services;
using PhoneToLinux.Security;

namespace phonetolinux.ViewModels
{
    /// <summary>
    /// Main application ViewModel responsible for coordinating top-level UI states,
    /// active views (dialer, chat, pairing), navigation tabs, call overlays, and setup lifecycle.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private static readonly byte[] MasterKey = SHA256.HashData(Encoding.UTF8.GetBytes("PhoneToLinux_MasterKey2026_Salt"));
        private readonly SecureStorageService _storageService;
        private readonly string _storageDirectory;
        private PairingListenerService? _pairingListener;

        [ObservableProperty]
        private bool _isPaired;

        [ObservableProperty]
        private int _selectedTabIndex;

        [ObservableProperty]
        private PairingViewModel _pairing;

        [ObservableProperty]
        private DialerViewModel _dialer = new();

        [ObservableProperty]
        private ChatViewModel _activeChat = new();

        [ObservableProperty]
        private string _currentTheme = "Dark";

        // Call overlay and state management properties (defaulted to false/empty to prevent unwanted overlays)
        [ObservableProperty]
        private bool _isInCall = false;

        [ObservableProperty]
        private bool _isIncomingCall = false;

        [ObservableProperty]
        private string _contactName = "Unknown";

        [ObservableProperty]
        private string _phoneNumber = "";

        public MainViewModel()
        {
            _storageService = new SecureStorageService(MasterKey);
            
            // Resolve application storage directory in user profile (.local/share/phonetolinux)
            _storageDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "phonetolinux"
            );

            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }

            _pairing = new PairingViewModel();

            // Evaluate stored credentials on application launch
            CheckPairingStatus();
        }

        /// <summary>
        /// Inspects the secure storage directory to determine if the device has already been paired
        /// and securely restores the phone IP configuration using AES-256 decryption.
        /// </summary>
        public void CheckPairingStatus()
        {
            string pairedFilePath = Path.Combine(_storageDirectory, "paired_device.dat");
            IsPaired = File.Exists(pairedFilePath);

            if (IsPaired)
            {
                SelectedTabIndex = 0; // Default to Dialer tab

                try
                {
                    // Securely read and decrypt the paired device data using the master key
                    string decryptedPayload = _storageService.ReadAndDecrypt(pairedFilePath);
                    
                    var jsonDoc = System.Text.Json.JsonDocument.Parse(decryptedPayload);
                    if (jsonDoc.RootElement.TryGetProperty("phoneIp", out var ipProp))
                    {
                        string savedIp = ipProp.GetString();
                        if (!string.IsNullOrEmpty(savedIp))
                        {
                            PhoneConfig.SaveIp(savedIp);
                        }
                    }
                }
                catch
                {
                    // Fallback silently if decryption fails or file integrity is compromised
                }
            }
            else
            {
                SelectedTabIndex = 3; // Fallback index for initial setup

                // Start HTTP listener service for initial pairing setup
                _pairingListener = new PairingListenerService(this);
                _pairingListener.StartListening(5000);
            }
        }

        /// <summary>
        /// Invoked when the PIN handshake completes successfully from the mobile app.
        /// </summary>
        [RelayCommand]
        public void OnPairingCompleted()
        {
            IsPaired = true;
            SelectedTabIndex = 0; // Automatically switch to the Dialer workspace
        }

        /// <summary>
        /// Clears all stored encrypted credentials and reverts the UI to the initial PIN pairing screen.
        /// Useful when reinstalling the Android or Linux application.
        /// </summary>
        [RelayCommand]
        public void UnpairDevice()
        {
            string pairedFilePath = Path.Combine(_storageDirectory, "paired_device.dat");
            string sessionFilePath = Path.Combine(_storageDirectory, "session_keys.dat");

            if (File.Exists(pairedFilePath)) File.Delete(pairedFilePath);
            if (File.Exists(sessionFilePath)) File.Delete(sessionFilePath);

            IsPaired = false;
            SelectedTabIndex = 3;
            Pairing.GeneratePairingPinCode();

            _pairingListener?.StopListening();
            _pairingListener = new PairingListenerService(this);
            _pairingListener.StartListening(5000);
        }

        /// <summary>
        /// Command triggered to append a digit or symbol to the current phone number string.
        /// </summary>
        [RelayCommand]
        public void AppendNumber(string number)
        {
            PhoneNumber += number;
        }

        /// <summary>
        /// Command triggered to remove the last character from the current phone number string.
        /// </summary>
        [RelayCommand]
        public void Backspace()
        {
            if (!string.IsNullOrEmpty(PhoneNumber))
            {
                PhoneNumber = PhoneNumber.Substring(0, PhoneNumber.Length - 1);
            }
        }

        /// <summary>
        /// Command triggered to initiate an outgoing call using the dialed number.
        /// </summary>
        [RelayCommand]
        public void Call()
        {
            if (!string.IsNullOrEmpty(PhoneNumber))
            {
                IsInCall = true;
                IsIncomingCall = false;
                ContactName = "Wybieranie...";
            }
        }

        /// <summary>
        /// Command triggered to end an active phone call.
        /// </summary>
        [RelayCommand]
        public void EndCall()
        {
            IsInCall = false;
            IsIncomingCall = false;
        }

        /// <summary>
        /// Command triggered to answer an incoming phone call.
        /// </summary>
        [RelayCommand]
        public void AnswerCall()
        {
            IsIncomingCall = false;
        }
    }
}