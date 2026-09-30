using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using phonetolinux.ViewModels;
using System.Collections.Specialized;

namespace phonetolinux.Views;

/// <summary>
/// Code-behind logic for the main application window.
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.MessagesList.CollectionChanged -= OnMessagesListChanged;
        }

        _viewModel = DataContext as MainViewModel;

        if (_viewModel != null)
        {
            _viewModel.MessagesList.CollectionChanged += OnMessagesListChanged;
        }
    }

    private void OnMessagesListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ChatScrollViewer?.ScrollToEnd();
        }, DispatcherPriority.Loaded);
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