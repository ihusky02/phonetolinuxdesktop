using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using phonetolinux.Models;

namespace phonetolinux.ViewModels;

public partial class StorageBrowserViewModel : ViewModelBase
{
    // Configure HttpClient timeout once upon instantiation to avoid InvalidOperationException
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };
    
    // Runtime storage for the active device IP address (pre-configured for development session)
    private static string _activeDeviceIp = "192.168.100.90";

    /// <summary>
    /// Automatically retrieves the active device IP address. 
    /// Checks the cached session variable first, falls back to configuration, or prompts the user.
    /// </summary>
    private async Task<string> GetDeviceIpAsync()
    {
        // Return cached IP if already available in the current session
        if (!string.IsNullOrEmpty(_activeDeviceIp))
        {
            return _activeDeviceIp;
        }

        // Fallback: Prompt the user manually if no IP is stored
        var manualIp = await PromptForIpAddressAsync();
        if (!string.IsNullOrWhiteSpace(manualIp))
        {
            _activeDeviceIp = manualIp.Trim();
        }

        return _activeDeviceIp;
    }

    /// <summary>
    /// Helper method to display a lightweight dark-themed input dialog for manual IP entry.
    /// </summary>
    private async Task<string?> PromptForIpAddressAsync()
    {
        var desktop = Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
        if (desktop?.MainWindow == null) return null;

        var window = new Window
        {
            Title = "Device IP Required",
            Width = 350,
            Height = 170,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = Avalonia.Media.Brush.Parse("#1E1E1E")
        };

        var textBlock = new TextBlock
        {
            Text = "Enter phone's current IP address:",
            Foreground = Avalonia.Media.Brushes.White,
            Margin = new Avalonia.Thickness(15, 15, 15, 5)
        };

        var textBox = new TextBox
        {
            Watermark = "e.g. 192.168.1.50",
            Text = _activeDeviceIp,
            Margin = new Avalonia.Thickness(15, 0, 15, 15),
            Background = Avalonia.Media.Brush.Parse("#2C2C2C"),
            Foreground = Avalonia.Media.Brushes.White,
            CaretBrush = Avalonia.Media.Brushes.White
        };

        var button = new Button
        {
            Content = "Connect",
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Avalonia.Thickness(15, 0, 15, 15),
            Background = Avalonia.Media.Brush.Parse("#1E88E5"),
            Foreground = Avalonia.Media.Brushes.White
        };

        string? result = null;
        button.Click += (_, _) =>
        {
            result = textBox.Text;
            window.Close();
        };

        window.Content = new StackPanel
        {
            Children =
            {
                textBlock,
                textBox,
                button
            }
        };

        await window.ShowDialog(desktop.MainWindow);
        return result;
    }

    [ObservableProperty]
    private string _currentPath = "/";

    [ObservableProperty]
    private ObservableCollection<PhoneFileItem> _files = new();

    [ObservableProperty]
    private PhoneFileItem? _selectedFile;

    public StorageBrowserViewModel()
    {
        // Automatically load the root directory of the phone upon initialization
        _ = LoadFilesAsync("");
    }

    /// <summary>
    /// Asynchronously fetches files and directories from the connected Android device.
    /// </summary>
    [RelayCommand]
    private async Task LoadFilesAsync(string path)
    {
        try
        {
            string targetPath = string.IsNullOrEmpty(path) ? "/" : path;
            CurrentPath = targetPath;
            
            string currentIp = await GetDeviceIpAsync();
            if (string.IsNullOrEmpty(currentIp)) return; // Abort if user canceled the prompt

            string url = $"http://{currentIp}:5000/storage/list?path={Uri.EscapeDataString(targetPath)}";
            
            Console.WriteLine($"[StorageBrowser] Requesting: {url}");
            var response = await _httpClient.GetStringAsync(url);
            Console.WriteLine($"[StorageBrowser] Response received successfully! Raw length: {response.Length}");
            
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var items = JsonSerializer.Deserialize<PhoneFileItem[]>(response, options);

            // Safely update the observable UI collection on the main thread using a new collection instance
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var newCollection = new ObservableCollection<PhoneFileItem>();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        newCollection.Add(item);
                        Console.WriteLine($"[StorageBrowser] Added item: {item.Name}");
                    }
                }
                Files = newCollection;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StorageBrowser] ERROR in LoadFilesAsync: {ex.GetType().Name} - {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the downloading of a selected file from the Android device to the Linux desktop.
    /// </summary>
    [RelayCommand]
    private async Task DownloadFileAsync()
    {
        if (SelectedFile == null || SelectedFile.IsDirectory)
            return;

        try
        {
            var mainWindow = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
            if (mainWindow == null) return;

            var dialog = new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Save file from phone",
                SuggestedFileName = SelectedFile.Name
            };

            var fileResult = await mainWindow.StorageProvider.SaveFilePickerAsync(dialog);
            if (fileResult != null)
            {
                string currentIp = await GetDeviceIpAsync();
                if (string.IsNullOrEmpty(currentIp)) return;

                string downloadUrl = $"http://{currentIp}:5000/storage/download?path={Uri.EscapeDataString(SelectedFile.RelativePath)}";
                
                using var response = await _httpClient.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();
                
                await using var contentStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = await fileResult.OpenWriteAsync();
                await contentStream.CopyToAsync(fileStream);

                Console.WriteLine($"[StorageBrowser] File successfully downloaded: {SelectedFile.Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StorageBrowser] Failed to download file: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles uploading a local file from the Linux desktop to the active path on the phone.
    /// </summary>
    [RelayCommand]
    private async Task UploadFileAsync()
    {
        try
        {
            var mainWindow = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
            if (mainWindow == null) return;

            var dialog = new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Select file to upload",
                AllowMultiple = false
            };

            var files = await mainWindow.StorageProvider.OpenFilePickerAsync(dialog);
            if (files.Count > 0)
            {
                var file = files[0];
                string fileName = file.Name;
                
                string targetPath = (CurrentPath == "/" ? "" : CurrentPath) + "/" + fileName;
                string currentIp = await GetDeviceIpAsync();
                if (string.IsNullOrEmpty(currentIp)) return;

                string uploadUrl = $"http://{currentIp}:5000/storage/upload?path={Uri.EscapeDataString(targetPath)}";

                using var content = new MultipartFormDataContent();
                await using var readStream = await file.OpenReadAsync();
                using var streamContent = new StreamContent(readStream);
                
                content.Add(streamContent, "file", fileName);

                var response = await _httpClient.PostAsync(uploadUrl, content);
                response.EnsureSuccessStatusCode();

                Console.WriteLine($"[StorageBrowser] File successfully uploaded: {fileName}");
                Refresh();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StorageBrowser] Failed to upload file: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigates one directory level up in the file hierarchy.
    /// </summary>
    [RelayCommand]
    private void NavigateUp()
    {
        if (CurrentPath == "/" || string.IsNullOrEmpty(CurrentPath)) 
            return;
        
        var parts = CurrentPath.TrimEnd('/').Split('/');
        if (parts.Length <= 1)
        {
            _ = LoadFilesAsync("");
            return;
        }
        
        var newPath = string.Join("/", parts, 0, parts.Length - 1);
        _ = LoadFilesAsync(newPath);
    }

    /// <summary>
    /// Refreshes the contents of the current directory.
    /// </summary>
    [RelayCommand]
    private void Refresh()
    {
        _ = LoadFilesAsync(CurrentPath == "/" ? "" : CurrentPath);
    }

    /// <summary>
    /// Handles item selection changes. If a directory is selected, navigates into it.
    /// </summary>
    partial void OnSelectedFileChanged(PhoneFileItem? value)
    {
        if (value == null) return;

        if (value.IsDirectory)
        {
            _ = LoadFilesAsync(value.RelativePath);
            SelectedFile = null;
        }
        else
        {
            Console.WriteLine($"[StorageBrowser] Selected file: {value.Name} ({value.FormattedSize})");
        }
    }
}