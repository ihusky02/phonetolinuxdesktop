# Timeline Changelog / Dziennik Zmian

---

## [2026-09-25] - UI & UX Improvements: Chat Conversation Selection & Navigation Rail Vector Icons

### 🇵🇱 Wersja Polska

#### 1. Naprawa podświetlania elementów w liście czatu (Tab 2: Chat)
- **Problem:**
  - W lewym panelu modułu czatu każdy element listy ostatnich rozmów (`RecentConversations`) był opakowany w statyczny kontener `<Border Background="#CC1E1E1E">`.
  - Powodowało to wrażenie, że wszystkie wątki na liście są trwale podświetlone/zaznaczone jednocześnie.
  - Lista korzystała z generycznego `ItemsControl`, który nie zarządzał stanem zaznaczenia elementu i nie synchronizował go z właściwością `SelectedConversation`.
  - Po kliknięciu w element nie dochodziło do uaktualnienia zaznaczenia w ViewModelu, a menu kontekstowe odwoływało się do nieistniejącej bezpośrednio nazwy komendy (`DeleteChatCommand`).
- **Rozwiązanie:**
  - Zastąpiono `ItemsControl` dedykowanym komponentem `ListBox` z dwukierunkowym powiązaniem `SelectedItem="{Binding SelectedConversation}"`.
  - Usunięto sztywne tło `#CC1E1E1E` z elementów, dzięki czemu elementy domyślnie posiadają przezroczyste tło.
  - Zdefiniowano dedykowane pseudoklasy stylów Avalonia UI:
    - `:pointerover` – delikatne, płynne rozjaśnienie tła pod kursorem myszy (`#1AFFFFFF`).
    - `:selected` – czytelne, akcentowe podświetlenie aktywnego wątku (`#253545`).
  - W [`MainViewModel.cs`](file:///home/stanislawtlolka/RiderProjects/phonetolinuxdesktop/ViewModels/MainViewModel.cs):
    - Wdrożono metodę częściową `OnSelectedConversationChanged`, która automatycznie wczytuje historię wiadomości (`LoadMessagesForNumberAsync`) przy zmianie zaznaczenia.
    - Zsynchronizowano właściwość `SelectedConversation` w komendach `SelectConversation` i `SendMessageToContact`.
    - Zabezpieczono zachowywanie aktywnego wątku podczas odświeżania listy (`LoadConversationsAsync`).
    - Dodano alias `DeleteChatCommand => DeleteConversationCommand` dla pełnej kompatybilności powiązań w XAML.

#### 2. Wdrożenie ikon wektorowych w lewym pasku nawigacji
- **Problem:**
  - Przyciski nawigacyjne po lewej stronie od ekranu klawiatury numerycznej (Dialer) oraz pozostałych zakładek wyświetlały zwykłe emoji tekstowe (`📱`, `📇`, `💬`, `⚙️`).
  - Powodowało to niespójność wizualną względem nowoczesnej ikony wektorowej pamięci urządzenia (`Storage / PathIcon`).
- **Rozwiązanie:**
  - W [`Views/MainWindow.axaml`](file:///home/stanislawtlolka/RiderProjects/phonetolinuxdesktop/Views/MainWindow.axaml) zastąpiono elementy `TextBlock` z emoji ostrymi, skalowalnymi ikonami wektorowymi `PathIcon`:
    - **Książka adresowa (Kontakty, Indeks 1):** Karta kontaktowa z popiersiem w kolorze jasnoniebieskim (`#42A5F5`).
    - **Czat (Wiadomości, Indeks 2):** Dymek konwersacji z liniami tekstu w kolorze błękitnym (`#29B6F6`).
    - **Klawiatura numeryczna / Telefon (Dialer, Indeks 0):** Słuchawka telefonu w kolorze zielonym (`#4CAF50`).
    - **Ustawienia (Indeks 3):** Zębatka konfiguracyjna w kolorze neutralnej szarości (`#9CA3AF`) ze wsparciem podświetlenia aktywnej zakładki.

---

### 🇬🇧 English Version

#### 1. Fix Conversation Item Highlight Glitch in Chat List (Tab 2: Chat)
- **Problem:**
  - In the left conversation pane of the Chat module, every item in `RecentConversations` was wrapped inside a static container `<Border Background="#CC1E1E1E">`.
  - In Avalonia's dark theme, this static card background caused every single conversation in the list to appear permanently selected and highlighted at the same time.
  - The conversation list was rendered using a generic `ItemsControl`, which lacks native selection tracking and was not synchronized with `SelectedConversation`.
  - Clicking a conversation item did not assign `SelectedConversation` in the ViewModel, and the context menu delete binding pointed to a mismatched command name.
- **Solution:**
  - Replaced `ItemsControl` with an Avalonia `ListBox` bound to `SelectedItem="{Binding SelectedConversation}"`.
  - Removed the static `#CC1E1E1E` background so unselected items remain transparent.
  - Implemented tailored Avalonia UI style pseudo-classes:
    - `:pointerover` – subtle hover highlight under the cursor (`#1AFFFFFF`).
    - `:selected` – distinct accent highlight for the active thread (`#253545`).
  - In [`MainViewModel.cs`](file:///home/stanislawtlolka/RiderProjects/phonetolinuxdesktop/ViewModels/MainViewModel.cs):
    - Added partial method `OnSelectedConversationChanged` to automatically trigger conversation message loading (`LoadMessagesForNumberAsync`) on selection change.
    - Synchronized `SelectedConversation` across `SelectConversation` and `SendMessageToContact`.
    - Added state preservation to maintain the active conversation selection across background list reloads in `LoadConversationsAsync`.
    - Added the `DeleteChatCommand => DeleteConversationCommand` alias for robust XAML binding compatibility.

#### 2. Modern Vector Path Icons in Left Navigation Rail
- **Problem:**
  - The navigation rail situated immediately to the left of the numeric keypad dialer and other tabs relied on raw text emojis (`📱`, `📇`, `💬`, `⚙️`).
  - This looked inconsistent alongside the modern vector `PathIcon` used for the Storage tab.
- **Solution:**
  - In [`Views/MainWindow.axaml`](file:///home/stanislawtlolka/RiderProjects/phonetolinuxdesktop/Views/MainWindow.axaml), replaced all emoji `TextBlock`s with crisp, scalable vector `PathIcon` elements:
    - **Address Book (Contacts, Index 1):** Contact badge/card icon in bright blue (`#42A5F5`).
    - **Chat (Messages, Index 2):** Speech bubble conversation icon in light blue (`#29B6F6`).
    - **Numeric Dialer / Phone (Dialer, Index 0):** Phone handset icon in material green (`#4CAF50`).
    - **Settings (Index 3):** Settings gear icon in neutral gray (`#9CA3AF`) with full active-tab highlight support.
