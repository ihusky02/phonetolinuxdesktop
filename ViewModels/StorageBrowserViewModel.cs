using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using phonetolinux.Models;

namespace phonetolinux.ViewModels;

public partial class StorageBrowserViewModel : ViewModelBase
{
    private readonly HttpClient _httpClient = new();
    
    // TODO: Eventually, this address will be fetched from DevicePairingService
    private string _deviceIp = "192.168.100.90"; 

    [ObservableProperty]
    private string _currentPath = "/";

    [ObservableProperty]
    private ObservableCollection<PhoneFileItem> _files = new();

    [ObservableProperty]
    private PhoneFileItem? _selectedFile;

    public StorageBrowserViewModel()
    {
        // Load the main directory of the phone on startup
        _ = LoadFilesAsync("");
    }

    [RelayCommand]
    private async Task LoadFilesAsync(string path)
    {
        try
        {
            CurrentPath = string.IsNullOrEmpty(path) ? "/" : path;
            
            // Call our Android endpoint
            string url = $"http://{_deviceIp}:5000/storage/list?path={Uri.EscapeDataString(path)}";
            var response = await _httpClient.GetStringAsync(url);
            
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var items = JsonSerializer.Deserialize<PhoneFileItem[]>(response, options);

            Files.Clear();
            if (items != null)
            {
                foreach (var item in items)
                {
                    Files.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StorageBrowser] Failed to fetch files: {ex.Message}");
        }
    }

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

    [RelayCommand]
    private void Refresh()
    {
        _ = LoadFilesAsync(CurrentPath == "/" ? "" : CurrentPath);
    }

    // Allows navigating into folders upon clicking
    partial void OnSelectedFileChanged(PhoneFileItem? value)
    {
        if (value != null && value.IsDirectory)
        {
            _ = LoadFilesAsync(value.RelativePath);
            SelectedFile = null; // Deselect after entering
        }
    }
}