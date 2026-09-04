using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace phonetolinux.Models;

public partial class ContactItem : ObservableObject
{
    [ObservableProperty]
    [property: JsonPropertyName("name")]
    [property: JsonInclude]
    private string _name = "";

    [ObservableProperty]
    [property: JsonPropertyName("phoneNumber")]
    [property: JsonInclude]
    private string _phoneNumber = "";

    // Additional helper fields in case the server returns the number under a different name
    [JsonInclude]
    [JsonPropertyName("number")]
    public string ServerNumber 
    { 
        set 
        { 
            if (string.IsNullOrEmpty(_phoneNumber)) 
                PhoneNumber = value; 
        } 
    }

    [JsonInclude]
    [JsonPropertyName("phone")]
    public string ServerPhone 
    { 
        set 
        { 
            if (string.IsNullOrEmpty(_phoneNumber)) 
                PhoneNumber = value; 
        } 
    }
}