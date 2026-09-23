# Dokumentacja Techniczna Projektu: PhoneToLinux Desktop

**Wersja:** 1.0.1 
**Platforma:** .NET 8.0 (`net8.0`) / Linux (x64)  
**Środowisko graficzne:** Avalonia UI 11  
**Plik projektu:** `phonetolinux.csproj`  
**Autor / Repozytorium:** ihusky02 / PhoneToLinux Desktop  

---

## 1. Wprowadzenie i Cel Projektu

**PhoneToLinux Desktop** to nowoczesna aplikacja desktopowa dla systemu Linux, umożliwiająca pełną integrację komputera ze smartfonem z systemem Android. Aplikacja eliminuje potrzebę sięgania po telefon podczas pracy na komputerze, oferując:

- **Wykonywanie i odbieranie połączeń głosowych** bezpośrednio z poziomu wbudowanego dialera i nakładki powiadomień.
- **Wysyłanie i odbieranie wiadomości SMS/MMS** wraz z pełną synchronizacją wątków konwersacji w czasie rzeczywistym.
- **Synchronizację kontaktów** z książki adresowej telefonu z automatycznym dopasowywaniem numerów do nazw kontaktów.
- **Powiadomienia natywne systemu Linux** za pośrednictwem systemowego demona powiadomień (`notify-send` / libnotify).
- **Zdalny menedżer plików (WebDAV) z automatycznym wykrywaniem IP** umożliwiający przeglądanie pamięci telefonu, pobieranie oraz wysyłanie plików bez konieczności ręcznego wpisywania adresu IP.
- **Bezpieczne parowanie urządzeń** przy użyciu 6-cyfrowego kodu PIN oraz szyfrowania danych lokalnych kluczem AES-256.
- **Dynamiczny silnik wtyczek Roslyn (DNN Engine)** kompilujący skrypty C# w locie w pamięci podręcznej aplikacji.
- **Zautomatyzowany mechanizm aktualizacji** pobierający najnowsze pakiety instalacyjne `.deb` bezpośrednio z GitHub.

---

## 2. Architektura i Wzorzec Projektowy

Aplikacja została zaprojektowana w architekturze **MVVM (Model-View-ViewModel)** z modułowym podziałem odpowiedzialności oraz asynchronicznym modelem przetwarzania zdarzeń.

### 2.1. Stos Technologiczny

| Kategoria | Technologia / Komponent | Wersja | Rola w projekcie |
| :--- | :--- | :--- | :--- |
| **Środowisko** | .NET SDK / CLR | `8.0` | Środowisko uruchomieniowe |
| **Język** | C# | `12.0` | Implementacja logiki i widoków |
| **UI Framework** | Avalonia UI | `11.2.3` | Wieloplatformowy interfejs graficzny |
| **Motyw i Fonty** | Fluent Theme, Inter Font | `11.2.3` | Nowoczesny interfejs graficzny w stylu Fluent |
| **Wzorzec MVVM** | CommunityToolkit.Mvvm | `8.4.2` | Generator kodu, `[ObservableProperty]`, `[RelayCommand]` |
| **Kompilacja w locie** | Microsoft.CodeAnalysis.CSharp | `5.9.0` | Silnik Roslyn do dynamicznego ładowania wtyczek (.cs) |
| **Klient WebDAV** | WebDav.Client | `2.9.0` | Obsługa pamięci masowej telefonu |
| **Komunikacja D-Bus** | Tmds.DBus.Protocol | `0.21.3` | Bezpieczna integracja z magistralą D-Bus na Linuxie (załatana podatność GHSA-xrw6-gwf8-vvr9) |
| **Kryptografia** | System.Security.Cryptography | .NET 8 BCL | Szyfrowanie AES-256-CBC, funkcja skrótu SHA-256, RandomNumberGenerator |

---

## 3. Struktura Projektu i Organizacja Kodu

Główne katalogi i pliki źródłowe:

- `App.axaml / App.axaml.cs` – Punkt wejścia aplikacji Avalonia, inicjalizacja okna konfiguracji IP lub okna głównego, uruchomienie mechanizmu Auto-Discovery.
- `Program.cs` – Konfiguracja platformy Avalonia oraz wejście `Main()`.
- `ViewLocator.cs` – Mapowanie modeli widoków na widoki XAML w architekturze MVVM.
- `Converters/` – Konwertery wartości XAML (`TabHighlightConverter.cs`).
- `Models/` – Modele danych (`ChatContext.cs`, `ChatModels.cs`, `ContactItem.cs`, `MmsAttachment.cs`, `PhoneFileItem.cs`).
- `ViewModels/` – Warstwa logiki biznesowej i stanu widoków:
  - `MainViewModel.cs` – Główny kontroler aplikacji.
  - `ChatViewModel.cs` – Logika czatu i wiadomości.
  - `DialerViewModel.cs` – Obsługa klawiatury numerycznej.
  - `PairingViewModel.cs` – Kod PIN i status parowania.
  - `StorageBrowserViewModel.cs` – Eksplorator plików telefonu oparty na automatycznym rozwiązywaniu adresu IP.
  - `SettingsViewModel.cs` – Konfiguracja i aktualizacje.
- `Views/` – Widoki interfejsu użytkownika (`MainWindow.axaml`, `PairingView.axaml`, `StorageBrowserView.axaml`, `SettingsView.axaml`, `IpConfigWindow.axaml`).
- `Services/` – Usługi bazowe:
  - `DeviceIpResolver.cs` – Automatyczne wykrywanie adresu IP telefonu w sieci lokalnej (sprawdzanie pamięci podręcznej, szyfrowanego pliku parowania i natychmiastowe skanowanie podsieci Wi-Fi na portach 5001/5000).
  - `PhoneConfig.cs` – Zapis i odczyt konfiguracji adresu IP i sekretu parowania w katalogu `~/.phonetolinux`.
  - `PairingListenerService.cs` – Wbudowany serwer HTTP nasłuchujący na porcie 5000 żądań parowania z telefonu.
  - `UpdateService.cs` – Sprawdzanie nowej wersji na GitHubie i instalowanie pakietu `.deb` przez `pkexec`.
  - `DnnPluginLoader.cs` – Silnik dynamicznej kompilacji Roslyn dla skryptów C#.
- `Security/` – Moduł bezpieczeństwa (`DevicePairingService.cs`, `SecureStorageService.cs`, `AutochangeIP.cs`, `DynamicFolderManager.cs`).
- `PluginSource/` – Wtyczki komunikacyjne (`PhoneSsePlugin.cs`, `PhoneCallPlugin.cs`, `SmsPlugin.cs`, `ContactsPlugin.cs`, `ConversationsPlugin.cs`, `StoragePlugin.cs`, `LinuxNotificationPlugin.cs`).

---

## 4. Protokoły Komunikacyjne i Kanały Sieciowe

Aplikacja komunikuje się ze smartfonem w sieci lokalnej (Wi-Fi lub USB Tethering) przy użyciu 4 głównych kanałów:

```
+---------------------------------------------------------------------------------------+
|                                  ZESTAWIENIE PORTÓW                                   |
+-------------------+---------+---------------------------------------------------------+
| Port              | Typ     | Funkcja                                                 |
+-------------------+---------+---------------------------------------------------------+
| TCP 5000 (Telefon)| HTTP/SSE| REST API telefonu + Strumień Server-Sent Events         |
| TCP 5000 (Desktop)| HTTP    | Serwer nasłuchujący żądania parowania z telefonu        |
| TCP 5001 (Telefon)| WebDAV  | Obsługa przesyłania plików z i do pamięci urządzenia    |
| UDP 5002 (Telefon)| UDP     | Podpisany pakiet autowykrywania IP telefonu             |
+-------------------+---------+---------------------------------------------------------+
```

### 4.1. Automatyczne Wykrywanie IP Telefonu (`DeviceIpResolver`)
W celu wyeliminowania konieczności ręcznego podawania IP przed przeglądaniem plików, moduł `DeviceIpResolver` stosuje sekwencyjne autowykrywanie:
1. Sprawdzenie aktywnego połączenia socket na porcie 5001/5000 dla ostatnio zapisanego adresu IP w `PhoneConfig`.
2. Odszyfrowanie pliku sesji `paired_device.dat` i przetestowanie zawartego w nim IP.
3. Równoległe, szybkie skanowanie podsieci IPv4 komputera (np. `192.168.1.1–254`) na portach WebDAV (`5001`) i API (`5000`).
4. Automatyczne zapisanie nowo wykrytego adresu IP po zmianie podsieci/DHCP.

---

## 5. Bezpieczeństwo i Kryptografia

- **Załatanie podatności GHSA-xrw6-gwf8-vvr9 (CVE-2026-39959)**: Wdrożono pakiet `Tmds.DBus.Protocol` w wersji `0.21.3+`, eliminując ryzyko ataków odmowy usługi (DoS) oraz podszywania się pod sygnały D-Bus.
- **Szyfrowanie AES-256-CBC**: Dane parowania i klucze sesyjne są szyfrowane na dysku kluczem derywowanym z PIN-u i adresów MAC (`DevicePairingService`).

---
*Dokumentacja wygenerowana dla projektu PhoneToLinux Desktop (wersja 1.1.5.3).*