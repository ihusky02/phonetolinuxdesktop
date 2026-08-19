using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using phonetolinux.ViewModels;

namespace phonetolinux.Views;

/// <summary>
/// Klasa logiki głównego okna aplikacji.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Przykład użycia załadowanej wtyczki w oknie głównym:
        // string wynikKonwersacji = Program.GlobalPluginManager.ExecutePlugin("/conversations", "");
    }

    /// <summary>
    /// Obsługa przesuwania okna za własny pasek tytułowy.
    /// </summary>
    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    /// <summary>Minimalizuje okno aplikacji.</summary>
    private void Minimize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>Maksymalizuje lub przywraca normalny rozmiar okna.</summary>
    private void Maximize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    /// <summary>Zamyka aplikację.</summary>
    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}