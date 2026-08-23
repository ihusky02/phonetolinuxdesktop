using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneToLinux.Security;

namespace phonetolinux.ViewModels
{
    /// <summary>
    /// ViewModel for the main application window.
    /// Controls overall navigation state, first-launch pairing overlays, and sub-viewmodels.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly SecureStorageService _storageService;

        [ObservableProperty]
        private bool _isPaired;

        [ObservableProperty]
        private int _selectedTabIndex;

        [ObservableProperty]
        private PairingViewModel _pairing;

        public MainViewModel()
        {
            _storageService = new SecureStorageService();
            Pairing = new PairingViewModel();

            // Evaluate stored credentials on application launch
            CheckPairingStatus();
        }

        /// <summary>
        /// Inspects the secure storage directory to determine if the device has already been paired.
        /// </summary>
        public void CheckPairingStatus()
        {
            // Set IsPaired to true if session credentials exist in secure storage
            IsPaired = _storageService.FileExists("paired_device.dat");

            if (IsPaired)
            {
                SelectedTabIndex = 0; // Default to Dialer tab
            }
            else
            {
                SelectedTabIndex = 3; // Fallback index for initial setup
            }
        }

        /// <summary>
        /// Invoked when the QR code handshake completes successfully from the mobile app.
        /// </summary>
        [RelayCommand]
        public void OnPairingCompleted()
        {
            IsPaired = true;
            SelectedTabIndex = 0; // Automatically switch to the Dialer workspace
        }

        /// <summary>
        /// Clears all stored encrypted credentials and reverts the UI to the initial QR pairing screen.
        /// Useful when reinstalling the Android or Linux application.
        /// </summary>
        [RelayCommand]
        public void UnpairDevice()
        {
            _storageService.DeleteFile("paired_device.dat");
            _storageService.DeleteFile("session_keys.dat");

            IsPaired = false;
            Pairing.GeneratePairingQrCode();
        }
    }
}