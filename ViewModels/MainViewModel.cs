using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using phonetolinux.Services;
using phonetolinux.Models;
using PhoneToLinux.Security;
using phonetolinux.Plugins; 

namespace phonetolinux.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private static readonly byte[] MasterKey = SHA256.HashData(Encoding.UTF8.GetBytes("PhoneToLinux_MasterKey2026_Salt"));
        private static readonly HttpClient SharedHttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        private readonly SecureStorageService _storageService;
        private readonly ContactsPlugin _contactsPlugin;
        private readonly SmsPlugin _smsPlugin;
        private readonly ConversationsPlugin _conversationsPlugin;
        private readonly PhoneCallPlugin _phoneCallPlugin;
        private readonly PhoneSsePlugin _phoneSsePlugin; 
        private readonly LinuxNotificationPlugin _notificationPlugin;
        private readonly string _storageDirectory;
        private PairingListenerService? _pairingListener;

        [ObservableProperty]
        private bool _isPaired;

        [ObservableProperty]
        private int _selectedTabIndex;

        [ObservableProperty]
        private PairingViewModel _pairing;

        [ObservableProperty]
        private DialerViewModel _dialer = new();

        [ObservableProperty]
        private ChatViewModel _activeChat = new();

        [ObservableProperty]
        private string _currentTheme = "Dark";

        [ObservableProperty]
        private ChatConversationItem? _selectedConversation;

        [ObservableProperty]
        private bool _isInCall = false;

        [ObservableProperty]
        private bool _isIncomingCall = false;

        [ObservableProperty]
        private string _contactName = "Unknown";

        [ObservableProperty]
        private string _phoneNumber = "";

        [ObservableProperty]
        private string _currentMessageText = "";

        [ObservableProperty]
        private ObservableCollection<ContactItem> _contactsList = new();

        [ObservableProperty]
        private ObservableCollection<ChatConversationItem> _recentConversations = new();

        [ObservableProperty]
        private ObservableCollection<ChatMessageItem> _messagesList = new();

        public MainViewModel()
        {
            _storageService = new SecureStorageService(MasterKey);
            _contactsPlugin = new ContactsPlugin(SharedHttpClient);
            _smsPlugin = new SmsPlugin(SharedHttpClient);
            _conversationsPlugin = new ConversationsPlugin(SharedHttpClient);
            _phoneCallPlugin = new PhoneCallPlugin(SharedHttpClient);
            _phoneSsePlugin = new PhoneSsePlugin(); 
            _notificationPlugin = new LinuxNotificationPlugin();
            
            _storageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "phonetolinux");
            if (!Directory.Exists(_storageDirectory)) Directory.CreateDirectory(_storageDirectory);

            // Subscribe to SSE plugin events
            _phoneSsePlugin.OnCallReceived += HandleIncomingCall;
            _phoneSsePlugin.OnCallEnded += HandleCallEnded;
            _phoneSsePlugin.OnSmsReceived += HandleIncomingSms;

            _pairing = new PairingViewModel();
            CheckPairingStatus();
        }

        public void CheckPairingStatus()
        {
            string pairedFilePath = Path.Combine(_storageDirectory, "paired_device.dat");
            IsPaired = File.Exists(pairedFilePath);

            if (IsPaired)
            {
                SelectedTabIndex = 0;
                try
                {
                    string decryptedPayload = _storageService.ReadAndDecrypt(pairedFilePath);
                    using var jsonDoc = JsonDocument.Parse(decryptedPayload);
                    if (jsonDoc.RootElement.TryGetProperty("phoneIp", out var ipProp))
                    {
                        string savedIp = ipProp.GetString() ?? "";
                        if (!string.IsNullOrEmpty(savedIp))
                        {
                            Console.WriteLine($"[SSE START] Starting listener on IP: {savedIp}");
                            
                            // Initialize the SSE plugin directly based on the IP loaded from the secure file
                            _phoneSsePlugin.Initialize(savedIp, 5000); 
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error instead of swallowing it silently
                    Console.WriteLine($"[SSE ERROR] Error during SSE plugin startup: {ex.Message}");
                }
            }
            else
            {
                SelectedTabIndex = 3;
                _pairingListener = new PairingListenerService(this);
                _pairingListener.StartListening(5000);
            }
        }

        // --- SSE Event Handlers ---

        private void HandleIncomingCall(string number, string sender)
        {
            // Fallback: if number is empty/unknown, try to check sender argument
            string rawNumber = !string.IsNullOrEmpty(number) && number != "Unknown" ? number : sender;
            if (string.IsNullOrEmpty(rawNumber)) rawNumber = "Unknown";

            // Resolve contact name using the raw phone number
            string displayName = ResolveContactName(rawNumber, null);

            Console.WriteLine($"[TEST] Incoming call detected from: '{rawNumber}', Resolved Name: '{displayName}'");

            _notificationPlugin.ShowNotification(
                title: "Incoming Call",
                message: $"{displayName} ({rawNumber})",
                icon: "call-start",
                urgency: "critical"
            );

            // Dispatch to UI thread to update the view and force overlay open
            Dispatcher.UIThread.Post(() =>
            {
                PhoneNumber = rawNumber;
                ContactName = displayName;
                IsIncomingCall = true;
                IsInCall = true;
            });
        }

        private void HandleCallEnded()
        {
            Console.WriteLine("[TEST] Call ended event received.");
            
            Dispatcher.UIThread.Post(() =>
            {
                IsInCall = false;
                IsIncomingCall = false;
            });
        }

        private void HandleIncomingSms(string sender, string message)
        {
            _notificationPlugin.ShowNotification(
                title: $"SMS from: {sender}",
                message: message,
                icon: "mail-unread",
                urgency: "normal"
            );

            Dispatcher.UIThread.Post(() =>
            {
                if (PhoneNumber == sender || ContactName == sender)
                {
                    MessagesList.Add(new ChatMessageItem { Text = message, IsOutgoing = false });
                }
                _ = LoadConversationsAsync();
            });
        }

        // --------------------------

        [RelayCommand]
        public void SelectTab(object? parameter)
        {
            if (parameter != null && int.TryParse(parameter.ToString(), out int index))
            {
                SelectedTabIndex = index;
                if (index == 1) _ = LoadContactsAsync();
                else if (index == 2) _ = LoadConversationsAsync();
            }
        }

        public async Task LoadContactsAsync()
        {
            try
            {
                var fetchedContacts = await _contactsPlugin.GetContactsAsync();
                if (fetchedContacts != null && fetchedContacts.Count > 0)
                {
                    ContactsList.Clear();
                    foreach (var contact in fetchedContacts) ContactsList.Add(contact);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTACTS ERROR] {ex.Message}");
            }
        }

        public async Task LoadConversationsAsync()
        {
            try
            {
                if (ContactsList.Count == 0) await LoadContactsAsync();

                var fetchedThreads = await _conversationsPlugin.GetConversationsFromServerAsync();
                if (fetchedThreads != null && fetchedThreads.Count > 0)
                {
                    RecentConversations.Clear();
                    foreach (var thread in fetchedThreads)
                    {
                        string rawAddr = !string.IsNullOrWhiteSpace(thread.PhoneNumber) ? thread.PhoneNumber : thread.ContactName ?? "";
                        string displayName = ResolveContactName(rawAddr, thread.ContactName);
                        RecentConversations.Add(new ChatConversationItem { ContactName = displayName, LastMessage = thread.lastMessage ?? "...", PhoneNumber = rawAddr });
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONVERSATIONS ERROR] {ex.Message}");
            }
        }

        private string ResolveContactName(string rawPhoneNumber, string? serverContactName)
        {
            if (!string.IsNullOrWhiteSpace(serverContactName) && serverContactName != rawPhoneNumber && !serverContactName.StartsWith("+")) return serverContactName;
            if (string.IsNullOrWhiteSpace(rawPhoneNumber)) return "Unknown Contact";
            if (rawPhoneNumber.Any(char.IsLetter)) return rawPhoneNumber;

            string cleanTarget = GetLast9Digits(rawPhoneNumber);
            if (string.IsNullOrEmpty(cleanTarget)) return rawPhoneNumber;

            foreach (var contact in ContactsList)
            {
                if (string.IsNullOrWhiteSpace(contact.PhoneNumber)) continue;
                string cleanContactNum = GetLast9Digits(contact.PhoneNumber);
                if (cleanTarget == cleanContactNum && !string.IsNullOrWhiteSpace(contact.Name)) return contact.Name;
            }
            return rawPhoneNumber;
        }

        private static string GetLast9Digits(string number)
        {
            if (string.IsNullOrWhiteSpace(number)) return string.Empty;
            string digits = new string(number.Where(char.IsDigit).ToArray());
            return digits.Length > 9 ? digits.Substring(digits.Length - 9) : digits;
        }

        [RelayCommand]
        public void SendMessageToContact(string phoneNumber)
        {
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                SelectedTabIndex = 2;
                PhoneNumber = phoneNumber;
                ContactName = ResolveContactName(phoneNumber, null);
                _ = LoadMessagesForNumberAsync(phoneNumber);
            }
        }

        [RelayCommand]
        public async Task CallSpecificNumber(string phoneNumber)
        {
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                PhoneNumber = phoneNumber;
                ContactName = ResolveContactName(phoneNumber, null);
                IsInCall = true;
                IsIncomingCall = false;
                await _phoneCallPlugin.StartCallAsync(phoneNumber);
            }
        }

        [RelayCommand]
        public async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(CurrentMessageText)) return;
            string targetNumber = string.IsNullOrEmpty(PhoneNumber) && ActiveChat != null ? ActiveChat.PhoneNumber : PhoneNumber;
            if (string.IsNullOrEmpty(targetNumber)) return;

            string textToSend = CurrentMessageText;
            bool success = await _smsPlugin.SendSmsAsync(targetNumber, textToSend);

            if (success)
            {
                MessagesList.Add(new ChatMessageItem { Text = textToSend, IsOutgoing = true });
                CurrentMessageText = "";
            }
        }

        [RelayCommand]
        public async Task SelectConversation(ChatConversationItem conversation)
        {
            if (conversation != null)
            {
                string targetAddress = !string.IsNullOrWhiteSpace(conversation.PhoneNumber) ? conversation.PhoneNumber : conversation.ContactName;
                ContactName = string.IsNullOrWhiteSpace(conversation.ContactName) ? targetAddress : conversation.ContactName;
                PhoneNumber = targetAddress;

                if (ActiveChat != null) ActiveChat.PhoneNumber = PhoneNumber;
                
                await LoadMessagesForNumberAsync(targetAddress);
            }
        }

        [RelayCommand]
        public async Task DeleteConversation(ChatConversationItem? conversation)
        {
            if (conversation == null) return;
            string targetAddress = !string.IsNullOrWhiteSpace(conversation.PhoneNumber) ? conversation.PhoneNumber : conversation.ContactName;
            if (string.IsNullOrWhiteSpace(targetAddress)) return;

            bool success = await _conversationsPlugin.DeleteConversationAsync(targetAddress);
            if (success)
            {
                RecentConversations.Remove(conversation);
                if (PhoneNumber == targetAddress || ContactName == conversation.ContactName)
                {
                    MessagesList.Clear();
                    PhoneNumber = "";
                    ContactName = "";
                    SelectedConversation = null;
                }
            }
        }

        /// <summary>
        /// Highly resilient JSON message parser with exhaustive error logging.
        /// </summary>
        private async Task LoadMessagesForNumberAsync(string addressInput)
        {
            if (string.IsNullOrWhiteSpace(addressInput)) return;

            List<string> candidates = new List<string> { addressInput.Trim() };
            if (addressInput.Any(char.IsLetter))
            {
                candidates.Add(addressInput.ToLowerInvariant());
                candidates.Add(addressInput.ToUpperInvariant());
            }
            else
            {
                string cleanNoSpaces = new string(addressInput.Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '(' && c != ')').ToArray());
                if (!candidates.Contains(cleanNoSpaces)) candidates.Add(cleanNoSpaces);

                string last9 = GetLast9Digits(addressInput);
                if (!string.IsNullOrEmpty(last9) && !candidates.Contains(last9)) candidates.Add(last9);

                string withPlus48 = "+48" + last9;
                if (!candidates.Contains(withPlus48)) candidates.Add(withPlus48);
            }

            foreach (var target in candidates.Distinct())
            {
                try
                {
                    string url = $"{PhoneConfig.GetBaseUrl()}/messages?address={Uri.EscapeDataString(target)}";
                    Console.WriteLine($"\n[MESSAGES DEBUG] Requesting URL: {url}");
                    
                    HttpResponseMessage response = await SharedHttpClient.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"[MESSAGES DEBUG] RAW JSON RESPONSE from Android: {json}");

                        List<ChatMessageItem>? fetchedMessages = null;
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                        using (JsonDocument doc = JsonDocument.Parse(json))
                        {
                            if (doc.RootElement.ValueKind == JsonValueKind.Array)
                            {
                                fetchedMessages = JsonSerializer.Deserialize<List<ChatMessageItem>>(json, options);
                            }
                            else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                            {
                                if (doc.RootElement.TryGetProperty("messages", out var msgs) && msgs.ValueKind == JsonValueKind.Array)
                                {
                                    fetchedMessages = JsonSerializer.Deserialize<List<ChatMessageItem>>(msgs.GetRawText(), options);
                                }
                                else if (doc.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                                {
                                    fetchedMessages = JsonSerializer.Deserialize<List<ChatMessageItem>>(data.GetRawText(), options);
                                }
                            }
                        }

                        if (fetchedMessages != null && fetchedMessages.Count > 0)
                        {
                            MessagesList.Clear();
                            foreach (var msg in fetchedMessages) MessagesList.Add(msg);
                            Console.WriteLine($"[MESSAGES DEBUG] SUCCESS - Displaying {fetchedMessages.Count} messages in UI.\n");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("[MESSAGES DEBUG] JSON was parsed successfully, but the message array was empty []");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[MESSAGES DEBUG] Server returned HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MESSAGES ERROR] Critical failure parsing messages: {ex.Message}");
                }
            }
            
            MessagesList.Clear();
        }

        [RelayCommand]
        public void OnPairingCompleted()
        {
            IsPaired = true;
            SelectedTabIndex = 0;
            
            // Read the IP directly from the secured pairing file right after pairing
            string pairedFilePath = Path.Combine(_storageDirectory, "paired_device.dat");
            if (File.Exists(pairedFilePath))
            {
                try
                {
                    string decryptedPayload = _storageService.ReadAndDecrypt(pairedFilePath);
                    using var jsonDoc = JsonDocument.Parse(decryptedPayload);
                    
                    if (jsonDoc.RootElement.TryGetProperty("phoneIp", out var ipProp))
                    {
                        string? savedIp = ipProp.GetString();
                        if (!string.IsNullOrEmpty(savedIp))
                        {
                            _phoneSsePlugin.Initialize(savedIp, 5000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PAIRING ERROR] Could not read IP for SSE initialization: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        public void UnpairDevice()
        {
            string pFile = Path.Combine(_storageDirectory, "paired_device.dat");
            if (File.Exists(pFile)) File.Delete(pFile);
            IsPaired = false;
            SelectedTabIndex = 3;
            _phoneSsePlugin.Shutdown(); // Stop listening when unpaired
            Pairing.GeneratePairingPinCode();
        }

        [RelayCommand]
        public void AppendNumber(string number) => PhoneNumber += number;

        [RelayCommand]
        public void Backspace()
        {
            if (!string.IsNullOrEmpty(PhoneNumber)) PhoneNumber = PhoneNumber.Substring(0, PhoneNumber.Length - 1);
        }

        [RelayCommand]
        public async Task Call()
        {
            if (!string.IsNullOrEmpty(PhoneNumber))
            {
                IsInCall = true;
                IsIncomingCall = false;
                ContactName = ResolveContactName(PhoneNumber, null);
                await _phoneCallPlugin.StartCallAsync(PhoneNumber);
            }
        }

        [RelayCommand]
        public async Task EndCall()
        {
            IsInCall = false;
            IsIncomingCall = false;
            
            // Depending on the state, we can either end an active call or reject an incoming one
            if (IsIncomingCall)
            {
                await _phoneSsePlugin.RejectCallAsync();
            }
            else
            {
                await _phoneCallPlugin.EndCallAsync();
            }
        }

        [RelayCommand]
        public async Task AnswerCall()
        {
            IsIncomingCall = false;
            // Use the SSE plugin to send the answer command to Android
            await _phoneSsePlugin.AnswerCallAsync(); 
        }
    }
}