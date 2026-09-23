<img width="1920" height="1080" alt="PhoneToLinux Desktop Screenshot" src="https://github.com/user-attachments/assets/ab01e90e-5e46-417d-888f-8b2ef8cad087" />

# PhoneToLinux Desktop

A modern desktop application built with Avalonia UI and .NET 8 for integrating Android smartphones directly into the Linux desktop environment.

---

## 🇬🇧 English Documentation

### What Works
- **Voice Calls**: Place and receive phone calls directly from Linux with real-time notification overlays and built-in dialer.
- **SMS Integration**: Send and receive SMS messages with full conversation thread synchronization.
- **IP Auto-Detection**: Seamless automatic IP resolution (`DeviceIpResolver`) for remote file storage browsing without manual IP entry.
- **Remote File Browser**: Browse internal phone storage, upload, and download files over WebDAV.
- **Native Notifications**: Linux desktop notifications via `notify-send`.
- **Security & Privacy**: AES-256 encrypted session storage, cryptographically derived master keys, and patched D-Bus protocols.
- **Auto-Update System**: One-click update mechanism via settings.

### Installation

#### 🟦 Fedora (via Copr Repository)

You can install `phonetolinuxdesktop` directly from the Fedora Copr repository:

1. **Enable the Copr repository:**
   ```bash
   sudo dnf copr enable stanislav1988/phonetolinuxdesktop
   ```

2. **Install the package:**
   ```bash
   sudo dnf install phonetolinuxdesktop
   ```

3. **Run the application:**
   ```bash
   phonetolinuxdesktop
   ```

#### 🌀 ~~Debian / Ubuntu (.deb Package) [OUTDATED / DEPRECATED]~~

> ⚠️ **Notice:** Manual installation via `.deb` package links is **outdated / deprecated**. Please install `phonetolinuxdesktop` via the official Fedora Copr repository or build from source.

~~Download the `.deb` package directly from Google Drive and install it manually:~~  
~~`sudo apt install ./phonetolinuxdesktop*.deb`~~

---

## 🇵🇱 Polska Dokumentacja

### Co działa
- **Połączenia głosowe**: Wykonywanie i odbieranie połączeń telefonicznych z nakładką powiadomień i wbudowanym dialerem.
- **Wiadomości SMS**: Wysyłanie i odbieranie wiadomości z pełną synchronizacją wątków w czasie rzeczywistym.
- **Automatyczne wykrywanie IP**: Moduł `DeviceIpResolver` automatycznie wykrywa i łączy się z telefonem w sieci lokalnej bez konieczności ręcznego podawania IP.
- **Eksplorator plików**: Przeglądanie pamięci telefonu, pobieranie oraz wysyłanie plików przez protokół WebDAV.
- **Powiadomienia natywne**: Powiadomienia w systemie Linux przy użyciu `notify-send`.
- **Bezpieczeństwo**: Szyfrowanie sesji kluczem AES-256 oraz aktualne pakiety bezpieczeństwa.

### Instalacja

#### 🟦 Fedora (repozytorium Copr)

Możesz zainstalować pakiet `phonetolinuxdesktop` bezpośrednio z repozytorium Fedora Copr:

1. **Włącz repozytorium Copr:**
   ```bash
   sudo dnf copr enable stanislav1988/phonetolinuxdesktop
   ```

2. **Zainstaluj pakiet:**
   ```bash
   sudo dnf install phonetolinuxdesktop
   ```

3. **Uruchom aplikację:**
   ```bash
   phonetolinuxdesktop
   ```

#### 🌀 ~~Debian / Ubuntu (Pakiet .deb) [PRZESTARZAŁE / NIEAKTUALNE]~~

> ⚠️ **Uwaga:** Ręczna instalacja z pakietu `.deb` z dysku Google jest **nieaktualna i przestarzała**. Zaleca się instalację pakietu z repozytorium Fedora Copr lub kompilację ze źródeł.

~~Pobierz pakiet `.deb` z Google Drive i zainstaluj go ręcznie:~~  
~~`sudo apt install ./phonetolinuxdesktop*.deb`~~
