# PhoneToLinux Desktop - Dokumentacja API

Automatycznie wygenerowany spis klas, interfejsów i metod z kodu C#.

---

## 📦 Klasa / Interfejs: `phonetolinux.TabHighlightConverter`

Converts current selected tab index to active background highlight brush.

## 📦 Klasa / Interfejs: `phonetolinux.Models.ChatMessageItem`

Represents a single chat message within a conversation thread.

### `F:phonetolinux.Models.ChatMessageItem._text`

The actual text content of the message.

### `F:phonetolinux.Models.ChatMessageItem._isOutgoing`

Indicates whether the message was sent by the user (true) or received (false).

### 🔧 Właściwość: `phonetolinux.Models.ChatMessageItem.ServerBody`

Fallback property to handle Android's native SMS database column "body".
            Maps the incoming JSON "body" to the "Text" property if it wasn't provided directly.

### 🔧 Właściwość: `phonetolinux.Models.ChatMessageItem.ServerType`

Fallback property to handle Android's native SMS database column "type".
            Maps the incoming JSON "type" (1 = Received/Inbox, 2 = Sent/Outbox) to the "IsOutgoing" boolean flag.

### 🔧 Właściwość: `phonetolinux.Models.ChatMessageItem.Text`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.Models.ChatMessageItem.IsOutgoing`

Brak opisu.

## 📦 Klasa / Interfejs: `phonetolinux.Models.ChatConversationItem`

Represents a summarized conversation thread in the recent chats list (left panel).

### `F:phonetolinux.Models.ChatConversationItem._contactName`

Display name of the contact, or the raw phone number if the name is not in the address book.

### `F:phonetolinux.Models.ChatConversationItem._phoneNumber`

The phone number or alphanumeric sender ID associated with the conversation.

### `F:phonetolinux.Models.ChatConversationItem._lastMessage`

Preview snippet of the most recent message in the thread.

### 🔧 Właściwość: `phonetolinux.Models.ChatConversationItem.ContactName`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.Models.ChatConversationItem.PhoneNumber`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.Models.ChatConversationItem.LastMessage`

Brak opisu.

## 📦 Klasa / Interfejs: `phonetolinux.Models.ContactItem`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.Models.ContactItem.Name`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.Models.ContactItem.PhoneNumber`

Brak opisu.

## 📦 Klasa / Interfejs: `phonetolinux.Models.MmsAttachment`

Represents an MMS attachment (image, audio, or document) with its metadata.

## 📦 Klasa / Interfejs: `phonetolinux.Services.ChatHistoryPlugin`

Plugin responsible for fetching chat history from the phone server 
            and managing local persistence in JSON files.

### 🔧 Właściwość: `phonetolinux.Services.ChatHistoryPlugin.Endpoint`

Brak opisu.

### ⚙️ Metoda: `phonetolinux.Services.ChatHistoryPlugin.Execute(System.String)`

Executes the main plugin task for a given query parameter.

### ⚙️ Metoda: `phonetolinux.Services.ChatHistoryPlugin.FetchRemoteHistoryAsync(System.Net.Http.HttpClient,System.String)`

Asynchronously fetches chat history from the remote phone server via HTTP endpoint.

### ⚙️ Metoda: `phonetolinux.Services.ChatHistoryPlugin.LoadHistoryAsync(System.String)`

Asynchronously loads message history for the specified identifier from local storage.

### ⚙️ Metoda: `phonetolinux.Services.ChatHistoryPlugin.SaveHistoryAsync(System.String,System.Collections.Generic.IEnumerable{phonetolinux.Models.ChatMessageItem})`

Asynchronously saves message history for the specified identifier to a local JSON file.
            Deduplicates items before persisting.

## 📦 Klasa / Interfejs: `phonetolinux.Services.ContactsPlugin`

Plugin responsible for fetching the list of contacts from the mobile device
            via an HTTP request to the server, deserializing the JSON response, and deduplicating entries.

### ⚙️ Metoda: `phonetolinux.Services.ContactsPlugin.#ctor(System.Net.Http.HttpClient)`

Initializes a new instance of the

### 🔧 Właściwość: `phonetolinux.Services.ContactsPlugin.Endpoint`

Brak opisu.

### ⚙️ Metoda: `phonetolinux.Services.ContactsPlugin.Execute(System.String)`

Executes the plugin operation for a given query.

### ⚙️ Metoda: `phonetolinux.Services.ContactsPlugin.GetContactsAsync`

Asynchronously fetches the list of contacts from the phone server and deduplicates them by unique contact names.

### ⚙️ Metoda: `phonetolinux.Services.ContactsPlugin.NormalizePhoneNumber(System.String)`

Normalizes phone numbers to standard 9-digit format for comparison.

## 📦 Klasa / Interfejs: `phonetolinux.Services.ConversationDto`

DTO object representing data for a single conversation fetched from the server.
            Supports mapping for multiple field names (number/address) emitted by the Android server.

## 📦 Klasa / Interfejs: `phonetolinux.Services.ConversationsPlugin`

Plugin responsible for fetching and managing recent conversations from the mobile device
            via HTTP requests, deserializing JSON responses, and handling deletion commands.

### ⚙️ Metoda: `phonetolinux.Services.ConversationsPlugin.DeleteConversationAsync(System.String)`

Asynchronously sends an HTTP DELETE request to the Android server to remove an SMS thread by address.

## 📦 Klasa / Interfejs: `phonetolinux.Services.LinuxNotificationPlugin`

Plugin responsible for displaying native Linux desktop notifications (using notify-send)
            triggered by incoming phone events such as SMS messages or phone calls.

### 🔧 Właściwość: `phonetolinux.Services.LinuxNotificationPlugin.Endpoint`

Brak opisu.

### ⚙️ Metoda: `phonetolinux.Services.LinuxNotificationPlugin.Execute(System.String)`

Executes the plugin operation for a given query parameter.

### ⚙️ Metoda: `phonetolinux.Services.LinuxNotificationPlugin.ShowNotification(System.String,System.String,System.String,System.String)`

Displays a native desktop notification on the Linux system using notify-send.

### ⚙️ Metoda: `phonetolinux.Services.LinuxNotificationPlugin.SanitizeShellArgument(System.String)`

Sanitizes text strings to be safely passed as command-line arguments to shell utilities.

## 📦 Klasa / Interfejs: `phonetolinux.Services.PhoneCallPlugin`

Plugin responsible for managing phone calls (initiating, answering, and ending calls)
            via HTTP POST requests to the mobile device server.

### ⚙️ Metoda: `phonetolinux.Services.PhoneCallPlugin.#ctor(System.Net.Http.HttpClient)`

Initializes a new instance of the

### 🔧 Właściwość: `phonetolinux.Services.PhoneCallPlugin.Endpoint`

Brak opisu.

### ⚙️ Metoda: `phonetolinux.Services.PhoneCallPlugin.Execute(System.String)`

Executes the plugin action based on provided query parameters.

### ⚙️ Metoda: `phonetolinux.Services.PhoneCallPlugin.StartCallAsync(System.String)`

Asynchronously initiates a new phone call to the specified destination number.

### ⚙️ Metoda: `phonetolinux.Services.PhoneCallPlugin.EndCallAsync`

Asynchronously ends or rejects an active phone call.

### ⚙️ Metoda: `phonetolinux.Services.PhoneCallPlugin.AnswerCallAsync`

Asynchronously answers an incoming phone call.

## 📦 Klasa / Interfejs: `phonetolinux.Services.SmsHistoryPlugin`

Plugin responsible for fetching SMS conversation history for a specific phone number 
            from the mobile server via HTTP requests.

### ⚙️ Metoda: `phonetolinux.Services.SmsHistoryPlugin.#ctor(System.Net.Http.HttpClient)`

Initializes a new instance of the

### 🔧 Właściwość: `phonetolinux.Services.SmsHistoryPlugin.Endpoint`

Brak opisu.

### ⚙️ Metoda: `phonetolinux.Services.SmsHistoryPlugin.Execute(System.String)`

Executes the plugin operation for a given query parameter.

### ⚙️ Metoda: `phonetolinux.Services.SmsHistoryPlugin.GetChatHistoryFromServerAsync(System.String)`

Asynchronously fetches chat history from the phone server for the specified phone number
            and removes potential duplicate entries.

## 📦 Klasa / Interfejs: `phonetolinux.Services.SmsPlugin`

Plugin responsible for dispatching outgoing SMS messages via HTTP GET requests 
            to the mobile device's endpoint.

### ⚙️ Metoda: `phonetolinux.Services.SmsPlugin.#ctor(System.Net.Http.HttpClient)`

Initializes a new instance of the

### 🔧 Właściwość: `phonetolinux.Services.SmsPlugin.Endpoint`

Brak opisu.

### ⚙️ Metoda: `phonetolinux.Services.SmsPlugin.Execute(System.String)`

Executes the plugin operation for a given query parameter.

### ⚙️ Metoda: `phonetolinux.Services.SmsPlugin.SendSmsAsync(System.String,System.String)`

Asynchronously sends an SMS message to the specified destination number via HTTP GET.

## 📦 Klasa / Interfejs: `phonetolinux.Services.DnnPluginLoader`

Dynamic plugin loader supporting runtime Roslyn compilation for C# source files 
            and direct assembly loading for precompiled binaries.

### ⚙️ Metoda: `phonetolinux.Services.DnnPluginLoader.ExecutePlugin(System.String,System.String,System.String,System.Object)`

Compiles or loads a plugin file and executes the specified target method.

## 📦 Klasa / Interfejs: `phonetolinux.Services.PairingListenerService`

Lightweight HTTP Listener service handling incoming pairing requests from the Android device.
            Verifies the 6-digit PIN and 256-bit credentials, encrypts session data, and triggers UI state transition.

### ⚙️ Metoda: `phonetolinux.Services.PairingListenerService.StartListening(System.Int32)`

Starts asynchronously listening for incoming HTTP pairing requests on the specified port.

### ⚙️ Metoda: `phonetolinux.Services.PairingListenerService.ListenLoopAsync(System.Threading.CancellationToken)`

Main execution loop listening for inbound HTTP requests.

### ⚙️ Metoda: `phonetolinux.Services.PairingListenerService.ProcessPairingRequestAsync(System.Net.HttpListenerContext)`

Processes inbound pairing request payload, verifies the 6-digit PIN, and encrypts credentials.

### ⚙️ Metoda: `phonetolinux.Services.PairingListenerService.SendJsonResponseAsync(System.Net.HttpListenerResponse,System.Net.HttpStatusCode,System.String,System.String)`

Utility helper to format and send JSON HTTP responses.

### ⚙️ Metoda: `phonetolinux.Services.PairingListenerService.StopListening`

Stops the listener service and cleans up resources.

### 🔧 Właściwość: `phonetolinux.Plugins.MmsPlugin.Endpoint`

Brak opisu.

### ⚙️ Metoda: `phonetolinux.Plugins.MmsPlugin.#ctor(System.Net.Http.HttpClient)`

Initializes a new instance of the

### ⚙️ Metoda: `phonetolinux.Plugins.MmsPlugin.Execute(System.String)`

Execution method required by the

### ⚙️ Metoda: `phonetolinux.Plugins.MmsPlugin.DownloadAttachmentAsync(System.String,System.String,System.String)`

Asynchronously downloads an MMS attachment from the remote phone server.

## 📦 Klasa / Interfejs: `phonetolinux.Plugins.PhoneEventDto`

Data Transfer Object representing incoming events from the Android SSE stream.

## 📦 Klasa / Interfejs: `phonetolinux.Plugins.PhoneSsePlugin`

Plugin implementation responsible for maintaining the Server-Sent Events (SSE) stream 
            connection with the Android device and bridging calls and messages.

### ⚙️ Metoda: `phonetolinux.Plugins.PhoneSsePlugin.Initialize(System.String,System.Int32)`

Initializes the SSE background listener loop connecting to the phone.

### ⚙️ Metoda: `phonetolinux.Plugins.PhoneSsePlugin.ReadSseStreamAsync(System.String,System.Threading.CancellationToken)`

Continuously reads the SSE stream from the Android server with automatic reconnection handling.

### ⚙️ Metoda: `phonetolinux.Plugins.PhoneSsePlugin.DispatchEvent(phonetolinux.Plugins.PhoneEventDto)`

Dispatches incoming parsed events to their respective registered event handlers.

### ⚙️ Metoda: `phonetolinux.Plugins.PhoneSsePlugin.AnswerCallAsync`

Sends an HTTP POST command to the phone to answer the active incoming call.

### ⚙️ Metoda: `phonetolinux.Plugins.PhoneSsePlugin.RejectCallAsync`

Sends an HTTP POST command to the phone to reject the active incoming call.

### ⚙️ Metoda: `phonetolinux.Plugins.PhoneSsePlugin.Shutdown`

Shuts down the background listener and cancels active network tasks.

## 📦 Klasa / Interfejs: `phonetolinux.PluginSource.StoragePlugin`

Storage integration plugin implementation for desktop file manager mounting and bookmarks.
            Implements full IPhonetolinuxPlugin contract.

### ⚙️ Metoda: `phonetolinux.PluginSource.StoragePlugin.OnDeviceConnected(System.String)`

Triggered when phone connects/pairs with desktop application.
            Compatible with DnnPluginLoader single-string parameter signature.

### ⚙️ Metoda: `phonetolinux.PluginSource.StoragePlugin.OnDeviceDisconnected(System.String)`

Triggered when phone disconnects.

## 📦 Klasa / Interfejs: `phonetolinux.Security.AutochangeIP`

Authenticated IP discovery service protecting against IP injection/spoofing.

### ⚙️ Metoda: `phonetolinux.Security.AutochangeIP.DiscoverPhoneIpAsync(System.String,System.Int32,System.Threading.CancellationToken)`

Sends a signed UDP broadcast request to locate the Android device safely.

## 📦 Klasa / Interfejs: `phonetolinux.ViewModels.ChatViewModel`

ViewModel responsible for managing chat state, active conversation selection,
            loading message history, and real-time messaging updates.

### ⚙️ Metoda: `phonetolinux.ViewModels.ChatViewModel.LoadConversationsAndSyncAsync`

Asynchronously fetches recent conversations from the phone server or falls back to local storage.

### ⚙️ Metoda: `phonetolinux.ViewModels.ChatViewModel.InitializeChatAsync(phonetolinux.Models.ChatContext)`

Initializes active chat context and triggers message history loading.

### ⚙️ Metoda: `phonetolinux.ViewModels.ChatViewModel.SendMessage`

Sends a new SMS message and updates local history storage.

### ⚙️ Metoda: `phonetolinux.ViewModels.ChatViewModel.AddIncomingSms(phonetolinux.Models.ChatContext)`

Handles incoming real-time SMS pushes from background services.

### 🔧 Właściwość: `phonetolinux.ViewModels.ChatViewModel.PhoneNumber`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.ChatViewModel.ContactName`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.ChatViewModel.CurrentMessageText`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.ChatViewModel.MessagesList`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.ChatViewModel.RecentConversations`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.ChatViewModel.SelectedConversation`

Brak opisu.

### ⚙️ Metoda: `phonetolinux.ViewModels.ChatViewModel.OnSelectedConversationChanged(phonetolinux.Models.ChatConversationItem)`

Executes the logic for when

### `F:phonetolinux.ViewModels.ChatViewModel.sendMessageCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.ChatViewModel.SendMessageCommand`

Gets an

## 📦 Klasa / Interfejs: `phonetolinux.ViewModels.DialerViewModel`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.DialerViewModel.PhoneNumber`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.DialerViewModel.IsInCall`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.DialerViewModel.IsIncomingCall`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.DialerViewModel.ContactName`

Brak opisu.

### `F:phonetolinux.ViewModels.DialerViewModel.appendNumberCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.DialerViewModel.AppendNumberCommand`

Gets an

### `F:phonetolinux.ViewModels.DialerViewModel.backspaceCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.DialerViewModel.BackspaceCommand`

Gets an

### `F:phonetolinux.ViewModels.DialerViewModel.callCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.DialerViewModel.CallCommand`

Gets an

### `F:phonetolinux.ViewModels.DialerViewModel.endCallCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.DialerViewModel.EndCallCommand`

Gets an

### `F:phonetolinux.ViewModels.DialerViewModel.answerCallCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.DialerViewModel.AnswerCallCommand`

Gets an

## 📦 Klasa / Interfejs: `phonetolinux.ViewModels.MainViewModel`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.CurrentVersion`

Reads the current application version directly from the executing assembly.

### ⚙️ Metoda: `phonetolinux.ViewModels.MainViewModel.LoadMessagesForNumberAsync(System.String)`

Highly resilient JSON message parser with exhaustive error logging.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.IsPaired`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.StorageViewModel`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.SelectedTabIndex`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.Pairing`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.Dialer`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.ActiveChat`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.CurrentTheme`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.SelectedConversation`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.IsInCall`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.IsIncomingCall`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.ContactName`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.PhoneNumber`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.CurrentMessageText`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.UpdateStatusMessage`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.IsCheckingForUpdates`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.ContactsList`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.RecentConversations`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.MessagesList`

Brak opisu.

### `F:phonetolinux.ViewModels.MainViewModel.checkForUpdatesCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.CheckForUpdatesCommand`

Gets an

### `F:phonetolinux.ViewModels.MainViewModel.selectTabCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.SelectTabCommand`

Gets an

### `F:phonetolinux.ViewModels.MainViewModel.sendMessageToContactCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.SendMessageToContactCommand`

Gets an

### `F:phonetolinux.ViewModels.MainViewModel.callSpecificNumberCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.CallSpecificNumberCommand`

Gets an

### `F:phonetolinux.ViewModels.MainViewModel.sendMessageCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.SendMessageCommand`

Gets an

### `F:phonetolinux.ViewModels.MainViewModel.selectConversationCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.SelectConversationCommand`

Gets an

### `F:phonetolinux.ViewModels.MainViewModel.deleteConversationCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.DeleteConversationCommand`

Gets an

### `F:phonetolinux.ViewModels.MainViewModel.pairingCompletedCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.PairingCompletedCommand`

Gets an

### `F:phonetolinux.ViewModels.MainViewModel.unpairDeviceCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.UnpairDeviceCommand`

Gets an

### `F:phonetolinux.ViewModels.MainViewModel.appendNumberCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.AppendNumberCommand`

Gets an

### `F:phonetolinux.ViewModels.MainViewModel.backspaceCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.BackspaceCommand`

Gets an

### `F:phonetolinux.ViewModels.MainViewModel.callCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.CallCommand`

Gets an

### `F:phonetolinux.ViewModels.MainViewModel.endCallCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.EndCallCommand`

Gets an

### `F:phonetolinux.ViewModels.MainViewModel.answerCallCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.MainViewModel.AnswerCallCommand`

Gets an

## 📦 Klasa / Interfejs: `phonetolinux.ViewModels.PairingViewModel`

ViewModel responsible for managing initial device handshake and PIN generation logic.

### ⚙️ Metoda: `phonetolinux.ViewModels.PairingViewModel.GeneratePairingPinCode`

Generates a fresh 6-digit PIN code and calculates the active network endpoint.

### ⚙️ Metoda: `phonetolinux.ViewModels.PairingViewModel.GetActiveLocalIpAddress`

Resolves the primary local IPv4 address of the host machine.

### 🔧 Właściwość: `phonetolinux.ViewModels.PairingViewModel.PairingPin`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.PairingViewModel.StatusMessage`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.PairingViewModel.IpAddress`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.PairingViewModel.CanGeneratePin`

Brak opisu.

### `F:phonetolinux.ViewModels.PairingViewModel.generatePairingPinCodeCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.PairingViewModel.GeneratePairingPinCodeCommand`

Gets an

## 📦 Klasa / Interfejs: `phonetolinux.ViewModels.StorageBrowserViewModel`

Brak opisu.

### ⚙️ Metoda: `phonetolinux.ViewModels.StorageBrowserViewModel.GetDeviceIpAsync`

Automatically retrieves the active device IP address. 
            Checks the cached session variable first or prompts the user.

### ⚙️ Metoda: `phonetolinux.ViewModels.StorageBrowserViewModel.PromptForIpAddressAsync`

Helper method to display a lightweight dark-themed input dialog for manual IP entry.

### ⚙️ Metoda: `phonetolinux.ViewModels.StorageBrowserViewModel.LoadFilesAsync(System.String)`

Asynchronously fetches files and directories via WebDAV PROPFIND (Port 5001).

### ⚙️ Metoda: `phonetolinux.ViewModels.StorageBrowserViewModel.DownloadFileAsync`

Downloads selected file using WebDAV GET (Port 5001).

### ⚙️ Metoda: `phonetolinux.ViewModels.StorageBrowserViewModel.UploadFileAsync`

Uploads a local file using WebDAV PUT (Port 5001).

### ⚙️ Metoda: `phonetolinux.ViewModels.StorageBrowserViewModel.NavigateUp`

Navigates one directory level up in the hierarchy.

### ⚙️ Metoda: `phonetolinux.ViewModels.StorageBrowserViewModel.Refresh`

Refreshes the contents of the current directory.

### 🔧 Właściwość: `phonetolinux.ViewModels.StorageBrowserViewModel.CurrentPath`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.StorageBrowserViewModel.Files`

Brak opisu.

### 🔧 Właściwość: `phonetolinux.ViewModels.StorageBrowserViewModel.SelectedFile`

Brak opisu.

### ⚙️ Metoda: `phonetolinux.ViewModels.StorageBrowserViewModel.OnSelectedFileChanged(phonetolinux.Models.PhoneFileItem)`

Handles item selection. Navigates into selected directories automatically.

### `F:phonetolinux.ViewModels.StorageBrowserViewModel.loadFilesCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.StorageBrowserViewModel.LoadFilesCommand`

Gets an

### `F:phonetolinux.ViewModels.StorageBrowserViewModel.downloadFileCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.StorageBrowserViewModel.DownloadFileCommand`

Gets an

### `F:phonetolinux.ViewModels.StorageBrowserViewModel.uploadFileCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.StorageBrowserViewModel.UploadFileCommand`

Gets an

### `F:phonetolinux.ViewModels.StorageBrowserViewModel.navigateUpCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.StorageBrowserViewModel.NavigateUpCommand`

Gets an

### `F:phonetolinux.ViewModels.StorageBrowserViewModel.refreshCommand`

The backing field for

### 🔧 Właściwość: `phonetolinux.ViewModels.StorageBrowserViewModel.RefreshCommand`

Gets an

### ⚙️ Metoda: `phonetolinux.Views.IpConfigWindow.InitializeComponent(System.Boolean)`

Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

## 📦 Klasa / Interfejs: `phonetolinux.Views.MainWindow`

Code-behind logic for the main application window.

### ⚙️ Metoda: `phonetolinux.Views.MainWindow.TitleBar_PointerPressed(System.Object,Avalonia.Input.PointerPressedEventArgs)`

Handles drag-and-drop window movement using the custom title bar.

### ⚙️ Metoda: `phonetolinux.Views.MainWindow.Minimize_Click(System.Object,Avalonia.Interactivity.RoutedEventArgs)`

Minimizes the application window.

### ⚙️ Metoda: `phonetolinux.Views.MainWindow.Maximize_Click(System.Object,Avalonia.Interactivity.RoutedEventArgs)`

Toggles between maximized and normal window state.

### ⚙️ Metoda: `phonetolinux.Views.MainWindow.Close_Click(System.Object,Avalonia.Interactivity.RoutedEventArgs)`

Closes the application.

### ⚙️ Metoda: `phonetolinux.Views.MainWindow.Window_KeyDown(System.Object,Avalonia.Input.KeyEventArgs)`

Triggers the phone call command when the Enter or Return key is pressed while on the Dialer tab.

### ⚙️ Metoda: `phonetolinux.Views.MainWindow.InitializeComponent(System.Boolean)`

Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

### ⚙️ Metoda: `phonetolinux.Views.PairingView.InitializeComponent(System.Boolean)`

Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

### ⚙️ Metoda: `phonetolinux.Views.SettingsView.InitializeComponent(System.Boolean)`

Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

### ⚙️ Metoda: `phonetolinux.Views.StorageBrowserView.InitializeComponent(System.Boolean)`

Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

## 📦 Klasa / Interfejs: `PhoneToLinux.Compiler.CompilerArchiveModifier`

Narzędzie kompilatora odpowiedzialne za automatyczne przenoszenie skompilowanych 
            bibliotek wtyczek (.dll) do głównego folderu Library w projekcie.

### ⚙️ Metoda: `PhoneToLinux.Compiler.CompilerArchiveModifier.StartSilentWatcher(System.String,System.String)`

Uruchamia monitorowanie folderu wyjściowego kompilatora w tle. 
            Gdy pojawia się nowa biblioteka .dll, automatycznie kopiuje ją do folderu Library.

### ⚙️ Metoda: `PhoneToLinux.Compiler.CompilerArchiveModifier.CopyDllToLibrary(System.String,System.String)`

Kopiuje skompilowaną bibliotekę DLL bezpośrednio do folderu Library.

## 📦 Klasa / Interfejs: `PhoneToLinux.Core.IPhonePlugin`

Reprezentuje wspólny interfejs dla wszystkich wtyczek (.dnn) po stronie desktopowej.
            Każda nowa funkcja ładowana dynamicznie musi implementować ten kontrakt.

### 🔧 Właściwość: `PhoneToLinux.Core.IPhonePlugin.Endpoint`

Ścieżka endpointu obsługiwana przez wtyczkę (np. "/conversations").

### ⚙️ Metoda: `PhoneToLinux.Core.IPhonePlugin.Execute(System.String)`

Wykonuje główną logikę wtyczki na podstawie przekazanych parametrów żądania.

## 📦 Klasa / Interfejs: `PhoneToLinux.Plugins.PhoneConfigPlugin`

Wtyczka odpowiedzialna za zarządzanie konfiguracją połączenia z telefonem (adres IP i port),
            w tym odczyt i zapis stanu do pliku konfiguracyjnego w katalogu użytkownika.

### `F:PhoneToLinux.Plugins.PhoneConfigPlugin.ConfigDir`

Ścieżka do katalogu konfiguracyjnego w profilu użytkownika.

### `F:PhoneToLinux.Plugins.PhoneConfigPlugin.ConfigFilePath`

Pełna ścieżka do pliku konfiguracyjnego config.json.

### `F:PhoneToLinux.Plugins.PhoneConfigPlugin.Port`

Numer portu nasłuchiwania serwera na telefonie.

### 🔧 Właściwość: `PhoneToLinux.Plugins.PhoneConfigPlugin.PhoneIp`

Aktualny adres IP telefonu.

### 🔧 Właściwość: `PhoneToLinux.Plugins.PhoneConfigPlugin.Endpoint`

Brak opisu.

## 📦 Klasa / Interfejs: `PhoneToLinux.Plugins.PhoneConfigPlugin.ConfigModel`

Model danych dla deserializacji i serializacji konfiguracji JSON.

### ⚙️ Metoda: `PhoneToLinux.Plugins.PhoneConfigPlugin.Execute(System.String)`

Wykonuje operacje wtyczki w zależności od przekazanych parametrów (np. pobranie lub ustawienie IP).

### ⚙️ Metoda: `PhoneToLinux.Plugins.PhoneConfigPlugin.GetBaseUrl`

Zwraca pełny adres bazowy URL do komunikacji z serwerem na telefonie.

### ⚙️ Metoda: `PhoneToLinux.Plugins.PhoneConfigPlugin.LoadSavedIp`

Łagodzi i wczytuje zapisany adres IP z pliku konfiguracyjnego na dysku.

### ⚙️ Metoda: `PhoneToLinux.Plugins.PhoneConfigPlugin.SaveIp(System.String)`

Zapisuje nowy adres IP telefonu do pliku konfiguracyjnego JSON.

## 📦 Klasa / Interfejs: `PhoneToLinux.Plugins.ChatSyncPlugin`

Plugin responsible for selecting, filtering, and synchronizing 
            active conversations in the chat list.

### ⚙️ Metoda: `PhoneToLinux.Plugins.ChatSyncPlugin.GetDefaultOrFirstConversation(phonetolinux.Models.ChatContext)`

Retrieves the default or first available conversation from the context,
            ensuring thread deduplication.

### ⚙️ Metoda: `PhoneToLinux.Plugins.ChatSyncPlugin.DeduplicateConversations(System.Collections.Generic.IEnumerable{phonetolinux.Models.ChatConversationItem})`

Filters a collection of conversation items by unique phone number.

### ⚙️ Metoda: `PhoneToLinux.Plugins.ChatSyncPlugin.NormalizePhoneNumber(System.String)`

Normalizes phone numbers to standard 9-digit format for robust comparison.

## 📦 Klasa / Interfejs: `PhoneToLinux.Desktop.PluginManager`

Scans the specified directory, dynamically loads plugin libraries (.dll),
            and routes incoming requests to the appropriate plugin handlers.

### ⚙️ Metoda: `PhoneToLinux.Desktop.PluginManager.LoadPlugins(System.String)`

Searches the specified library directory for plugin files with the .dll extension
            and registers them in application memory.
            Ignores subdirectories to prevent interference with secure storage paths.

### ⚙️ Metoda: `PhoneToLinux.Desktop.PluginManager.ExecutePlugin(System.String,System.String)`

Handles incoming queries by routing them to the corresponding plugin based on the endpoint.

## 📦 Klasa / Interfejs: `PhoneToLinux.Security.DevicePairingService`

Service responsible for establishing a secure pairing mechanism between Desktop and Android.
            Generates secure 6-digit PIN payloads and derives a unique 256-bit AES master key.
            Includes graceful fallbacks for systems without accessible physical MAC addresses.

### ⚙️ Metoda: `PhoneToLinux.Security.DevicePairingService.GetDesktopMacAddress`

Retrieves the MAC address of the first operational network interface on the Linux desktop.
            Falls back to a deterministic machine key if no active physical interface is reported.

### ⚙️ Metoda: `PhoneToLinux.Security.DevicePairingService.GeneratePairingPin`

Generates a cryptographically secure 6-digit pairing PIN formatted as "xxx xxx".

### ⚙️ Metoda: `PhoneToLinux.Security.DevicePairingService.GeneratePairingPayload(System.String,System.Int32,System.String)`

Generates the connection string payload containing IP, port, MAC, and the secure pairing PIN.

### ⚙️ Metoda: `PhoneToLinux.Security.DevicePairingService.DeriveAesKey(System.String,System.String)`

Derives a 256-bit (32-byte) AES key by combining the desktop MAC, Android MAC, and pairing PIN.
            Uses SHA-256 to ensure the resulting key is exactly 256 bits long.

## 📦 Klasa / Interfejs: `PhoneToLinux.Security.DynamicFolderManager`

Implements Moving Target Defense pattern by periodically changing 
            the physical directory location of encrypted local data.

### ⚙️ Metoda: `PhoneToLinux.Security.DynamicFolderManager.StartPeriodicRelocation(System.TimeSpan)`

Starts the background timer to periodically relocate data files.

## 📦 Klasa / Interfejs: `PhoneToLinux.Security.SecureStorageService`

Service responsible for encrypting and decrypting local sensitive data using AES-256.

### ⚙️ Metoda: `PhoneToLinux.Security.SecureStorageService.EncryptAndWrite(System.String,System.String)`

Encrypts plain text data and writes it to the specified file path along with the generated Initialization Vector (IV).

### ⚙️ Metoda: `PhoneToLinux.Security.SecureStorageService.ReadAndDecrypt(System.String)`

Reads an encrypted file, extracts the IV, and decrypts the content back to plain text.

## 📦 Klasa / Interfejs: `CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs`

A helper type providing cached, reusable

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.Text`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.IsOutgoing`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.ContactName`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.PhoneNumber`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.LastMessage`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.Name`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.CurrentMessageText`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.MessagesList`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.RecentConversations`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.SelectedConversation`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.IsInCall`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.IsIncomingCall`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.IsPaired`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.StorageViewModel`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.SelectedTabIndex`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.Pairing`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.Dialer`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.ActiveChat`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.CurrentTheme`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.UpdateStatusMessage`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.IsCheckingForUpdates`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.ContactsList`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.PairingPin`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.StatusMessage`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.IpAddress`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.CanGeneratePin`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.CurrentPath`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.Files`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangingArgs.SelectedFile`

The cached

## 📦 Klasa / Interfejs: `CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs`

A helper type providing cached, reusable

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.Text`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.IsOutgoing`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.ContactName`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.PhoneNumber`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.LastMessage`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.Name`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.CurrentMessageText`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.MessagesList`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.RecentConversations`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.SelectedConversation`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.IsInCall`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.IsIncomingCall`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.IsPaired`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.StorageViewModel`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.SelectedTabIndex`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.Pairing`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.Dialer`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.ActiveChat`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.CurrentTheme`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.UpdateStatusMessage`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.IsCheckingForUpdates`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.ContactsList`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.PairingPin`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.StatusMessage`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.IpAddress`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.CanGeneratePin`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.CurrentPath`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.Files`

The cached

### `F:CommunityToolkit.Mvvm.ComponentModel.__Internals.__KnownINotifyPropertyChangedArgs.SelectedFile`

The cached
