using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AndroidCallBridge.Models;

namespace AndroidCallBridge.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _phoneNumber = "";

    [ObservableProperty]
    private bool _isInCall = false;

    [ObservableProperty]
    private bool _isIncomingCall = false;

    [ObservableProperty]
    private string _contactName = "Nieznany";
    
    [ObservableProperty]
    private ObservableCollection<ContactItem> _contactsList = new();

    [RelayCommand]
    private void AppendNumber(string number)
    {
        PhoneNumber += number;
    }

    [RelayCommand]
    private void Backspace()
    {
        if (PhoneNumber.Length > 0)
        {
            PhoneNumber = PhoneNumber.Substring(0, PhoneNumber.Length - 1);
        }
    }

    // Wywołanie połączenia na S25 Ultra przez ADB
    [RelayCommand]
    private void Call()
    {
        if (!string.IsNullOrEmpty(PhoneNumber))
        {
            ContactName = "Wychodzące...";
            IsIncomingCall = false;
            IsInCall = true;

            // Wywołanie natywnego dialera Androida przez adb shell
            ExecuteAdbCommand($"shell am start -a android.intent.action.CALL -d tel:{PhoneNumber}");
        }
    }

    // Rozłączenie / Odrzucenie połączenia na telefonie
    [RelayCommand]
    private void EndCall()
    {
        // Symulacja naciśnięcia przycisku zakończenia rozmowy przez ADB
        ExecuteAdbCommand("shell input keyevent KEYCODE_ENDCALL");

        IsInCall = false;
        IsIncomingCall = false;
        PhoneNumber = "";
        ContactName = "Nieznany";
    }
    
    [RelayCommand]
    private void CallSpecificNumber(string number)
    {
        if (!string.IsNullOrEmpty(number))
        {
            PhoneNumber = number;
            Call(); // Uruchamia logikę połączenia
        }
    }

    // Odebranie połączenia przychodzącego na telefonie
    [RelayCommand]
    private void AnswerCall()
    {
        // Symulacja odebrania połączenia przez ADB
        ExecuteAdbCommand("shell input keyevent KEYCODE_CALL");
        IsIncomingCall = false;
    }

    // Pomocnicza metoda do wywoływania komend adb w systemie Linux
    private void ExecuteAdbCommand(string arguments)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "adb",
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Błąd ADB: {ex.Message}");
        }
    }
}