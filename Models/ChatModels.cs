using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace phonetolinux.Models;

/// <summary>
/// Represents a single chat message within a conversation thread.
/// </summary>
public partial class ChatMessageItem : ObservableObject
{
    /// <summary>
    /// The actual text content of the message.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("text")]
    private string _text = "";

    /// <summary>
    /// Indicates whether the message was sent by the user (true) or received (false).
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("isOutgoing")]
    private bool _isOutgoing = true;

    /// <summary>
    /// Fallback property to handle Android's native SMS database column "body".
    /// Maps the incoming JSON "body" to the "Text" property if it wasn't provided directly.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("body")]
    public string ServerBody 
    { 
        set 
        { 
            if (string.IsNullOrEmpty(_text)) 
            {
                Text = value ?? ""; 
            }
        } 
    }

    /// <summary>
    /// Fallback property to handle Android's native SMS database column "type".
    /// Maps the incoming JSON "type" (1 = Received/Inbox, 2 = Sent/Outbox) to the "IsOutgoing" boolean flag.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("type")]
    public int ServerType 
    { 
        set 
        { 
            IsOutgoing = (value == 2); 
        } 
    }
}

/// <summary>
/// Represents a summarized conversation thread in the recent chats list (left panel).
/// </summary>
public partial class ChatConversationItem : ObservableObject
{
    /// <summary>
    /// Display name of the contact, or the raw phone number if the name is not in the address book.
    /// </summary>
    [ObservableProperty]
    private string _contactName = "";

    /// <summary>
    /// The phone number or alphanumeric sender ID associated with the conversation.
    /// </summary>
    [ObservableProperty]
    private string _phoneNumber = "";

    /// <summary>
    /// Preview snippet of the most recent message in the thread.
    /// </summary>
    [ObservableProperty]
    private string _lastMessage = "";
}