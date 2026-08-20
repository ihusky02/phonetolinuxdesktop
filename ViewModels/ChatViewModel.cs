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

public partial class ChatViewModel : ObservableObject
{
    private readonly SmsPlugin _smsService;
    private readonly ChatHistoryPlugin _historyService;
    private readonly SmsHistoryPlugin _smsHistoryService;
    private readonly ConversationsPlugin _conversationsService;

    [ObservableProperty]
    private string _phoneNumber = "";

    [ObservableProperty]
    private string _contactName = "Wybierz kontakt";

    [ObservableProperty]
    private string _currentMessageText = "";

    [ObservableProperty]
    private ObservableCollection<ChatMessageItem> _messagesList = new();

    [ObservableProperty]
    private ObservableCollection<ChatConversationItem> _recentConversations = new();

    [ObservableProperty]
    private ChatConversationItem? _selectedConversation;

    public ChatViewModel()
    {
        _smsService = new SmsPlugin();
        _historyService = new ChatHistoryPlugin();
        _smsHistoryService = new SmsHistoryPlugin();
        _conversationsService = new ConversationsPlugin();

        StartRealtimeSmsListener();
        _ = LoadConversationsAndSyncAsync();
    }

    partial void OnSelectedConversationChanged(ChatConversationItem? value)
    {
        if (value != null)
        {
            ContactName = string.IsNullOrEmpty(value.ContactName) ? value.PhoneNumber : value.ContactName;
            PhoneNumber = value.PhoneNumber ?? "";

            var context = new ChatContext
            {
                ContactName = ContactName,
                PhoneNumber = PhoneNumber
            };
            _ = InitializeChatAsync(context);
        }
    }

    public async Task LoadConversationsAndSyncAsync()
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

                if (SelectedConversation == null && RecentConversations.Count > 0)
                {
                    SelectedConversation = RecentConversations[0];
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Błąd ładowania konwersacji]: {ex.Message}");
        }
    }

    public async Task InitializeChatAsync(ChatContext context)
    {
        ContactName = context.ContactName;
        PhoneNumber = context.PhoneNumber;

        if (!string.IsNullOrEmpty(PhoneNumber) && PhoneNumber.Any(char.IsLetter))
        {
            var match = context.AllContacts?.FirstOrDefault(x => string.Equals(x.Name?.Trim(), PhoneNumber.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match != null && !string.IsNullOrEmpty(match.PhoneNumber))
            {
                PhoneNumber = match.PhoneNumber;
            }
        }

        await LoadChatHistoryAsync(ContactName, PhoneNumber);
    }

    private async Task LoadChatHistoryAsync(string contactName, string phoneNumber)
    {
        await Dispatcher.UIThread.InvokeAsync(() => MessagesList.Clear());

        List<ChatMessageItem>? history = null;
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
                foreach (var msg in history) MessagesList.Add(msg);
            }
        });
    }

    private void StartRealtimeSmsListener()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(2000);
                try
                {
                    if (!string.IsNullOrEmpty(PhoneNumber) && PhoneNumber != "Wybierz kontakt")
                    {
                        var freshHistory = await _smsHistoryService.GetChatHistoryFromServerAsync(PhoneNumber);
                        
                        if (freshHistory != null)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                if (freshHistory.Count != MessagesList.Count)
                                {
                                    MessagesList.Clear();
                                    foreach (var msg in freshHistory)
                                    {
                                        MessagesList.Add(msg);
                                    }
                                }
                            });
                        }
                    }
                }
                catch { }
            }
        });
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessageText)) return;
        if (string.IsNullOrEmpty(PhoneNumber)) return;

        string textToSend = CurrentMessageText;

        bool success = await _smsService.SendSmsAsync(PhoneNumber, textToSend);
        
        if (success)
        {
            var msg = new ChatMessageItem { Text = textToSend, IsOutgoing = true };
            MessagesList.Add(msg);

            if (!string.IsNullOrEmpty(ContactName) && ContactName != "Wybierz kontakt")
            {
                await _historyService.SaveHistoryAsync(ContactName, MessagesList);
            }

            CurrentMessageText = "";
        }
    }

    public async void AddIncomingSms(ChatContext context)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            bool isCurrentChat = string.Equals(ContactName?.Trim(), context.ContactName?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(PhoneNumber?.Trim(), context.ContactName?.Trim(), StringComparison.OrdinalIgnoreCase);

            if (!isCurrentChat && context.AllContacts != null)
            {
                var contactMatch = context.AllContacts.FirstOrDefault(x => string.Equals(x.PhoneNumber?.Trim(), context.ContactName?.Trim(), StringComparison.OrdinalIgnoreCase));
                if (contactMatch != null)
                {
                    isCurrentChat = string.Equals(ContactName?.Trim(), contactMatch.Name?.Trim(), StringComparison.OrdinalIgnoreCase);
                }
            }

            if (isCurrentChat)
            {
                MessagesList.Add(new ChatMessageItem { Text = context.RawMessageText, IsOutgoing = false });
            }
        });

        await _historyService.SaveHistoryAsync(context.ContactName, MessagesList);
    }
}