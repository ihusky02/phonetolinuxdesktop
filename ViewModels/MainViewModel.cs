using CommunityToolkit.Mvvm.ComponentModel;

namespace phonetolinux.ViewModels
{
    /// <summary>
    /// Main application ViewModel responsible for coordinating top-level UI states, 
    /// active views (dialer, chat), navigation tabs, and future theme management.
    /// Acts as a pure layout coordinator without executing direct business logic.
    /// </summary>
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int _selectedTabIndex = 0;

        [ObservableProperty]
        private DialerViewModel _dialer = new();

        [ObservableProperty]
        private ChatViewModel _activeChat = new();

        [ObservableProperty]
        private string _currentTheme = "Dark"; // Placeholder prepared for future UI theme switching support
    }
}