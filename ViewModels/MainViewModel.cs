using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AndroidCallBridge.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private string _searchQuery = "";

    [ObservableProperty]
    private bool _isSearchActive = false;

    [ObservableProperty]
    private ObservableCollection<ContactItem> _contactsList = new();

    // Główna, ukryta lista wszystkich kontaktów
    private List<ContactItem> _allContacts = new();

    public MainViewModel()
    {
        LoadContactsFromPhone();
    }

    // Bezpieczna, publiczna metoda do odświeżania listy
    public void FilterContacts()
    {
        IsSearchActive = !string.IsNullOrEmpty(SearchQuery);
        
        var query = SearchQuery?.ToLower() ?? "";

        // Filtrowanie z zabezpieczeniem przed pustymi nazwami (null)
        var filtered = string.IsNullOrEmpty(query)
            ? _allContacts
            : _allContacts.Where(c => !string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(query)).ToList();

        ContactsList.Clear();
        foreach (var c in filtered)
        {
            ContactsList.Add(c);
        }
    }

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

    [RelayCommand]
    private void Call()
    {
        if (!string.IsNullOrEmpty(PhoneNumber))
        {
            ContactName = "Wychodzące...";
            IsIncomingCall = false;
            IsInCall = true;
            ExecuteAdbCommand($"shell am start -a android.intent.action.CALL -d tel:{PhoneNumber}");
        }
    }

    [RelayCommand]
    private void EndCall()
    {
        ExecuteAdbCommand("shell input keyevent KEYCODE_ENDCALL");
        IsInCall = false;
        IsIncomingCall = false;
        PhoneNumber = "";
        ContactName = "Nieznany";
    }

    [RelayCommand]
    private void AnswerCall()
    {
        ExecuteAdbCommand("shell input keyevent KEYCODE_CALL");
        IsIncomingCall = false;
    }

    [RelayCommand]
    private void CallSpecificNumber(string number)
    {
        if (!string.IsNullOrEmpty(number))
        {
            PhoneNumber = number;
            Call();
        }
    }
    
    [RelayCommand] // lub [RelayCommand] zależnie od Twojego atrybutu
    private void SendMessageToContact(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber)) return;

        try
        {
            // Przekazuje żądanie bezpośrednio do domyślnej przeglądarki głównego systemu ChromeOS
            Process.Start(new ProcessStartInfo
            {
                FileName = "cros-sensible-browser",
                Arguments = "https://messages.google.com/web/",
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch (System.Exception ex)
        {
            // Awaryjne fallback, gdyby cros-sensible-browser nie zadziałał
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = "https://messages.google.com/web/",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch (System.Exception fallbackEx)
            {
                System.Console.WriteLine($"Błąd otwierania przeglądarki ChromeOS: {fallbackEx.Message}");
            }
        }
    }

    private void LoadContactsFromPhone()
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "adb",
                Arguments = "shell content query --uri content://com.android.contacts/data/phones",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                string currentName = "";
                string currentPhone = "";

                foreach (var line in output.Split('\n'))
                {
                    if (line.Contains("display_name="))
                    {
                        var parts = line.Split("display_name=");
                        if (parts.Length > 1)
                        {
                            currentName = parts[1].Split(',')[0].Trim();
                        }
                    }

                    if (line.Contains("data4=") || line.Contains("data1="))
                    {
                        var parts = line.Contains("data4=") ? line.Split("data4=") : line.Split("data1=");
                        if (parts.Length > 1)
                        {
                            currentPhone = parts[1].Split(',')[0].Trim();
                        }
                    }

                    if (!string.IsNullOrEmpty(currentName) && !string.IsNullOrEmpty(currentPhone) && currentName != "NULL")
                    {
                        // Zapisujemy do głównej, bezpiecznej listy
                        if (!_allContacts.Any(c => c.PhoneNumber == currentPhone))
                        {
                            _allContacts.Add(new ContactItem { Name = currentName, PhoneNumber = currentPhone });
                        }
                        currentName = "";
                        currentPhone = "";
                    }
                }
            }
            
            // Po załadowaniu wszystkiego, wypełniamy listę widoczną dla użytkownika
            FilterContacts();
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Błąd pobierania kontaktów: {ex.Message}");
        }
    }

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