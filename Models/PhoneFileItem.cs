using System;

namespace phonetolinux.Models;

public class PhoneFileItem
{
    public bool IsDirectory { get; set; }
    public long LastModified { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }

    // UI Helpers (Icons and Colors)
    public string FormattedSize => IsDirectory ? "" : FormatBytes(SizeBytes);
    
    // Folder icon or File icon
    public string IconPath => IsDirectory 
        ? "M10 4H4C2.9 4 2.01 4.9 2.01 6L2 18C2 19.1 2.9 20 4 20H20C21.1 20 22 19.1 22 18V8C22 6.9 21.1 6 20 6H12L10 4Z" 
        : "M14 2H6C4.9 2 4 2.9 4 4V20C4 21.1 4.9 22 6 22H18C19.1 22 20 21.1 20 20V8L14 2M13 9V3.5L18.5 9H13Z";
        
    public string IconColor => IsDirectory ? "#FFA000" : "#B0BEC5";

    private string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = (decimal)bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return string.Format("{0:n1} {1}", number, suffixes[counter]);
    }
}