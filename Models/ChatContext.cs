using System.Collections.Generic;
using System.Collections.ObjectModel; // <-- KLUCZOWE: Ten using jest wymagany dla ObservableCollection
using phonetolinux.ViewModels;

namespace phonetolinux.Models;

public class ChatContext
{
    public string ContactName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string RawMessageText { get; set; } = "";
    public List<ContactItem> AllContacts { get; set; } = new();
    
    public ObservableCollection<ChatConversationItem> RecentConversations { get; set; } = new();
}