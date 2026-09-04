using System;
using System.Diagnostics;
using PhoneToLinux.Core;

namespace phonetolinux.Services
{
    /// <summary>
    /// Plugin responsible for displaying native Linux desktop notifications (using notify-send)
    /// triggered by incoming phone events such as SMS messages or phone calls.
    /// </summary>
    public class LinuxNotificationPlugin : IPhonePlugin
    {
        /// <inheritdoc />
        public string Endpoint => "/notification";

        /// <summary>
        /// Executes the plugin operation for a given query parameter.
        /// </summary>
        /// <param name="queryParams">Query parameters.</param>
        /// <returns>Response in JSON format.</returns>
        public string Execute(string queryParams)
        {
            return $"{{\"status\":\"LinuxNotificationPlugin active\", \"query\":\"{queryParams}\"}}";
        }

        /// <summary>
        /// Displays a native desktop notification on the Linux system using notify-send.
        /// </summary>
        /// <param name="title">Notification title (e.g., sender name or phone number).</param>
        /// <param name="message">Notification body content (e.g., SMS text).</param>
        /// <param name="icon">Optional system icon name (e.g., "mail-unread", "call-start", "dialog-information").</param>
        /// <param name="urgency">Urgency level: "low", "normal", or "critical". Default is "normal".</param>
        public void ShowNotification(string title, string message, string icon = "dialog-information", string urgency = "normal")
        {
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                // Escape critical shell special characters to prevent arguments syntax errors or injection
                string safeTitle = SanitizeShellArgument(title);
                string safeMessage = SanitizeShellArgument(message);
                string safeIcon = SanitizeShellArgument(icon);
                string safeUrgency = SanitizeShellArgument(urgency);

                // Build argument string for notify-send
                string arguments = $"--app-name=\"PhoneToLinux\" --icon=\"{safeIcon}\" --urgency=\"{safeUrgency}\" \"{safeTitle}\" \"{safeMessage}\"";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "notify-send",
                    Arguments = arguments,
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

        /// <summary>
        /// Sanitizes text strings to be safely passed as command-line arguments to shell utilities.
        /// </summary>
        private static string SanitizeShellArgument(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("$", "\\$")
                .Replace("`", "\\`");
        }
    }
}