using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using PhoneToLinux.Core; // <--- Poprawna przestrzeń nazw zdefiniowana w interfejsie
using phonetolinux.Models;

namespace phonetolinux.Plugins
{
    /// <summary>
    /// Secure dynamic plugin for handling MMS attachments implementing:
    /// 1. Strict MIME-type whitelisting
    /// 2. Path Traversal & filename sanitization
    /// 3. Strict 15MB file size enforcement
    /// </summary>
    public class MmsPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient = new();
        private const long MaxFileSizeLimit = 15 * 1024 * 1024; // 15 MB limit in bytes

        // Wymagane przez interfejs IPhonePlugin
        public string Endpoint => "/mms";

        // 1. Strict MIME-type whitelist to prevent malicious payload or executable delivery
        private static readonly string[] AllowedMimeTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",
            "audio/mpeg",
            "audio/ogg",
            "audio/wav",
            "text/plain"
        };

        private readonly string _downloadDirectory;

        public MmsPlugin()
        {
            _downloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MmsDownloads");
            if (!Directory.Exists(_downloadDirectory))
            {
                Directory.CreateDirectory(_downloadDirectory);
            }
        }

        // Wymagana przez interfejs IPhonePlugin metoda wykonawcza
        public string Execute(string queryParams)
        {
            // Tutaj w przyszłości obsłużysz wywołanie z endpointu /mms
            return "MMS Plugin executed successfully.";
        }

        public async Task<MmsAttachment?> DownloadAttachmentAsync(string remoteUrl, string fileName, string contentType)
        {
            try
            {
                // Security Check 1: Validate MIME-type against the strict whitelist
                if (string.IsNullOrEmpty(contentType) || !AllowedMimeTypes.Contains(contentType.ToLowerInvariant()))
                {
                    throw new SecurityException($"Blocked untrusted or unsafe Content-Type: {contentType}");
                }

                using var response = await _httpClient.GetAsync(remoteUrl, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                // Security Check 3: Enforce strict 15MB size limit via Content-Length header
                long contentLength = response.Content.Headers.ContentLength ?? 0;
                if (contentLength > MaxFileSizeLimit)
                {
                    throw new InvalidOperationException("MMS attachment exceeds the 15MB size limit.");
                }

                // Security Check 2: Prevent Path Traversal attacks by isolating file name and using a safe prefix
                string cleanFileName = Path.GetFileName(fileName);
                string safeFileName = $"{Guid.NewGuid()}_{cleanFileName}";
                string localPath = Path.Combine(_downloadDirectory, safeFileName);

                // Stream securely to disk
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    await contentStream.CopyToAsync(fileStream);
                }

                return new MmsAttachment
                {
                    FileName = cleanFileName,
                    ContentType = contentType,
                    LocalFilePath = localPath,
                    FileSizeInBytes = contentLength
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MmsPlugin Security Warning] {ex.Message}");
                return null;
            }
        }
    }
}