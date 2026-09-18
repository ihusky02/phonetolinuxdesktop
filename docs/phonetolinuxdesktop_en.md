# Technical Documentation: PhoneToLinux Desktop

**Version:** 1.1.5.3  
**Target Framework:** .NET 8.0 (`net8.0`) / Linux (x64)  
**UI Framework:** Avalonia UI 11  
**Project File:** `phonetolinux.csproj`  
**Repository / Author:** ihusky02 / PhoneToLinux Desktop  

---

## 1. Introduction and Project Objective

**PhoneToLinux Desktop** is an open-source desktop application designed for Linux environments, enabling two-way integration between a Linux computer and an Android smartphone over a local network. The application eliminates the need to reach for the mobile phone while working on the desktop by providing:

- **Placing and Answering Phone Calls** in real time directly from the built-in dialer and call overlay notifications.
- **Sending and Receiving SMS/MMS Messages** with live thread synchronization and chat history.
- **Address Book / Contact Synchronization** with automatic phone number normalization and contact name resolution.
- **Native Linux Desktop Notifications** via the system notification daemon (`notify-send` / libnotify).
- **Remote Storage & File Management (WebDAV)** allowing browsing, downloading, and uploading files to/from the mobile device via Wi-Fi.
- **Secure Device Pairing** using a 6-digit PIN code and AES-256 local encrypted storage.
- **Dynamic Roslyn Plugin Engine (DNN Engine)** that compiles C# scripts on-the-fly directly into application memory.
- **Automated Update Pipeline** downloading `.deb` installation packages directly from GitHub releases.

---

## 2. Architecture and Design Patterns

The application is built on the **MVVM (Model-View-ViewModel)** architectural pattern with modular separation of concerns and an asynchronous, non-blocking event-driven communication model.

### 2.1. Technology Stack

| Category | Technology / Library | Version | Purpose in Project |
| :--- | :--- | :--- | :--- |
| **Runtime** | .NET SDK / CLR | `8.0` | Execution platform |
| **Language** | C# | `12.0` | Core logic and UI code-behind |
| **UI Framework** | Avalonia UI | `11.2.3` | Cross-platform desktop user interface |
| **Theme & Typography** | Fluent Theme, Inter Font | `11.2.3` | Modern Fluent-styled dark/light UI |
| **MVVM Framework** | CommunityToolkit.Mvvm | `8.4.2` | Source generators, `[ObservableProperty]`, `[RelayCommand]` |
| **Runtime Compilation** | Microsoft.CodeAnalysis.CSharp | `5.9.0` | Roslyn compiler for dynamic plugin scripts (.cs) |
| **WebDAV Client** | WebDav.Client | `2.9.0` | Remote file system operations (Port 5001) |
| **Cryptography** | System.Security.Cryptography | .NET 8 BCL | AES-256-CBC encryption, SHA-256 key derivation, RandomNumberGenerator |

---

## 3. Project Structure and Code Organization

Directory layout and source file descriptions:

- `App.axaml / App.axaml.cs` – Application entry point and lifecycle manager. Launches the initial setup wizard if the phone IP is unconfigured, or triggers IP Auto-Discovery and opens the main window.
- `Program.cs` – Avalonia platform configuration and `Main()` bootstrap.
- `ViewLocator.cs` – Maps ViewModels to their corresponding Avalonia XAML Views.
- `Converters/` – Value converters for XAML bindings:
  - `TabHighlightConverter.cs` – Highlights the active sidebar navigation item based on tab index.
- `Models/` – Data transfer objects and domain models:
  - `ChatContext.cs` – Encapsulates context for active chat threads.
  - `ChatModels.cs` – `ChatMessageItem` (single SMS/MMS item), `ChatConversationItem` (conversation summary).
  - `ContactItem.cs` – Phone contact entity (Name, Phone Number).
  - `MmsAttachment.cs` – Metadata and binary payload definition for MMS attachments.
  - `PhoneFileItem.cs` – File/directory representation for the WebDAV browser.
- `ViewModels/` – Presentation and business logic layer:
  - `MainViewModel.cs` – Central orchestrator managing active tabs, incoming call overlays, SSE events, and notifications.
  - `ChatViewModel.cs` – Handles conversation selection, SMS dispatch, and thread loading.
  - `DialerViewModel.cs` – Manages keypad inputs, outgoing calls, and call state.
  - `PairingViewModel.cs` – Generates cryptographically secure PIN codes, resolves local host IP, and manages pairing state.
  - `StorageBrowserViewModel.cs` – WebDAV operations (file listing, uploading, downloading, folder navigation).
  - `SettingsViewModel.cs` – Theme selection and update checker actions.
- `Views/` – Avalonia UI XAML Views:
  - `MainWindow.axaml` – Main application window containing sidebar navigation, dialer, chat, contacts, and active call overlays.
  - `PairingView.axaml` – Device pairing screen displaying PIN code and connection status.
  - `StorageBrowserView.axaml` – Remote phone storage file explorer interface.
  - `SettingsView.axaml` – Application settings and update management view.
  - `IpConfigWindow.axaml` – Standalone dialog for initial IP entry.
- `Services/` – Shared application services:
  - `PhoneConfig.cs` – Manages persistent configuration (IP and session secret) in `~/.phonetolinux`.
  - `PairingListenerService.cs` – Embedded HTTP server listening on TCP port 5000 for pairing handshakes from Android.
  - `UpdateService.cs` – GitHub version checker and `.deb` package installer via `pkexec`.
  - `DnnPluginLoader.cs` – Roslyn-based dynamic C# script compilation and assembly execution.
- `Security/` – Cryptography and data protection:
  - `DevicePairingService.cs` – 6-digit PIN generator, Linux MAC address resolution, and 256-bit AES key derivation via SHA-256.
  - `SecureStorageService.cs` – AES-256-CBC encryption/decryption with random 16-byte Initialization Vectors (IV).
  - `AutochangeIP.cs` – Signed UDP broadcast discovery on port 5002 to protect against IP spoofing.
  - `DynamicFolderManager.cs` – Moving Target Defense implementation (rotates local encrypted storage directories).
- `PluginSource/` – Communication plugins (`IPhonetolinuxPlugin`):
  - `PhoneSsePlugin.cs` – Server-Sent Events client for real-time calls and SMS events.
  - `PhoneCallPlugin.cs` – REST API integration for telephony actions (`/call`, `/endcall`, `/answer`).
  - `SmsPlugin.cs` / `SmsHistoryPlugin.cs` – REST API integration for sending and retrieving SMS messages.
  - `ContactsPlugin.cs` – REST API integration for fetching and deduplicating contacts.
  - `ConversationsPlugin.cs` – REST API integration for managing and deleting SMS threads.
  - `StoragePlugin.cs` – WebDAV integration for phone storage operations.
  - `LinuxNotificationPlugin.cs` – Shell wrapper for desktop notifications via `notify-send`.

---

## 4. Communication Protocols and Network Channels

Communication between the Linux desktop application and the Android server takes place across 4 distinct network channels over the local network (Wi-Fi or USB Tethering):

```
+---------------------------------------------------------------------------------------+
|                                    NETWORK CHANNELS                                   |
+-------------------+---------+---------------------------------------------------------+
| Port              | Type    | Purpose                                                 |
+-------------------+---------+---------------------------------------------------------+
| TCP 5000 (Phone)  | HTTP/SSE| REST API (calls, SMS, contacts) + SSE Event Stream      |
| TCP 5000 (Desktop)| HTTP    | Desktop pairing handshake listener (PairingListener)    |
| TCP 5001 (Phone)  | WebDAV  | Remote file storage access (Upload / Download)          |
| UDP 5002 (Phone)  | UDP     | Signed UDP broadcast listener (Auto-discovery)          |
+-------------------+---------+---------------------------------------------------------+
```

### 4.1. Server-Sent Events (SSE) – `PhoneSsePlugin`
- **Endpoint:** `GET http://{phoneIp}:5000/events`
- **Content-Type:** `text/event-stream`
- **Payload Format:** JSON objects (`PhoneEventDto`) containing fields: `event`, `number`, `sender`, `message`.
- **Supported Events:**
  - `incoming_call`: Signals an incoming voice call; triggers a `critical` priority desktop notification via `notify-send` and displays the full-screen call overlay in `MainWindow.axaml`.
  - `call_ended`: Resets call state across the UI and Dialer.
  - `incoming_sms`: Delivers new SMS text in real-time, adds the message to `MessagesList`, and refreshes conversation threads.
- **Resilience:** The background worker loop (`ReadSseStreamAsync`) features automatic reconnect logic with a 3-second backoff in case of connection dropouts.

### 4.2. Phone REST API (Port 5000)
- `GET /contacts` – Fetches the phone contact list (`[ { "name": "...", "number": "..." } ]`).
- `GET /conversations` – Fetches recent SMS conversation threads.
- `GET /messages?address={target}` – Retrieves full message history for a specific phone number or contact ID.
- `GET /send?number={nr}&message={msg}` or `POST /send` – Sends an outgoing SMS message.
- `POST /call` – Initiates an outgoing voice call (`{"number": "..."}`).
- `POST /answer` – Answers an incoming call.
- `POST /endcall` or `POST /reject` – Ends or rejects the active call.
- `DELETE /conversations?address={target}` – Deletes an SMS thread from the phone.

### 4.3. WebDAV Remote Storage (Port 5001)
- `PROPFIND /` – Lists directory contents and file metadata for the current path (`LoadFilesAsync`).
- `GET /{path}` – Downloads a remote file to the Linux host (`DownloadFileAsync`).
- `PUT /{path}` – Uploads a local file to the phone's internal storage (`UploadFileAsync`).

---

## 5. Security Architecture and Cryptography

Security operations reside under the `PhoneToLinux.Security` namespace:

### 5.1. Key Derivation and Device Handshake (`DevicePairingService`)
1. The desktop generates a cryptographically secure 6-digit PIN using `RandomNumberGenerator.GetInt32(0, 1000000)`.
2. Resolves the active network interface MAC address on Linux (`GetDesktopMacAddress`).
3. When the user confirms pairing on the Android app, both endpoints exchange MAC addresses and the PIN.
4. A 256-bit AES master key is derived via `SHA-256`:
   $$\text{Key} = \text{SHA-256}(\text{MAC}_{\text{Desktop}} \parallel \text{MAC}_{\text{Android}} \parallel \text{PIN} \parallel \text{"PhoneToLinux\_Salt2026"})$$

### 5.2. Local Encrypted Storage (`SecureStorageService`)
- Sensitive session files (such as `paired_device.dat`) are encrypted with **AES-256-CBC**.
- A unique 16-byte Initialization Vector (**IV**) is generated per write and prepended to the binary output.

### 5.3. IP Anti-Spoofing & Auto-Discovery (`AutochangeIP`)
- When the mobile device changes Wi-Fi networks, discovery packets broadcasted over UDP port 5002 are validated against the derived pairing secret, preventing unauthorized nodes from hijacking the session.

### 5.4. Moving Target Defense (`DynamicFolderManager`)
- Protects against local disk snooping by periodically migrating encrypted storage assets into randomized subfolders.

---

## 6. Dynamic Roslyn Plugin Engine (DNN Engine)

The `DnnPluginLoader` class implements runtime extensibility:

1. **C# Source Scripts (`.cs`)**:
   - Parses code directly into a `CSharpSyntaxTree`.
   - Compiles the tree in-memory via `CSharpCompilation.Emit` against loaded application assemblies.
   - Instantiates the target class and executes methods via Reflection without restarting the main application.
2. **Precompiled Assemblies (`.dll`)**:
   - Directly loads binaries into memory via `Assembly.LoadFrom`.

---

## 7. User Interface and Presentation Layer

The UI in `MainWindow.axaml` is organized into tab views controlled by `SelectedTabIndex`:

1. **Dialer (`SelectedTabIndex = 0`):**
   - Keypad for manual dialing (`1-9, *, #, +`).
   - Quick dialing, backspace support, keyboard shortcuts (`Enter` to dial).
   - Real-time active call overlay with caller ID resolution and call timer.
2. **Contacts (`SelectedTabIndex = 1`):**
   - Address book listing with search/filter capabilities.
   - Quick-action buttons to call or message a contact directly.
3. **Chat & SMS Messages (`SelectedTabIndex = 2`):**
   - Left pane: Conversation threads (`RecentConversations`) with latest message snippets.
   - Right pane: Chronological message history (`MessagesList`) with bubble styling (incoming vs outgoing).
   - Composition bar to send new SMS messages.
   - Option to delete conversation threads.
4. **Device Pairing (`SelectedTabIndex = 3`):**
   - Visual display of the 6-digit pairing PIN and local host IP.
   - Live pairing status updates.
5. **Storage Browser (`SelectedTabIndex = 4`):**
   - Hierarchical file manager for Android internal storage.
   - File download and upload progress handling.
6. **Settings (`SelectedTabIndex = 5`):**
   - Theme toggle (Dark / Light).
   - Display of currently running version.
   - One-click update check and installation.

---

## 8. Build, Installation, and Updates

### 8.1. System Requirements
- Linux (x86_64) running kernel 5.x or higher.
- System utilities: `libnotify-bin` (for `notify-send`), `policykit-1` (for `pkexec`).
- .NET 8.0 SDK (for building from source).

### 8.2. Building from Source
```bash
# Clone the repository
git clone https://github.com/ihusky02/phonetolinuxdesktop.git
cd phonetolinuxdesktop

# Restore NuGet dependencies
dotnet restore

# Build in Release configuration
dotnet build -c Release

# Run the desktop application
dotnet run -c Release
```

### 8.3. Building Debian Package (`.deb`)
The repository includes standard Debian packaging scripts in `debian/`:
```bash
dpkg-buildpackage -us -uc -b
```

### 8.4. Automated Update Mechanism (`UpdateService`)
- The client queries `version.json` hosted on the main GitHub branch.
- If the remote version exceeds the current assembly version, it downloads `phonetolinux_update.deb` to `/tmp`.
- Automatically prompts for root authorization and installs the update via `pkexec apt install -y "/tmp/phonetolinux_update.deb"`.

---
*Technical documentation for PhoneToLinux Desktop (Version 1.1.5.3).*
