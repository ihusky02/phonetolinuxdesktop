using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using phonetolinux.ViewModels;
using phonetolinux.Models;

namespace phonetolinux.Views;

/// <summary>
/// Code-behind logic for the main application window.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Triggered when the user changes the selected item in the conversations ListBox.
    /// </summary>
    private void ConversationsList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainViewModel vm && sender is ListBox listBox && listBox.SelectedItem is ChatConversationItem conversation)
        {
            _ = vm.SelectConversation(conversation);
        }
    }

    /// <summary>
    /// Triggered when the user clicks on a conversation row (even if already selected).
    /// </summary>
    private void Conversation_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainViewModel vm && sender is Control control && control.DataContext is ChatConversationItem conversation)
        {
            vm.SelectedConversation = conversation;
            _ = vm.SelectConversation(conversation);
        }
    }

    /// <summary>
    /// Handles drag-and-drop window movement using the custom title bar.
    /// </summary>
    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    /// <summary>
    /// Minimizes the application window.
    /// </summary>
    private void Minimize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Toggles between maximized and normal window state.
    /// </summary>
    private void Maximize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    /// <summary>
    /// Closes the application.
    /// </summary>
    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Triggers the phone call command when the Enter or Return key is pressed while on the Dialer tab.
    /// </summary>
    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Return)
        {
            if (DataContext is MainViewModel vm && vm.SelectedTabIndex == 0)
            {
                if (vm.CallCommand.CanExecute(null))
                {
                    vm.CallCommand.Execute(null);
                }
            }
        }
    }
}