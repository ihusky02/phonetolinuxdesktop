using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using phonetolinux.Models;
using phonetolinux.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Threading;

namespace phonetolinux.ViewModels;

public partial class ChatConversationItem : ObservableObject
{
    [ObservableProperty]
    private string _contactName = "";

    [ObservableProperty]
    private string _phoneNumber = "";

    [ObservableProperty]
    private string _lastMessage = "";
}

public partial class MainViewModel : ViewModelBase
{
    private readonly PhonetoLinuxCall _callService;
    private readonly PhonetoLinuxSMS _smsService;
    private readonly PhonetoLinuxContacts _contactsService;
    private readonly phonetolinuxchathistory _historyService;
    private readonly phonetolinuxconversations _conversationsService;
    private readonly phonetolinuxsmshistory _smsHistoryService;
    private readonly PhonetoLinuxStream _smsStreamService; // Usługa nasłuchu w czasie rzeczywistym

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

    [ObservableProperty]
    private bool _isSearchActive = false;

    [ObservableProperty]
    private string _currentMessageText = "";

    [ObservableProperty]
    private ObservableCollection<ContactItem> _contactsList = new();

    [ObservableProperty]
    private ObservableCollection<ChatMessageItem> _messagesList = new();

    [ObservableProperty]
    private ObservableCollection<ChatConversationItem> _recentConversations = new();

    [ObservableProperty]
    private ChatConversationItem? _selectedConversation;

    private List<ContactItem> _allContacts = new();

    public MainViewModel()
    {
        _callService = new PhonetoLinuxCall();
        _smsService = new PhonetoLinuxSMS();
        _contactsService = new PhonetoLinuxContacts();
        _historyService = new phonetolinuxchathistory();
        _conversationsService = new phonetolinuxconversations();
        _smsHistoryService = new phonetolinuxsmshistory();
        _smsStreamService = new PhonetoLinuxStream();

        IsInCall = false;
        IsIncomingCall = false;

        _ = LoadContactsFromPhoneAsync();
        _ = LoadRecentConversationsAsync();

        // Uruchomienie nasłuchu przychodzących SMS-ów w czasie rzeczywistym
        StartRealtimeSmsListener();
    }

    private void StartRealtimeSmsListener()
    {
        try
        {
            // Pobieramy bazowy URL z klasy PhoneConfig i wyciągamy z niego IP oraz port
            string baseUrl = PhoneConfig.GetBaseUrl(); 
            
            if (string.IsNullOrEmpty(baseUrl))
            {
                Console.WriteLine("Brak skonfigurowanego adresu telefonu w PhoneConfig.");
                return;
            }

            Uri uri = new Uri(baseUrl);
            string phoneIp = uri.Host;
            int port = uri.Port > 0 ? uri.Port : 5000;

            _smsStreamService.StartListening(phoneIp, port, (sender, message) =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    AddIncomingSms(sender, message);
                });
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd uruchamiania strumienia SMS: {ex.Message}");
        }
    }

    private async Task LoadRecentConversationsAsync()
    {
        try
        {
            var phoneConversations = await _conversationsService.GetConversationsFromServerAsync();
            var list = new List<ChatConversationItem>();

            if (phoneConversations != null && phoneConversations.Count > 0)
            {
                var uniqueConversations = phoneConversations
                    .GroupBy(c => (c.contactName ?? c.phoneNumber)?.Trim(), StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First());

                foreach (var conv in uniqueConversations)
                {
                    list.Add(new ChatConversationItem 
                    { 
                        ContactName = string.IsNullOrEmpty(conv.contactName) ? conv.phoneNumber : conv.contactName, 
                        PhoneNumber = conv.phoneNumber ?? "", 
                        LastMessage = conv.lastMessage 
                    });
                }
            }
            else
            {
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string storageDir = Path.Combine(homeDir, ".phonetolinux", "chats");
                if (Directory.Exists(storageDir))
                {
                    var files = Directory.GetFiles(storageDir, "*.json");
                    foreach (var file in files)
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        list.Add(new ChatConversationItem { ContactName = name, PhoneNumber = name, LastMessage = "Historia zapisana" });
                    }
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                RecentConversations.Clear();
                foreach (var item in list.Take(6))
                {
                    RecentConversations.Add(item);
                }
            });
        }
        catch (Exception) { }
    }

    partial void OnSelectedConversationChanged(ChatConversationItem? value)
    {
        if (value != null)
        {
            ContactName = string.IsNullOrEmpty(value.ContactName) ? value.PhoneNumber : value.ContactName;
            
            if (!string.IsNullOrEmpty(value.PhoneNumber))
            {
                PhoneNumber = value.PhoneNumber;
            }
            else
            {
                var contact = _allContacts.FirstOrDefault(x => 
                    string.Equals(x.Name?.Trim(), value.ContactName?.Trim(), StringComparison.OrdinalIgnoreCase));

                if (contact != null && !string.IsNullOrEmpty(contact.PhoneNumber))
                {
                    PhoneNumber = contact.PhoneNumber;
                }
                else
                {
                    PhoneNumber = value.ContactName;
                }
            }
            
            _ = LoadChatHistoryForContact(ContactName, PhoneNumber);
        }
    }

    [RelayCommand]
    private void SelectConversation(ChatConversationItem? conversation)
    {
        if (conversation != null)
        {
            SelectedConversation = conversation;
        }
    }

    [RelayCommand]
    private async Task DeleteChat(ChatConversationItem? conversationToRemove)
    {
        var targetConversation = conversationToRemove ?? SelectedConversation;

        if (targetConversation == null && (string.IsNullOrEmpty(ContactName) || ContactName == "Wybierz kontakt"))
            return;

        string targetName = targetConversation?.ContactName ?? ContactName;
        string targetPhone = targetConversation?.PhoneNumber ?? PhoneNumber;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (SelectedConversation == targetConversation || SelectedConversation == null)
            {
                MessagesList.Clear();
                ContactName = "Wybierz kontakt";
                PhoneNumber = "";
                SelectedConversation = null;
            }
        });

        try
        {
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string storageDir = Path.Combine(homeDir, ".phonetolinux", "chats");
            
            if (Directory.Exists(storageDir))
            {
                var files = Directory.GetFiles(storageDir, "*.json");
                foreach (var file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (string.Equals(fileName, targetName, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(targetPhone) && string.Equals(fileName, targetPhone, StringComparison.OrdinalIgnoreCase)))
                    {
                        File.Delete(file);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas usuwania lokalnego pliku historii: {ex.Message}");
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (targetConversation != null && RecentConversations.Contains(targetConversation))
            {
                RecentConversations.Remove(targetConversation);
            }
            else
            {
                var itemToRemove = RecentConversations.FirstOrDefault(x => 
                    string.Equals(x.ContactName?.Trim(), targetName?.Trim(), StringComparison.OrdinalIgnoreCase));
                if (itemToRemove != null)
                {
                    RecentConversations.Remove(itemToRemove);
                }
            }
        });
    }

    private async Task LoadChatHistoryForContact(string contactName, string phoneNumber = "")
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            MessagesList.Clear();
        });

        List<ChatMessageItem> history = null;

        if (!string.IsNullOrEmpty(phoneNumber))
        {
            history = await _smsHistoryService.GetChatHistoryFromServerAsync(phoneNumber);
        }

        if (history == null || history.Count == 0)
        {
            history = await _historyService.LoadHistoryAsync(contactName);
            if ((history == null || history.Count == 0) && !string.IsNullOrEmpty(phoneNumber))
            {
                history = await _historyService.LoadHistoryAsync(phoneNumber);
            }
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            MessagesList.Clear();
            if (history != null)
            {
                foreach (var msg in history)
                {
                    MessagesList.Add(msg);
                }
            }
        });
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

    public async void AddIncomingSms(string sender, string text)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // Sprawdzamy, czy SMS pochodzi od osoby/numeru, z którą obecnie rozmawiamy
            bool isCurrentChat = string.Equals(ContactName?.Trim(), sender?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(PhoneNumber?.Trim(), sender?.Trim(), StringComparison.OrdinalIgnoreCase);

            if (isCurrentChat)
            {
                var newMsg = new ChatMessageItem 
                { 
                    Text = text, 
                    IsOutgoing = false 
                };
                MessagesList.Add(newMsg);
            }

            UpdateRecentConversations(sender, text);
        });

        await _historyService.SaveHistoryAsync(sender, MessagesList);
    }

    private void UpdateRecentConversations(string sender, string lastMsg)
    {
        var existing = RecentConversations.FirstOrDefault(x => 
            string.Equals(x.ContactName?.Trim(), sender?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            existing.LastMessage = lastMsg;
            int index = RecentConversations.IndexOf(existing);
            if (index > 0)
            {
                RecentConversations.Move(index, 0);
            }
        }
        else
        {
            RecentConversations.Insert(0, new ChatConversationItem { ContactName = sender, PhoneNumber = sender, LastMessage = lastMsg });
            if (RecentConversations.Count > 6) RecentConversations.RemoveAt(6);
        }
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
                _ = LoadChatHistoryForContact(contact.Name, contact.PhoneNumber);
            }
            SelectedTabIndex = 2;
        }
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessageText)) return;

        string targetPhone = PhoneNumber; 
        if (string.IsNullOrEmpty(targetPhone)) return;

        var matchedContact = _allContacts.FirstOrDefault(x => 
            string.Equals(x.Name?.Trim(), targetPhone.Trim(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ContactName?.Trim(), x.Name?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (matchedContact != null && !string.IsNullOrEmpty(matchedContact.PhoneNumber))
        {
            targetPhone = matchedContact.PhoneNumber;
        }

        Console.WriteLine($"[DEBUG SEND] Wysyłam SMS do: {targetPhone} | Treść: {CurrentMessageText}");

        bool success = await _smsService.SendSmsAsync(targetPhone, CurrentMessageText);
        
        if (success)
        {
            var msg = new ChatMessageItem { Text = CurrentMessageText, IsOutgoing = true };
            MessagesList.Add(msg);

            if (!string.IsNullOrEmpty(ContactName) && ContactName != "Wybierz kontakt")
            {
                UpdateRecentConversations(ContactName, CurrentMessageText);
                await _historyService.SaveHistoryAsync(ContactName, MessagesList);
            }

            CurrentMessageText = "";
        }
        else
        {
            Console.WriteLine("Nie udało się wysłać wiadomości SMS.");
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

public class ChatMessageItem
{
    public string Text { get; set; } = "";
    public bool IsOutgoing { get; set; } = true;
}