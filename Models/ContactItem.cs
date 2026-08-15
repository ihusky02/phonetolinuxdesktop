using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

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

    // Dodatkowe pola pomocnicze na wypadek, gdy serwer zwraca numer pod inną nazwą
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