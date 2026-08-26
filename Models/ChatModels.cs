using CommunityToolkit.Mvvm.ComponentModel;

namespace phonetolinux.Models;

public partial class ChatMessageItem : ObservableObject
{
    [ObservableProperty]
    private string _text = "";

    [ObservableProperty]
    private bool _isOutgoing = true;
}

public partial class ChatConversationItem : ObservableObject
{
    [ObservableProperty]
    private string _contactName = "";

    [ObservableProperty]
    private string _phoneNumber = "";

    [ObservableProperty]
    private string _lastMessage = "";
}