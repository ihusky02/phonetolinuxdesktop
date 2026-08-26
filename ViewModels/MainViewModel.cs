using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using phonetolinux.Models;
using phonetolinux.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Threading;

namespace phonetolinux.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly PhoneCallPlugin _callService;
    private readonly ContactsPlugin _contactsService;

    [ObservableProperty]
    private string _phoneNumber = "";

    [ObservableProperty]
    private bool _isInCall = false;

    [ObservableProperty]
    private bool _isIncomingCall = false;

    [ObservableProperty]
    private string _contactName = "Wybierz kontakt";

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private string _searchQuery = "";

    partial void OnSearchQueryChanged(string value)
    {
        FilterContacts();
    }

    [ObservableProperty]
    private bool _isSearchActive = false;

    [ObservableProperty]
    private ObservableCollection<ContactItem> _contactsList = new();

    [ObservableProperty]
    private ChatViewModel _activeChat = new();

    private List<ContactItem> _allContacts = new();

    public MainViewModel()
    {
        _callService = new PhoneCallPlugin();
        _contactsService = new ContactsPlugin();

        IsInCall = false;
        IsIncomingCall = false;

        _ = LoadContactsFromPhoneAsync();
    }

    private async Task LoadContactsFromPhoneAsync()
    {
        try
        {
            var contacts = await _contactsService.GetContactsAsync();
            var tempList = new List<ContactItem>();
            foreach (var c in contacts)
            {
                if (!string.IsNullOrEmpty(c.Name))
                {
                    string cleanPhone = !string.IsNullOrEmpty(c.PhoneNumber)
                        ? new string(c.PhoneNumber.Where(ch => char.IsDigit(ch) || ch == '+').ToArray())
                        : "Brak numeru";
                    
                    if (!tempList.Any(x => x.Name == c.Name))
                    {
                        tempList.Add(new ContactItem { Name = c.Name, PhoneNumber = cleanPhone });
                    }
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _allContacts.Clear();
                foreach (var item in tempList)
                {
                    _allContacts.Add(item);
                }
                FilterContacts();
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd pobierania kontaktów z telefonu: {ex.Message}");
        }
    }

    public void FilterContacts()
    {
        IsSearchActive = !string.IsNullOrEmpty(SearchQuery);
        var query = SearchQuery?.ToLower() ?? "";

        var filtered = string.IsNullOrEmpty(query)
            ? _allContacts
            : _allContacts.Where(c => !string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(query)).ToList();

        ContactsList.Clear();
        foreach (var c in filtered)
        {
            ContactsList.Add(c);
        }
    }

    public void AddIncomingSms(string sender, string text)
    {
        var context = new ChatContext
        {
            ContactName = sender,
            RawMessageText = text,
            AllContacts = _allContacts
        };
        ActiveChat.AddIncomingSms(context);
    }

    [RelayCommand]
    private void AppendNumber(string number) => PhoneNumber += number;

    [RelayCommand]
    private void Backspace()
    {
        if (PhoneNumber.Length > 0)
            PhoneNumber = PhoneNumber.Substring(0, PhoneNumber.Length - 1);
    }

    [RelayCommand]
    private async Task Call()
    {
        if (string.IsNullOrEmpty(PhoneNumber)) return;

        bool success = await _callService.StartCallAsync(PhoneNumber);
        if (success)
        {
            IsInCall = true;
            IsIncomingCall = false;
        }
    }

    [RelayCommand]
    private async Task EndCall()
    {
        await _callService.EndCallAsync();
        ResetCallState();
    }

    [RelayCommand]
    private async Task AnswerCall()
    {
        bool success = await _callService.AnswerCallAsync();
        if (success)
        {
            IsIncomingCall = false;
            IsInCall = true;
        }
    }

    [RelayCommand]
    private async Task CallSpecificNumber(string number)
    {
        if (!string.IsNullOrEmpty(number))
        {
            PhoneNumber = number;
            await Call();
        }
    }
    
    [RelayCommand]
    private void SendMessageToContact(string? phoneNumber)
    {
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            PhoneNumber = phoneNumber;
            var contact = _allContacts.FirstOrDefault(x => x.PhoneNumber == phoneNumber);
            if (contact != null)
            {
                ContactName = contact.Name;
                var context = new ChatContext
                {
                    ContactName = ContactName,
                    PhoneNumber = PhoneNumber,
                    AllContacts = _allContacts
                };
                _ = ActiveChat.InitializeChatAsync(context);
            }
            SelectedTabIndex = 2; // Zakładka czatu
        }
    }

    private void ResetCallState()
    {
        IsInCall = false;
        IsIncomingCall = false;
        PhoneNumber = "";
        ContactName = "Nieznany";
    }
}