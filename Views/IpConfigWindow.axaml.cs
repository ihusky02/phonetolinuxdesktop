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
            // Enter the last saved IP, if it exists.
            IpTextBox.Text = PhoneConfig.PhoneIp;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string ip = IpTextBox.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(ip))
            {
                PhoneConfig.SaveIp(ip);
            }
            Close(); // Close the configuration window and go to the main interface.
        }
    }
}