using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using phonetolinux.Services;
using phonetolinux.ViewModels;
using phonetolinux.Views;
using phonetolinux.Security;

namespace phonetolinux;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // If the IP address is not configured yet, launch the setup window first
            if (string.IsNullOrEmpty(PhoneConfig.PhoneIp))
            {
                var configWindow = new IpConfigWindow();
                desktop.MainWindow = configWindow;

                // Handle setup window closing event
                configWindow.Closed += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(PhoneConfig.PhoneIp))
                    {
                        var mainWindow = new MainWindow
                        {
                            DataContext = new MainViewModel()
                        };
                        desktop.MainWindow = mainWindow;
                        mainWindow.Show();
                    }
                    else
                    {
                        // Shutdown application if configuration was cancelled
                        desktop.Shutdown();
                    }
                };
            }
            else
            {
                // Attempt to automatically discover updated phone IP on network change
                var autoIpService = new AutochangeIP();

                // Retrieve the pairing secret from PhoneConfig or Services
                string pairingSecret = PhoneConfig.PairingSecret;

                if (!string.IsNullOrEmpty(pairingSecret))
                {
                    string? discoveredIp = await autoIpService.DiscoverPhoneIpAsync(pairingSecret, timeoutMs: 2000);

                    if (!string.IsNullOrEmpty(discoveredIp))
                    {
                        PhoneConfig.PhoneIp = discoveredIp;
                        // PhoneConfig.Save();
                    }
                }

                // Launch main application window
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(),
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}