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

        AddHandler(TextInputEvent, OnWindowTextInput, RoutingStrategies.Tunnel);
        AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);
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

    private void OnWindowTextInput(object? sender, TextInputEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.SelectedTabIndex == 1)
        {
            if (!string.IsNullOrEmpty(e.Text) && char.IsLetterOrDigit(e.Text[0]))
            {
                vm.SearchQuery += e.Text;
                vm.FilterContacts();
                e.Handled = true;
            }
        }
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            if (vm.SelectedTabIndex == 1)
            {
                if (e.Key == Key.Back && vm.SearchQuery.Length > 0)
                {
                    vm.SearchQuery = vm.SearchQuery.Substring(0, vm.SearchQuery.Length - 1);
                    vm.FilterContacts();
                    e.Handled = true;
                }
            }
            else if (vm.SelectedTabIndex == 0)
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                {
                    if (vm.CallCommand.CanExecute(null))
                    {
                        vm.CallCommand.Execute(null);
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
