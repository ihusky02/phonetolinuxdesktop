using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebDav;
using phonetolinux.Models;
using phonetolinux.Services;

namespace phonetolinux.ViewModels;

public partial class StorageBrowserViewModel : ViewModelBase
{
    // Runtime storage for the active device IP address
    private static string _activeDeviceIp = string.Empty;

    /// <summary>
    /// Automatically retrieves the active device IP address. 
    /// Checks cached session/paired config first, performs local subnet auto-discovery, or prompts user as fallback.
    /// </summary>
    private async Task<string> GetDeviceIpAsync()
    {
        if (!string.IsNullOrEmpty(_activeDeviceIp) && await DeviceIpResolver.TestPortAsync(_activeDeviceIp, 5001, 300))
        {
            return _activeDeviceIp;
        }

        // Auto-detect IP from cached config, paired credentials, or subnet scanning
        string autoIp = await DeviceIpResolver.ResolvePhoneIpAsync();
        if (!string.IsNullOrWhiteSpace(autoIp))
        {
            _activeDeviceIp = autoIp;
            return _activeDeviceIp;
        }

        // Prompt user only if auto-detection fails to find phone
        var manualIp = await PromptForIpAddressAsync();
        if (!string.IsNullOrWhiteSpace(manualIp))
        {
            _activeDeviceIp = manualIp.Trim();
            PhoneConfig.SaveIp(_activeDeviceIp);
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
            Watermark = "e.g. 192.168.100.90",
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
        _ = LoadFilesAsync("/");
    }

    /// <summary>
    /// Asynchronously fetches files and directories via WebDAV PROPFIND (Port 5001).
    /// </summary>
    [RelayCommand]
    private async Task LoadFilesAsync(string path)
    {
        try
        {
            string targetPath = string.IsNullOrEmpty(path) ? "/" : path;
            
            string currentIp = await GetDeviceIpAsync();
            if (string.IsNullOrEmpty(currentIp)) return;

            var clientParams = new WebDavClientParams
            {
                BaseAddress = new Uri($"http://{currentIp}:5001/"),
                Timeout = TimeSpan.FromSeconds(10)
            };

            using var client = new WebDavClient(clientParams);
            Console.WriteLine($"[WebDAV] Sending PROPFIND for path: {targetPath}");
            
            var result = await client.Propfind(targetPath);

            if (result.IsSuccessful)
            {
                CurrentPath = targetPath;
                var newCollection = new ObservableCollection<PhoneFileItem>();

                // Skip the first item as it represents the queried directory itself
                foreach (var res in result.Resources.Skip(1))
                {
                    var uri = new Uri(res.Uri, UriKind.RelativeOrAbsolute);
                    string rawPath = uri.IsAbsoluteUri ? uri.AbsolutePath : res.Uri;
                    string cleanPath = Uri.UnescapeDataString(rawPath);
                    string name = cleanPath.TrimEnd('/').Split('/').LastOrDefault() ?? "Unknown";

                    newCollection.Add(new PhoneFileItem
                    {
                        Name = name,
                        RelativePath = cleanPath,
                        IsDirectory = res.IsCollection,
                        SizeBytes = res.ContentLength ?? 0
                    });
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Files = newCollection;
                });
                
                Console.WriteLine($"[WebDAV] Successfully loaded {newCollection.Count} items.");
            }
            else
            {
                Console.WriteLine($"[WebDAV] PROPFIND failed: {result.StatusCode} - {result.Description}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebDAV Error] LoadFilesAsync: {ex.GetType().Name} - {ex.Message}");
        }
    }

    /// <summary>
    /// Downloads selected file using WebDAV GET (Port 5001).
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

                var clientParams = new WebDavClientParams
                {
                    BaseAddress = new Uri($"http://{currentIp}:5001/"),
                    Timeout = TimeSpan.FromMinutes(5)
                };

                using var client = new WebDavClient(clientParams);
                var response = await client.GetRawFile(SelectedFile.RelativePath);

                if (response.IsSuccessful)
                {
                    await using var destinationStream = await fileResult.OpenWriteAsync();
                    await response.Stream.CopyToAsync(destinationStream);
                    Console.WriteLine($"[WebDAV] File successfully downloaded: {SelectedFile.Name}");
                }
                else
                {
                    Console.WriteLine($"[WebDAV] Download failed: {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebDAV Error] DownloadFileAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Uploads a local file using WebDAV PUT (Port 5001).
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
                
                string currentIp = await GetDeviceIpAsync();
                if (string.IsNullOrEmpty(currentIp)) return;

                var clientParams = new WebDavClientParams
                {
                    BaseAddress = new Uri($"http://{currentIp}:5001/"),
                    Timeout = TimeSpan.FromMinutes(5)
                };

                using var client = new WebDavClient(clientParams);
                string remotePath = (CurrentPath.EndsWith("/") ? CurrentPath : CurrentPath + "/") + fileName;

                await using var readStream = await file.OpenReadAsync();
                var response = await client.PutFile(remotePath, readStream);

                if (response.IsSuccessful)
                {
                    Console.WriteLine($"[WebDAV] File successfully uploaded: {fileName}");
                    Refresh();
                }
                else
                {
                    Console.WriteLine($"[WebDAV] Upload failed: {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebDAV Error] UploadFileAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper method for Drag &amp; Drop: Downloads selected file to a specific local temp path.
    /// </summary>
    public async Task DownloadSelectedFileToPathAsync(string destinationPath)
    {
        if (SelectedFile == null || SelectedFile.IsDirectory) return;

        try
        {
            string currentIp = await GetDeviceIpAsync();
            if (string.IsNullOrEmpty(currentIp)) return;

            var clientParams = new WebDavClientParams
            {
                BaseAddress = new Uri($"http://{currentIp}:5001/"),
                Timeout = TimeSpan.FromMinutes(5)
            };

            using var client = new WebDavClient(clientParams);
            var response = await client.GetRawFile(SelectedFile.RelativePath);

            if (response.IsSuccessful)
            {
                await using var destinationStream = File.Create(destinationPath);
                await response.Stream.CopyToAsync(destinationStream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DragDrop Error] Download failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper method for Drag &amp; Drop: Uploads a file from local path directly to the phone.
    /// </summary>
    public async Task UploadFileFromPathAsync(string localFilePath)
    {
        if (!File.Exists(localFilePath)) return;

        try
        {
            string currentIp = await GetDeviceIpAsync();
            if (string.IsNullOrEmpty(currentIp)) return;

            var clientParams = new WebDavClientParams
            {
                BaseAddress = new Uri($"http://{currentIp}:5001/"),
                Timeout = TimeSpan.FromMinutes(5)
            };

            using var client = new WebDavClient(clientParams);
            string fileName = Path.GetFileName(localFilePath);
            string remotePath = (CurrentPath.EndsWith("/") ? CurrentPath : CurrentPath + "/") + fileName;

            await using var readStream = File.OpenRead(localFilePath);
            var response = await client.PutFile(remotePath, readStream);

            if (response.IsSuccessful)
            {
                Refresh();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DragDrop Error] Upload failure: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigates one directory level up in the hierarchy.
    /// </summary>
    [RelayCommand]
    private void NavigateUp()
    {
        if (CurrentPath == "/" || string.IsNullOrEmpty(CurrentPath)) 
            return;
        
        var parts = CurrentPath.TrimEnd('/').Split('/');
        if (parts.Length <= 1)
        {
            _ = LoadFilesAsync("/");
            return;
        }
        
        var newPath = string.Join("/", parts, 0, parts.Length - 1);
        _ = LoadFilesAsync(string.IsNullOrEmpty(newPath) ? "/" : newPath);
    }

    /// <summary>
    /// Refreshes the contents of the current directory.
    /// </summary>
    [RelayCommand]
    private void Refresh()
    {
        _ = LoadFilesAsync(CurrentPath);
    }

    /// <summary>
    /// Handles item selection. Navigates into selected directories automatically.
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
            Console.WriteLine($"[WebDAV] Selected file: {value.Name} ({value.FormattedSize})");
        }
    }
}