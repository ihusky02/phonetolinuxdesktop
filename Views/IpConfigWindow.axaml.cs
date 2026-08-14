using Avalonia.Controls;
using Avalonia.Interactivity;
using phonetolinux.Services;

namespace phonetolinux.Views
{
    public partial class IpConfigWindow : Window
    {
        public IpConfigWindow()
        {
            InitializeComponent();
            // Wpisz ostatnio zapamiętane IP, jeśli istnieje
            IpTextBox.Text = PhoneConfig.PhoneIp;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string ip = IpTextBox.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(ip))
            {
                PhoneConfig.SaveIp(ip);
            }
            Close(); // Zamknij okno konfiguracji i przejdź do głównego interfejsu
        }
    }
}