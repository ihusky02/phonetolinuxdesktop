using System;
using System.Threading.Tasks;
using System.Windows.Input;
using phonetolinux.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

public class SettingsViewModel : ReactiveObject
{
    // Get current version of the application dynamically
    public string CurrentVersion { get; } = 
        typeof(SettingsViewModel).Assembly.GetName().Version?.ToString() ?? "1.0.0.0";

    public ICommand CheckForUpdatesCommand { get; }

    public SettingsViewModel()
    {
        // Initialize the command tied to the update check method
        CheckForUpdatesCommand = ReactiveCommand.CreateFromTask(CheckUpdatesAsync);
    }

    private async Task CheckUpdatesAsync()
    {
        // Call the service we created earlier
        var (hasUpdate, version, changelog, url) = await UpdateService.CheckForUpdatesAsync();

        if (hasUpdate)
        {
            // TODO: Show a dialog to the user like: "New version {version} available! Update now?"
            // If user agrees, trigger the installation:
            await UpdateService.DownloadAndInstallUpdateAsync(url);
        }
        else
        {
            // TODO: Show a notification: "You are using the latest version."
        }
    }
}