using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using PhoneToLinux.Core;
using phonetolinux.Models;

namespace phonetolinux.Plugins
{
    /// <summary>
    /// Secure dynamic plugin for handling MMS attachments implementing:
    /// 1. Strict MIME-type whitelisting
    /// 2. Path Traversal protection & filename sanitization
    /// 3. Strict 15MB file size enforcement (header check and stream-level guard)
    /// </summary>
    public class MmsPlugin : IPhonePlugin
    {
        private readonly HttpClient _httpClient;
        private const long MaxFileSizeLimit = 15 * 1024 * 1024; // 15 MB limit in bytes

        /// <inheritdoc />
        public string Endpoint => "/mms";

        // Strict MIME-type whitelist to prevent malicious payload or executable delivery
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MmsPlugin"/> class.
        /// </summary>
        /// <param name="httpClient">Optional custom HttpClient instance. Uses a default client if none provided.</param>
        public MmsPlugin(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _downloadDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                ".phonetolinux", 
                "mms_downloads"
            );

            if (!Directory.Exists(_downloadDirectory))
            {
                Directory.CreateDirectory(_downloadDirectory);
            }
        }

        /// <summary>
        /// Execution method required by the <see cref="IPhonePlugin"/> interface.
        /// </summary>
        /// <param name="queryParams">Query parameters.</param>
        /// <returns>Response in JSON format.</returns>
        public string Execute(string queryParams)
        {
            return $"{{\"status\":\"MmsPlugin active\", \"query\":\"{queryParams}\"}}";
        }

        /// <summary>
        /// Asynchronously downloads an MMS attachment from the remote phone server.
        /// </summary>
        /// <param name="remoteUrl">Remote attachment URL.</param>
        /// <param name="fileName">Original file name.</param>
        /// <param name="contentType">MIME Content-Type of the attachment.</param>
        /// <returns>An instance of <see cref="MmsAttachment"/> if successful; otherwise, null.</returns>
        public async Task<MmsAttachment?> DownloadAttachmentAsync(string remoteUrl, string fileName, string contentType)
        {
            if (string.IsNullOrWhiteSpace(remoteUrl))
                return null;

            try
            {
                // Security Check 1: Validate MIME-type against strict whitelist
                string normalizedType = contentType?.Trim().ToLowerInvariant() ?? "";
                if (string.IsNullOrEmpty(normalizedType) || !AllowedMimeTypes.Contains(normalizedType))
                {
                    throw new SecurityException($"Blocked untrusted or unsafe Content-Type: {contentType}");
                }

                using var response = await _httpClient.GetAsync(remoteUrl, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                // Security Check 2: Enforce strict 15MB size limit via Content-Length header when available
                long contentLength = response.Content.Headers.ContentLength ?? 0;
                if (contentLength > MaxFileSizeLimit)
                {
                    throw new InvalidOperationException("MMS attachment exceeds the 15MB size limit.");
                }

                // Security Check 3: Prevent Path Traversal by cleaning filename and ensuring destination directory sandbox
                string rawFileName = Path.GetFileName(fileName);
                string cleanFileName = string.Join("_", rawFileName.Split(Path.GetInvalidFileNameChars()));
                if (string.IsNullOrWhiteSpace(cleanFileName))
                {
                    cleanFileName = "attachment.bin";
                }

                string safeFileName = $"{Guid.NewGuid()}_{cleanFileName}";
                string localPath = Path.Combine(_downloadDirectory, safeFileName);

                // Ensure the path strictly resides within the sandbox directory
                string fullPath = Path.GetFullPath(localPath);
                if (!fullPath.StartsWith(Path.GetFullPath(_downloadDirectory), StringComparison.OrdinalIgnoreCase))
                {
                    throw new SecurityException("Path Traversal detected in attachment target path.");
                }

                // Stream securely to disk with active size guard
                long totalBytesRead = 0;
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        totalBytesRead += bytesRead;
                        if (totalBytesRead > MaxFileSizeLimit)
                        {
                            fileStream.Close();
                            File.Delete(fullPath); // Clean up partially downloaded file
                            throw new InvalidOperationException("MMS attachment stream exceeded the 15MB size limit.");
                        }

                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                    }
                }

                return new MmsAttachment
                {
                    FileName = cleanFileName,
                    ContentType = normalizedType,
                    LocalFilePath = fullPath,
                    FileSizeInBytes = totalBytesRead
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