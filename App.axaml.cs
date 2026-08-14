using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using phonetolinux.Services;
using phonetolinux.ViewModels;
using phonetolinux.Views;

namespace phonetolinux;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Jeśli adres IP nie jest zapisany, najpierw uruchamiamy okno konfiguracji jako główne okno
            if (string.IsNullOrEmpty(PhoneConfig.PhoneIp))
            {
                var configWindow = new IpConfigWindow();
                desktop.MainWindow = configWindow;
                
                // Po zamknięciu okna konfiguracji (jeśli użytkownik wpisał IP), uruchamiamy właściwą aplikację
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
                        // Jeśli użytkownik zamknął okno bez wpisania IP, zamykamy aplikację
                        desktop.Shutdown();
                    }
                };
            }
            else
            {
                // Jeśli IP jest już zapisane, startujemy normalnie od razu z MainWindow
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(),
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}