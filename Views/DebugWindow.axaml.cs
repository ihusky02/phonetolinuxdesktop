using Avalonia.Controls;
using Avalonia.Interactivity;
using phonetolinux.ViewModels;

namespace phonetolinux.Views;

public partial class DebugWindow : Window
{
    public DebugWindow()
    {
        InitializeComponent();
    }

    private void OnSubmitClick(object? sender, RoutedEventArgs e)
    {
        string password = PasswordBox.Text ?? "";
        if (DebugSecurityManager.ValidatePasswordStrength(password, out string error))
        {
            Close(true);
        }
        else
        {
            ErrorTextBlock.Text = error;
        }
    }
}