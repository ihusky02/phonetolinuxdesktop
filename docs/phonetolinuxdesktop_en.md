# Technical Documentation: PhoneToLinux Desktop

**Version:** 1.0.1
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
- **Remote Storage & File Management (WebDAV) with IP Auto-Discovery** allowing browsing, downloading, and uploading files to/from the mobile device via Wi-Fi without manual IP entry.
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
| **D-Bus Protocol** | Tmds.DBus.Protocol | `0.21.3` | Secure Linux D-Bus communication (patched GHSA-xrw6-gwf8-vvr9 / CVE-2026-39959) |
| **Cryptography** | System.Security.Cryptography | .NET 8 BCL | AES-256-CBC encryption, SHA-256 key derivation, RandomNumberGenerator |

---

## 3. Project Structure and Code Organization

Directory layout and source file descriptions:

- `App.axaml / App.axaml.cs` – Application entry point and lifecycle manager.
- `Program.cs` – Avalonia platform configuration and `Main()` bootstrap.
- `ViewLocator.cs` – View to ViewModel resolver.
- `Converters/` – Value converters (`TabHighlightConverter.cs`).
- `Models/` – Data transfer objects (`ChatContext.cs`, `ChatModels.cs`, `ContactItem.cs`, `MmsAttachment.cs`, `PhoneFileItem.cs`).
- `ViewModels/` – Presentation logic layer:
  - `MainViewModel.cs` – Main controller managing tabs, call overlays, SSE events, and notifications.
  - `ChatViewModel.cs` – SMS thread selection and message handling.
  - `DialerViewModel.cs` – Keypad inputs and outgoing call controls.
  - `PairingViewModel.cs` – PIN generator and pairing state logic.
  - `StorageBrowserViewModel.cs` – Remote phone storage file explorer backed by automated IP resolution.
  - `SettingsViewModel.cs` – Theme selection and update checker actions.
- `Views/` – Avalonia UI XAML Views (`MainWindow.axaml`, `PairingView.axaml`, `StorageBrowserView.axaml`, `SettingsView.axaml`, `IpConfigWindow.axaml`).
- `Services/` – Shared application services:
  - `DeviceIpResolver.cs` – Dynamic IP auto-resolution service. Checks cached configuration, decrypts paired device files, and executes multi-threaded subnet socket scanning on ports 5001/5000.
  - `PhoneConfig.cs` – Persistent configuration manager (`~/.phonetolinux`).
  - `PairingListenerService.cs` – Embedded HTTP server listening on TCP port 5000 for pairing handshakes.
  - `UpdateService.cs` – GitHub version checker and `.deb` installer via `pkexec`.
  - `DnnPluginLoader.cs` – Roslyn dynamic script compiler.
- `Security/` – Cryptography and data protection (`DevicePairingService.cs`, `SecureStorageService.cs`, `AutochangeIP.cs`, `DynamicFolderManager.cs`).
- `PluginSource/` – Communication plugins (`PhoneSsePlugin.cs`, `PhoneCallPlugin.cs`, `SmsPlugin.cs`, `ContactsPlugin.cs`, `ConversationsPlugin.cs`, `StoragePlugin.cs`, `LinuxNotificationPlugin.cs`).

---

## 4. Communication Protocols and Network Channels

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

### 4.1. Automatic IP Resolution (`DeviceIpResolver`)
To eliminate manual IP prompt dialogs during storage browsing, `DeviceIpResolver` implements automatic multi-tier IP resolution:
1. Validates active TCP socket connectivity on port 5001/5000 for the cached IP address.
2. Decrypts `paired_device.dat` to test any newly paired IP address.
3. Concurrently scans local IPv4 subnets (`192.168.x.1–254`) on WebDAV (`5001`) and API (`5000`) ports.
4. Auto-saves newly discovered device IP addresses whenever network subnet or DHCP leases change.

---

## 5. Security Architecture and Cryptography

- **Vulnerability Patch GHSA-xrw6-gwf8-vvr9 (CVE-2026-39959)**: Upgraded `Tmds.DBus.Protocol` to `0.21.3+`, mitigating Denial of Service (DoS) and D-Bus signal spoofing risks.
- **AES-256-CBC Encryption**: Session credentials and pairing data are secured using AES-256 with key derivation from device MAC addresses and pairing PINs (`DevicePairingService`).

---
*Technical documentation for PhoneToLinux Desktop (Version 1.1.5.3).*