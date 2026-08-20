namespace phonetolinux.Models
{
    /// <summary>
    /// Represents an MMS attachment (image, audio, or document) with its metadata.
    /// </summary>
    public class MmsAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty; // e.g., "image/jpeg", "audio/mp3", "text/plain"
        public string LocalFilePath { get; set; } = string.Empty; // Local path where the file is saved on the PC
        public long FileSizeInBytes { get; set; }
    }
}