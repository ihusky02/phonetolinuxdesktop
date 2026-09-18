````# Dokumentacja Techniczna Projektu: PhoneToLinux Desktop

**Wersja:** 1.1.5.3  
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
- **Zdalny menedżer plików (WebDAV)** umożliwiający przeglądanie katalogów telefonu, pobieranie oraz wysyłanie plików przez sieć Wi-Fi.
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
| **Kryptografia** | System.Security.Cryptography | .NET 8 BCL | Szyfrowanie AES-256-CBC, funkcja skrótu SHA-256, RandomNumberGenerator |

---

## 3. Struktura Projektu i Organizacja Kodu

Główne katalogi i pliki źródłowe:

- `App.axaml / App.axaml.cs` – Punkt wejścia aplikacji Avalonia, inicjalizacja okna konfiguracji IP lub okna głównego, uruchomienie mechanizmu Auto-Discovery.
- `Program.cs` – Konfiguracja platformy Avalonia oraz wejście `Main()`.
- `ViewLocator.cs` – Mapowanie modeli widoków na widoki XAML w architekturze MVVM.
- `Converters/` – Konwertery wartości XAML (np. `TabHighlightConverter.cs` podświetlający wybraną zakładkę).
- `Models/` – Modele danych:
  - `ChatContext.cs` – Kontekst bieżącego czatu.
  - `ChatModels.cs` – `ChatMessageItem` (pojedyncza wiadomość), `ChatConversationItem` (wątek rozmowy).
  - `ContactItem.cs` – Wpis książki telefonicznej (imię/nazwisko, numer telefonu).
  - `MmsAttachment.cs` – Model załączników multimedialnych MMS.
  - `PhoneFileItem.cs` – Model pliku/katalogu w menedżerze WebDAV.
- `ViewModels/` – Warstwa logiki biznesowej i stanu widoków:
  - `MainViewModel.cs` – Główny kontroler aplikacji, zarządza przełączaniem kart, nasłuchem połączeń i globalnymi powiadomieniami.
  - `ChatViewModel.cs` – Logika czatu, wysyłania wiadomości i synchronizacji historii.
  - `DialerViewModel.cs` – Obsługa klawiatury numerycznej i inicjowania połączeń.
  - `PairingViewModel.cs` – Generowanie kodu PIN, wykrywanie lokalnego IP komputera i obsługa procesu parowania.
  - `StorageBrowserViewModel.cs` – Obsługa eksploratora plików telefonu (upload, download, nawigacja katalogów).
  - `SettingsViewModel.cs` – Konfiguracja motywu oraz sprawdzanie aktualizacji.
- `Views/` – Widoki interfejsu użytkownika (XAML):
  - `MainWindow.axaml` – Główne okno aplikacji z menu bocznym, dialerem, czatem, kontaktami i nakładką aktywnego połączenia.
  - `PairingView.axaml` – Ekran parowania z kodem PIN i statusem połączenia.
  - `StorageBrowserView.axaml` – Ekran eksploratora plików telefonu.
  - `SettingsView.axaml` – Ekran konfiguracji i aktualizacji.
  - `IpConfigWindow.axaml` – Okno pierwszego uruchomienia do wprowadzenia adresu IP.
- `Services/` – Usługi bazowe:
  - `PhoneConfig.cs` – Zapis i odczyt konfiguracji adresu IP i sekretu parowania w katalogu `~/.phonetolinux`.
  - `PairingListenerService.cs` – Wbudowany serwer HTTP nasłuchujący na porcie 5000 żądań parowania z telefonu.
  - `UpdateService.cs` – Sprawdzanie nowej wersji na GitHubie i instalowanie pakietu `.deb` przez `pkexec`.
  - `DnnPluginLoader.cs` – Silnik dynamicznej kompilacji Roslyn dla skryptów C#.
- `Security/` – Moduł bezpieczeństwa:
  - `DevicePairingService.cs` – Generowanie PIN-u, pobieranie adresu MAC i derywacja klucza AES-256 (SHA-256).
  - `SecureStorageService.cs` – Szyfrowanie i deszyfrowanie AES-256 z unikalnym wektorem inicjalizacyjnym (IV).
  - `AutochangeIP.cs` – Bezpieczne wykrywanie adresu IP telefonu przez podpisane pakiety UDP (port 5002).
  - `DynamicFolderManager.cs` – Ochrona Moving Target Defense (rotacja katalogu danych).
- `PluginSource/` – Wtyczki komunikacyjne (`IPhonetolinuxPlugin`):
  - `PhoneSsePlugin.cs` – Klient Server-Sent Events dla zdarzeń w czasie rzeczywistym.
  - `PhoneCallPlugin.cs` – REST API połączeń telefonicznych (`/call`, `/endcall`, `/answer`).
  - `SmsPlugin.cs` / `SmsHistoryPlugin.cs` – REST API wysyłania i odbierania wiadomości SMS.
  - `ContactsPlugin.cs` – REST API pobierania kontaktów z deduplikacją.
  - `ConversationsPlugin.cs` – REST API zarządzania wątkami SMS.
  - `StoragePlugin.cs` – Integracja pamięci masowej przez WebDAV.
  - `LinuxNotificationPlugin.cs` – Integracja z systemem Linux przez `notify-send`.

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

### 4.1. Server-Sent Events (SSE) – `PhoneSsePlugin`
- **Endpoint:** `GET http://{phoneIp}:5000/events`
- **Typ zawartości:** `text/event-stream`
- **Format danych:** Obiekty JSON (`PhoneEventDto`) z polami `event`, `number`, `sender`, `message`.
- **Główne zdarzenia:**
  - `incoming_call` – informuje o przychodzącym połączeniu; wyzwala powiadomienie `notify-send` o priorytecie `critical` oraz wysuwa nakładkę odebrania rozmowy w UI.
  - `call_ended` – zamyka stan aktywnego połączenia.
  - `incoming_sms` – dostarcza treść nowej wiadomości SMS i odświeża listę konwersacji.
- **Odporność na błędy:** Pętla `ReadSseStreamAsync` zawiera mechanizm automatycznego wznawiania połączenia po 3 sekundach w przypadku zerwania sieci.

### 4.2. REST API Telefonu (Port 5000)
- `GET /contacts` – Pobranie listy kontaktów (`[ { "name": "...", "number": "..." } ]`).
- `GET /conversations` – Pobranie aktywnych wątków rozmów SMS.
- `GET /messages?address={target}` – Pobranie pełnej historii wiadomości dla wskazanego numeru.
- `GET /send?number={nr}&message={msg}` lub `POST /send` – Wysłanie wiadomości SMS.
- `POST /call` – Rozpoczęcie połączenia głosowego (`{"number": "..."}`).
- `POST /answer` – Odebranie przychodzącego połączenia.
- `POST /endcall` lub `POST /reject` – Odrzucenie lub zakończenie połączenia.
- `DELETE /conversations?address={target}` – Usunięcie wątku SMS z telefonu.

### 4.3. Pamięć Masowa WebDAV (Port 5001)
- `PROPFIND /` – Pobranie listy plików i katalogów w bieżącej ścieżce.
- `GET /{path}` – Pobranie pliku z telefonu na dysk lokalny.
- `PUT /{path}` – Wysłanie pliku z dysku komputera do telefonu.

---

## 5. Bezpieczeństwo i Kryptografia

Warstwa bezpieczeństwa (`PhoneToLinux.Security`) zabezpiecza transmisję oraz dane zapisane na dysku:

### 5.1. Derywacja Klucza i Parowanie (`DevicePairingService`)
1. Komputer generuje losowy, 6-cyfrowy kod PIN (`RandomNumberGenerator.GetInt32`).
2. Pobiera adres MAC aktywnej karty sieciowej Linux (`GetDesktopMacAddress`).
3. Podczas zatwierdzenia na telefonie, komputer i smartfon wymieniają adresy MAC i PIN.
4. Generowany jest 256-bitowy klucz AES z użyciem funkcji skrótu `SHA-256`:
   $$\text{Key} = \text{SHA-256}(\text{MAC}_{\text{Desktop}} \parallel \text{MAC}_{\text{Android}} \parallel \text{PIN} \parallel \text{"PhoneToLinux\_Salt2026"})$$

### 5.2. Szyfrowanie Danych Lokalnych (`SecureStorageService`)
- Poufne dane sesji (np. `paired_device.dat`) są szyfrowane algorytmem **AES-256-CBC**.
- Przy każdym zapisie generowany jest losowy 16-bajtowy wektor inicjalizacyjny (**IV**), umieszczany na początku pliku binarnego.

### 5.3. Ochrona Przed Podszywaniem IP (`AutochangeIP`)
- Komputer weryfikuje tożsamość urządzenia za pomocą podpisanych pakietów UDP na porcie 5002, uniemożliwiając podszywanie się pod telefon w niezaufanych sieciach Wi-Fi.

### 5.4. Moving Target Defense (`DynamicFolderManager`)
- Ochrona przed statycznym skanowaniem dysku przez złośliwe procesy – okresowa migracja zaszyfrowanych plików konfiguracyjnych do losowo wyznaczanych podfolderów.

---

## 6. Dynamiczny Silnik Wtyczek Roslyn (DNN Engine)

Klasa `DnnPluginLoader` pozwala na ładowanie i wykonywanie rozszerzeń w czasie rzeczywistym:

1. **Pliki źródłowe C# (`.cs`)**:
   - Odczyt kodu źródłowego z pliku.
   - Parsowanie do drzewa składniowego `CSharpSyntaxTree`.
   - Dynamiczna kompilacja do pamięci (`CSharpCompilation.Emit`) z podpięciem zestawów bieżącej aplikacji.
   - Wywołanie metody docelowej przez mechanizm refleksji.
2. **Pliki prekompilowane (`.dll`)**:
   - Bezpośrednie załadowanie zestawu (`Assembly.LoadFrom`) i wywołanie metody.

---

## 7. Warstwa Prezentacji i Funkcje UI

Interfejs użytkownika w `MainWindow.axaml` podzielony jest na zakładki boczne sterowane indeksem `SelectedTabIndex`:

1. **Dialer (`SelectedTabIndex = 0`):**
   - Klawiatura numeryczna do wpisywania numeru telefonu (`1-9, *, #, +`).
   - Szybkie wybieranie numeru, kasowanie znaku (`Backspace`), klawisze funkcyjne `Enter`.
   - Zintegrowana nakładka rozmowy (Call Overlay) z informacją o nazwie kontaktu i czasie trwania połączenia.
2. **Kontakty (`SelectedTabIndex = 1`):**
   - Lista kontaktów pobrana z telefonu.
   - Przyciski szybkiego dzwonienia oraz wysłania wiadomości SMS do wybranego kontaktu.
3. **Czat i Wiadomości SMS (`SelectedTabIndex = 2`):**
   - Lewy panel: Lista ostatnich konwersacji (`RecentConversations`) z podglądem ostatniej wiadomości.
   - Prawy panel: Historia wiadomości wybranego kontaktu (`MessagesList`) z podziałem na wiadomości przychodzące i wychodzące.
   - Pole tekstowe do wysyłania nowych wiadomości SMS.
   - Opcja usuwania całego wątku wiadomości.
4. **Parowanie Urządzenia (`SelectedTabIndex = 3`):**
   - Wyświetlanie 6-cyfrowego kodu PIN oraz adresu IP komputera.
   - Status połączenia w czasie rzeczywistym.
5. **Przeglądarka Pamięci Telefonu (`SelectedTabIndex = 4`):**
   - Drzewo i lista plików z pamięci wewnętrznej telefonu.
   - Pobieranie plików z telefonu na dysk komputera.
   - Wysyłanie plików z komputera do telefonu.
6. **Ustawienia (`SelectedTabIndex = 5`):**
   - Wybór motywu graficznego (Dark / Light).
   - Informacja o bieżącej wersji aplikacji.
   - Przycisk sprawdzania i instalowania aktualizacji.

---

## 8. Budowanie, Instalacja i Aktualizacja

### 8.1. Wymagania Systemowe
- Linux (x86_64) z jądrem 5.x lub nowszym.
- Zainstalowane pakiety systemowe: `libnotify-bin` (dla `notify-send`), `policykit-1` (dla `pkexec`).
- .NET SDK 8.0 do budowania ze źródeł.

### 8.2. Kompilacja ze Źródeł
```bash
# Klonowanie repozytorium
git clone https://github.com/ihusky02/phonetolinuxdesktop.git
cd phonetolinuxdesktop

# Przywrócenie pakietów NuGet
dotnet restore

# Budowanie w trybie Release
dotnet build -c Release

# Uruchomienie aplikacji
dotnet run -c Release
```

### 8.3. Budowanie Pakietu Debian `.deb`
W repozytorium przygotowano pliki konfiguracyjne Debiana w folderze `debian/`:
```bash
dpkg-buildpackage -us -uc -b
```

### 8.4. Mechanizm Aktualizacji (`UpdateService`)
- Aplikacja odpytuje plik `version.json` na GitHubie.
- Jeśli wersja na serwerze jest wyższa niż lokalna, aplikacja pobiera plik `phonetolinux_update.deb` do katalogu `/tmp`.
- Następuje automatyczne uruchomienie instalatora za pomocą `pkexec apt install -y "/tmp/phonetolinux_update.deb"`.

---
*Dokumentacja wygenerowana dla projektu PhoneToLinux Desktop (wersja 1.1.5.3).*
````