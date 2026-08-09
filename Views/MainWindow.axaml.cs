using Avalonia.Controls;
using Avalonia.Input;
using AndroidCallBridge.ViewModels;

namespace AndroidCallBridge.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // Bezpośrednie chwytanie wpisywanego tekstu na poziomie całego okna
    protected override void OnTextInput(TextInputEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.SelectedTabIndex == 1)
        {
            if (!string.IsNullOrEmpty(e.Text) && char.IsLetterOrDigit(e.Text[0]))
            {
                vm.SearchQuery += e.Text;
                vm.FilterContacts();
                e.Handled = true; // Blokujemy przekazanie znaku dalej
            }
        }
        
        base.OnTextInput(e);
    }

    // Bezpośrednie chwytanie Backspace'a
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.SelectedTabIndex == 1)
        {
            if (e.Key == Key.Back && vm.SearchQuery.Length > 0)
            {
                vm.SearchQuery = vm.SearchQuery.Substring(0, vm.SearchQuery.Length - 1);
                vm.FilterContacts();
                e.Handled = true;
            }
        }
        
        base.OnKeyDown(e);
    }
}