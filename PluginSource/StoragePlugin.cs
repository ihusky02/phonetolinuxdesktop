using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using phonetolinux.Plugins;

namespace phonetolinux.PluginSource
{
    /// <summary>
    /// Storage integration plugin implementation for desktop file manager mounting and bookmarks.
    /// Implements full IPhonetolinuxPlugin contract.
    /// </summary>
    public class StoragePlugin : IPhonetolinuxPlugin
    {
        private const string BookmarkDisplayName = "PhoneToLinux Storage";

        public string Name => "StoragePlugin";
        public string Version => "1.0.0";

        // Required interface events with Action delegates
        public event Action<string, string>? OnCallReceived;
        public event Action? OnCallEnded;
        public event Action<string, string>? OnSmsReceived;

        public void Initialize(string ip, int port)
        {
            OnDeviceConnected(ip);
        }

        public void Shutdown()
        {
            // Optional cleanup on plugin unload
        }

        public Task<bool> AnswerCallAsync() => Task.FromResult(false);
        public Task<bool> RejectCallAsync() => Task.FromResult(false);

        /// <summary>
        /// Triggered when phone connects/pairs with desktop application.
        /// Compatible with DnnPluginLoader single-string parameter signature.
        /// </summary>
        public bool OnDeviceConnected(string phoneIp)
        {
            int port = 5000;
            string webdavUri = $"dav://{phoneIp}:{port}/";

            // 1. Mount network location using GVfs
            bool mounted = ExecuteCommand("gio", $"mount \"{webdavUri}\"");

            // 2. Register GTK bookmark (Nautilus, Thunar, Nemo)
            AddGtk3Bookmark(webdavUri, BookmarkDisplayName);

            // 3. Register KDE Plasma bookmark (Dolphin)
            AddKdePlaceBookmark(webdavUri, BookmarkDisplayName);

            return mounted;
        }

        /// <summary>
        /// Triggered when phone disconnects.
        /// </summary>
        public bool OnDeviceDisconnected(string phoneIp)
        {
            int port = 5000;
            string webdavUri = $"dav://{phoneIp}:{port}/";

            // 1. Remove GTK bookmark entry
            RemoveGtk3Bookmark(webdavUri);

            // 2. Unmount GVfs network location
            return ExecuteCommand("gio", $"mount -u \"{webdavUri}\"");
        }

        #region GTK & KDE Bookmarks Management

        private static void AddGtk3Bookmark(string uri, string label)
        {
            try
            {
                string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "gtk-3.0");
                Directory.CreateDirectory(configDir);

                string bookmarksFile = Path.Combine(configDir, "bookmarks");
                string bookmarkEntry = $"{uri} {label}";

                if (File.Exists(bookmarksFile))
                {
                    var lines = File.ReadAllLines(bookmarksFile).ToList();
                    if (!lines.Any(line => line.StartsWith(uri)))
                    {
                        lines.Add(bookmarkEntry);
                        File.WriteAllLines(bookmarksFile, lines);
                    }
                }
                else
                {
                    File.WriteAllLines(bookmarksFile, new[] { bookmarkEntry });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StoragePlugin] Failed to add GTK bookmark: {ex.Message}");
            }
        }

        private static void RemoveGtk3Bookmark(string uri)
        {
            try
            {
                string bookmarksFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "gtk-3.0", "bookmarks");
                if (File.Exists(bookmarksFile))
                {
                    var lines = File.ReadAllLines(bookmarksFile).Where(line => !line.StartsWith(uri)).ToList();
                    File.WriteAllLines(bookmarksFile, lines);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StoragePlugin] Failed to remove GTK bookmark: {ex.Message}");
            }
        }

        private static void AddKdePlaceBookmark(string uri, string label)
        {
            try
            {
                string xbelPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "user-places.xbel");
                if (!File.Exists(xbelPath)) return;

                var doc = XDocument.Load(xbelPath);
                var root = doc.Root;
                if (root == null) return;

                bool exists = root.Elements("bookmark").Any(b => b.Attribute("href")?.Value == uri);
                if (!exists)
                {
                    var newBookmark = new XElement("bookmark",
                        new XAttribute("href", uri),
                        new XElement("title", label),
                        new XElement("info",
                            new XElement("metadata",
                                new XAttribute("owner", "http://freedesktop.org"),
                                new XElement("bookmark:icon", new XAttribute("name", "phone"))
                            )
                        )
                    );
                    root.Add(newBookmark);
                    doc.Save(xbelPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StoragePlugin] Failed to update KDE places: {ex.Message}");
            }
        }

        #endregion

        #region Command Execution Helper

        private static bool ExecuteCommand(string command, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                process?.WaitForExit(2000);
                return process?.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}