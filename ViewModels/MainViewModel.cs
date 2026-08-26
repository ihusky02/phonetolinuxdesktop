using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using phonetolinux.Services;
using phonetolinux.Models;
using PhoneToLinux.Security;

namespace phonetolinux.ViewModels
{
    /// <summary>
    /// Main application ViewModel responsible for coordinating top-level UI states,
    /// active views (dialer, chat, contacts, pairing), navigation tabs, call overlays, and plugin integration.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private static readonly byte[] MasterKey = SHA256.HashData(Encoding.UTF8.GetBytes("PhoneToLinux_MasterKey2026_Salt"));
        private static readonly HttpClient SharedHttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        private readonly SecureStorageService _storageService;
        private readonly ContactsPlugin _contactsPlugin;
        private readonly SmsPlugin _smsPlugin;
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

        // Call overlay and state management properties
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

        // UI collections linked with compiled DLL plugins / HTTP responses
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
            
            // Resolve application storage directory in user profile (.local/share/phonetolinux)
            _storageDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "phonetolinux"
            );

            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }

            _pairing = new PairingViewModel();

            // Evaluate stored credentials on application launch
            CheckPairingStatus();
        }

        /// <summary>
        /// Inspects the secure storage directory to determine if the device has already been paired
        /// and securely restores the phone IP configuration using AES-256 decryption.
        /// </summary>
        public void CheckPairingStatus()
        {
            string pairedFilePath = Path.Combine(_storageDirectory, "paired_device.dat");
            IsPaired = File.Exists(pairedFilePath);

            if (IsPaired)
            {
                SelectedTabIndex = 0; // Default to Dialer tab

                try
                {
                    // Securely read and decrypt the paired device data using the master key
                    string decryptedPayload = _storageService.ReadAndDecrypt(pairedFilePath);
                    
                    var jsonDoc = JsonDocument.Parse(decryptedPayload);
                    if (jsonDoc.RootElement.TryGetProperty("phoneIp", out var ipProp))
                    {
                        string savedIp = ipProp.GetString();
                        if (!string.IsNullOrEmpty(savedIp))
                        {
                            PhoneConfig.SaveIp(savedIp);
                        }
                    }
                }
                catch
                {
                    // Fallback silently if decryption fails or file integrity is compromised
                }
            }
            else
            {
                SelectedTabIndex = 3; // Fallback index for initial setup

                // Start HTTP listener service for initial pairing setup
                _pairingListener = new PairingListenerService(this);
                _pairingListener.StartListening(5000);
            }
        }

        /// <summary>
        /// Handles tab switching from the navigation rail, supporting string, int, and object parameters.
        /// Triggers asynchronous plugin queries upon navigating to specific tabs.
        /// </summary>
        [RelayCommand]
        public void SelectTab(object? parameter)
        {
            if (parameter != null && int.TryParse(parameter.ToString(), out int index))
            {
                SelectedTabIndex = index;

                // Load data from connected mobile device when selecting Contacts (1) or Chat (2)
                if (index == 1)
                {
                    _ = LoadContactsAsync();
                }
                else if (index == 2)
                {
                    _ = LoadConversationsAsync();
                }
            }
        }

        /// <summary>
        /// Invokes ContactsPlugin to fetch deduplicated contacts or applies fallback mockup data.
        /// </summary>
        public async Task LoadContactsAsync()
        {
            try
            {
                var fetchedContacts = await _contactsPlugin.GetContactsAsync();

                if (fetchedContacts != null && fetchedContacts.Count > 0)
                {
                    ContactsList.Clear();
                    foreach (var contact in fetchedContacts)
                    {
                        ContactsList.Add(contact);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTACTS ERROR] Failed to fetch contacts: {ex.Message}");
            }

            // Fallback mockup entries if network response is empty
            if (ContactsList.Count == 0)
            {
                ContactsList.Clear();
                ContactsList.Add(new ContactItem { Name = "Anna Kowalska", PhoneNumber = "+48 500 100 200" });
                ContactsList.Add(new ContactItem { Name = "Jan Nowak", PhoneNumber = "+48 600 300 400" });
                ContactsList.Add(new ContactItem { Name = "Support Service", PhoneNumber = "+48 700 800 900" });
            }
        }

        /// <summary>
        /// Invokes HTTP endpoint to fetch conversations or applies fallback mockup data.
        /// </summary>
        public async Task LoadConversationsAsync()
        {
            try
            {
                string url = $"{PhoneConfig.GetBaseUrl()}/conversations";
                HttpResponseMessage response = await SharedHttpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var fetchedThreads = JsonSerializer.Deserialize<List<ChatConversationItem>>(json, options);

                    if (fetchedThreads != null && fetchedThreads.Count > 0)
                    {
                        RecentConversations.Clear();
                        foreach (var thread in fetchedThreads)
                        {
                            RecentConversations.Add(thread);
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONVERSATIONS ERROR] HTTP Request failed: {ex.Message}");
            }

            // Fallback mockup entries matching specification
            if (RecentConversations.Count == 0)
            {
                RecentConversations.Clear();
                RecentConversations.Add(new ChatConversationItem { ContactName = "Anna Kowalska", LastMessage = "Hi, file attached!", PhoneNumber = "+48 500 100 200" });
                RecentConversations.Add(new ChatConversationItem { ContactName = "Jan Nowak", LastMessage = "Thanks for Linux tips", PhoneNumber = "+48 600 300 400" });

                MessagesList.Clear();
                MessagesList.Add(new ChatMessageItem { Text = "Hello! How is the Phonetolinux UI progressing?", IsOutgoing = false });
                MessagesList.Add(new ChatMessageItem { Text = "Working on the V3.3 navigation layout!", IsOutgoing = true });
                MessagesList.Add(new ChatMessageItem { Text = "Awesome, sending docs over.", IsOutgoing = false });
            }
        }

        /// <summary>
        /// Command to initiate a chat session with a specific contact from the Contacts tab.
        /// </summary>
        [RelayCommand]
        public void SendMessageToContact(string phoneNumber)
        {
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                SelectedTabIndex = 2; // Switch to Chat tab
                PhoneNumber = phoneNumber;
            }
        }

        /// <summary>
        /// Command to initiate a direct call to a specific phone number from the Contacts tab.
        /// </summary>
        [RelayCommand]
        public void CallSpecificNumber(string phoneNumber)
        {
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                PhoneNumber = phoneNumber;
                IsInCall = true;
                IsIncomingCall = false;
                ContactName = phoneNumber;
            }
        }

        /// <summary>
        /// Dispatches message entered in the chat input box via SmsPlugin to the mobile device.
        /// </summary>
        [RelayCommand]
        public async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(CurrentMessageText)) return;

            string targetNumber = PhoneNumber;
            if (string.IsNullOrEmpty(targetNumber) && ActiveChat != null)
            {
                targetNumber = ActiveChat.PhoneNumber;
            }

            if (string.IsNullOrEmpty(targetNumber))
            {
                Console.WriteLine("[SMS ERROR] Cannot send message: No target phone number specified.");
                return;
            }

            string textToSend = CurrentMessageText;
            Console.WriteLine($"[SMS] Attempting to send message to {targetNumber}: '{textToSend}'");

            bool success = await _smsPlugin.SendSmsAsync(targetNumber, textToSend);

            if (success)
            {
                Console.WriteLine("[SMS] Message successfully sent by phone!");
                MessagesList.Add(new ChatMessageItem { Text = textToSend, IsOutgoing = true });
                CurrentMessageText = "";
            }
            else
            {
                Console.WriteLine("[SMS ERROR] Failed to dispatch SMS via phone endpoint.");
            }
        }

        /// <summary>
        /// Selects chat conversation thread from the list.
        /// </summary>
        [RelayCommand]
        public void SelectConversation(ChatConversationItem conversation)
        {
            if (conversation != null)
            {
                ContactName = conversation.ContactName;
                PhoneNumber = conversation.PhoneNumber ?? "";
            }
        }

        /// <summary>
        /// Invoked when the PIN handshake completes successfully from the mobile app.
        /// </summary>
        [RelayCommand]
        public void OnPairingCompleted()
        {
            IsPaired = true;
            SelectedTabIndex = 0; // Automatically switch to the Dialer workspace
        }

        /// <summary>
        /// Clears all stored encrypted credentials and reverts the UI to the initial PIN pairing screen.
        /// </summary>
        [RelayCommand]
        public void UnpairDevice()
        {
            string pairedFilePath = Path.Combine(_storageDirectory, "paired_device.dat");
            string sessionFilePath = Path.Combine(_storageDirectory, "session_keys.dat");
            string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".phonetolinux", "config.json");

            if (File.Exists(pairedFilePath)) File.Delete(pairedFilePath);
            if (File.Exists(sessionFilePath)) File.Delete(sessionFilePath);
            if (File.Exists(configFilePath)) File.Delete(configFilePath);

            IsPaired = false;
            SelectedTabIndex = 3;
            Pairing.GeneratePairingPinCode();

            _pairingListener?.StopListening();
            _pairingListener = new PairingListenerService(this);
            _pairingListener.StartListening(5000);
        }

        /// <summary>
        /// Command triggered to append a digit or symbol to the current phone number string.
        /// </summary>
        [RelayCommand]
        public void AppendNumber(string number)
        {
            PhoneNumber += number;
        }

        /// <summary>
        /// Command triggered to remove the last character from the current phone number string.
        /// </summary>
        [RelayCommand]
        public void Backspace()
        {
            if (!string.IsNullOrEmpty(PhoneNumber))
            {
                PhoneNumber = PhoneNumber.Substring(0, PhoneNumber.Length - 1);
            }
        }

        /// <summary>
        /// Command triggered to initiate an outgoing call using the dialed number.
        /// </summary>
        [RelayCommand]
        public void Call()
        {
            if (!string.IsNullOrEmpty(PhoneNumber))
            {
                IsInCall = true;
                IsIncomingCall = false;
                ContactName = "Dialing...";
            }
        }

        /// <summary>
        /// Command triggered to end an active phone call.
        /// </summary>
        [RelayCommand]
        public void EndCall()
        {
            IsInCall = false;
            IsIncomingCall = false;
        }

        /// <summary>
        /// Command triggered to answer an incoming phone call.
        /// </summary>
        [RelayCommand]
        public void AnswerCall()
        {
            IsIncomingCall = false;
        }
    }
}