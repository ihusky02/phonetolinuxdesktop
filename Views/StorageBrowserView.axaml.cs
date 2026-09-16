using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Linq;
using phonetolinux.ViewModels;

namespace phonetolinux.Views;

public partial class StorageBrowserView : UserControl
{
    public StorageBrowserView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private async void DataGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (!point.Properties.IsLeftButtonPressed) return;

        if (DataContext is StorageBrowserViewModel vm && vm.SelectedFile != null && !vm.SelectedFile.IsDirectory)
        {
            var data = new DataObject();
            
            string tempFolder = Path.Combine(Path.GetTempPath(), "phonetolinux_drag");
            Directory.CreateDirectory(tempFolder);
            string tempFilePath = Path.Combine(tempFolder, vm.SelectedFile.Name);

            await vm.DownloadSelectedFileToPathAsync(tempFilePath);

            if (File.Exists(tempFilePath))
            {
                data.Set(DataFormats.Files, new[] { tempFilePath });
                await DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
            }
        }
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        var files = e.Data.GetFiles()?.ToList();
        if (files != null && files.Count > 0 && DataContext is StorageBrowserViewModel vm)
        {
            foreach (IStorageItem file in files)
            {
                if (file.TryGetLocalPath() is string localPath)
                {
                    await vm.UploadFileFromPathAsync(localPath);
                }
            }
        }
    }
}