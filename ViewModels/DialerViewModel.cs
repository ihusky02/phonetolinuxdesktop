using System;
using System.Threading.Tasks;
using phonetolinux.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace phonetolinux.ViewModels;

public partial class DialerViewModel : ObservableObject
{
    private readonly PhoneCallPlugin _callService = new();

    [ObservableProperty]
    private string _phoneNumber = "";

    [ObservableProperty]
    private bool _isInCall = false;

    [ObservableProperty]
    private bool _isIncomingCall = false;

    [ObservableProperty]
    private string _contactName = "Wybierz kontakt";

    [RelayCommand]
    private void AppendNumber(string number) => PhoneNumber += number;

    [RelayCommand]
    private void Backspace()
    {
        if (PhoneNumber.Length > 0)
            PhoneNumber = PhoneNumber.Substring(0, PhoneNumber.Length - 1);
    }

    [RelayCommand]
    private async Task Call()
    {
        if (string.IsNullOrEmpty(PhoneNumber)) return;

        bool success = await _callService.StartCallAsync(PhoneNumber);
        if (success)
        {
            IsInCall = true;
            IsIncomingCall = false;
        }
    }

    [RelayCommand]
    private async Task EndCall()
    {
        await _callService.EndCallAsync();
        ResetCallState();
    }

    [RelayCommand]
    private async Task AnswerCall()
    {
        bool success = await _callService.AnswerCallAsync();
        if (success)
        {
            IsIncomingCall = false;
            IsInCall = true;
        }
    }

    public async Task CallSpecificNumberAsync(string number)
    {
        if (!string.IsNullOrEmpty(number))
        {
            PhoneNumber = number;
            await Call();
        }
    }

    private void ResetCallState()
    {
        IsInCall = false;
        IsIncomingCall = false;
        PhoneNumber = "";
        ContactName = "Nieznany";
    }
}