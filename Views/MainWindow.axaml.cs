using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using phonetolinux.Services;
using phonetolinux.ViewModels;
using System;

namespace phonetolinux.Views;

public partial class MainWindow : Window
{
    private readonly PhonetoLinuxCall _callService;
    private readonly PhonetoLinuxSMS _smsService; // Poprawiono nazwę typu
    private DispatcherTimer _smsPollTimer;

    public MainWindow()
    {
        InitializeComponent();

        _callService = new PhonetoLinuxCall();
        _smsService = new PhonetoLinuxSMS(); // Poprawiono inicjalizację

        StartSmsPolling();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        await InitializeCallMonitoringAsync();
    }

    private async System.Threading.Tasks.Task InitializeCallMonitoringAsync()
    {
        try
        {
            Title = "phonetolinux - Gotowy";
        }
        catch (Exception ex)
        {
            Title = $"phonetolinux - Błąd: {ex.Message}";
            Console.WriteLine($"Błąd monitorowania: {ex.Message}");
        }
    }

    private void StartSmsPolling()
    {
        _smsPollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };

        _smsPollTimer.Tick += async (sender, args) =>
        {
            try
            {
                var newMessages = await _smsService.GetIncomingSmsAsync();
                if (newMessages != null && newMessages.Count > 0)
                {
                    foreach (var msg in newMessages)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            if (DataContext is MainViewModel vm) 
                            { 
                                vm.AddIncomingSms(msg.sender, msg.text); 
                            }
                            Console.WriteLine($"[NOWY SMS] Od: {msg.sender} | Treść: {msg.text}");
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas pobierania SMS-ów: {ex.Message}");
            }
        };

        _smsPollTimer.Start();
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void Minimize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.SelectedTabIndex == 1)
        {
            if (!string.IsNullOrEmpty(e.Text) && char.IsLetterOrDigit(e.Text[0]))
            {
                vm.SearchQuery += e.Text;
                vm.FilterContacts();
                e.Handled = true;
            }
        }
        
        base.OnTextInput(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.D)
        {
            OpenDebugWindow();
            e.Handled = true;
            base.OnKeyDown(e);
            return;
        }

        if (DataContext is MainViewModel vm && vm.SelectedTabIndex == 1)
        {
            if (e.Key == Key.Back && vm.SearchQuery.Length > 0)
            {
                vm.SearchQuery = vm.SearchQuery.Substring(0, vm.SearchQuery.Length - 1);
                vm.FilterContacts();
                e.Handled = true;
            }
        }
        
        base.OnKeyDown(e);
    }

    private void OpenDebugWindow()
    {
        var debugWindow = new DebugWindow();
        debugWindow.Show();
    }
}