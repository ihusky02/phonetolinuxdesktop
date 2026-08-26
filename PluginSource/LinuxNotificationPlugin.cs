using System;
using System.Diagnostics;
using PhoneToLinux.Core;

namespace phonetolinux.Services
{
    /// <summary>
    /// Plugin responsible for displaying native Linux desktop notifications (using notify-send)
    /// triggered by incoming phone events like SMS or calls.
    /// </summary>
    public class LinuxNotificationPlugin : IPhonePlugin
    {
        /// <inheritdoc />
        public string Endpoint => "/notification";

        /// <summary>
        /// Executes the plugin operation for a given query.
        /// </summary>
        /// <param name="queryParams">Query parameters.</param>
        /// <returns>Response in JSON format.</returns>
        public string Execute(string queryParams)
        {
            return "{\"status\":\"LinuxNotificationPlugin active\"}";
        }

        /// <summary>
        /// Displays a native desktop notification on the Linux system.
        /// </summary>
        /// <param name="title">Notification title (e.g. sender name).</param>
        /// <param name="message">Notification body (e.g. SMS text).</param>
        public void ShowNotification(string title, string message)
        {
            try
            {
                // Escape quotes to prevent command injection issues
                string safeTitle = title.Replace("\"", "\\\"");
                string safeMessage = message.Replace("\"", "\\\"");

                // Using 'notify-send' which is standard across Linux desktop environments (Cinnamon, GNOME, XFCE, etc.)
                var startInfo = new ProcessStartInfo
                {
                    FileName = "notify-send",
                    Arguments = $"--app-name=\"PhoneToLinux\" \"{safeTitle}\" \"{safeMessage}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                process?.WaitForExit();

                Console.WriteLine($"[NOTIFICATION] System notification sent: {title} - {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTIFICATION ERROR] Failed to display system notification: {ex.Message}");
            }
        }
    }
}