using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMultipleNumbers))]
    private ObservableCollection<string> _phoneNumbers = new();

    [ObservableProperty]
    private string _selectedPhoneNumber = "";

    public bool HasMultipleNumbers => PhoneNumbers != null && PhoneNumbers.Count > 1;

    partial void OnPhoneNumberChanged(string value)
    {
        if (string.IsNullOrEmpty(SelectedPhoneNumber))
        {
            SelectedPhoneNumber = value;
        }
        if (PhoneNumbers.Count == 0 && !string.IsNullOrEmpty(value))
        {
            PhoneNumbers.Add(value);
        }
    }

    partial void OnPhoneNumbersChanged(ObservableCollection<string> value)
    {
        if (value != null && value.Count > 0 && string.IsNullOrEmpty(SelectedPhoneNumber))
        {
            SelectedPhoneNumber = value[0];
        }
    }

    // Additional helper fields in case the server returns the number under a different name
    [JsonInclude]
    [JsonPropertyName("number")]
    public string ServerNumber 
    { 
        set 
        { 
            if (string.IsNullOrEmpty(PhoneNumber)) 
                PhoneNumber = value; 
        } 
    }

    [JsonInclude]
    [JsonPropertyName("phone")]
    public string ServerPhone 
    { 
        set 
        { 
            if (string.IsNullOrEmpty(PhoneNumber)) 
                PhoneNumber = value; 
        } 
    }
}
