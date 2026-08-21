using System;
using System.IO;
using System.Threading;

namespace PhoneToLinux.Security
{
    /// <summary>
    /// Implements Moving Target Defense pattern by periodically changing 
    /// the physical directory location of encrypted local data.
    /// </summary>
    public class DynamicFolderManager
    {
        private readonly string _baseStorageDirectory;
        private string _currentSecureFolder;
        private Timer? _relocationTimer;

        public string CurrentSecureFolder => _currentSecureFolder;

        public DynamicFolderManager(string baseAppDirectory)
        {
            _baseStorageDirectory = Path.Combine(baseAppDirectory, "SecureStorage");
            if (!Directory.Exists(_baseStorageDirectory))
            {
                Directory.CreateDirectory(_baseStorageDirectory);
            }

            _currentSecureFolder = GenerateRandomFolderPath();
            EnsureFolderExists(_currentSecureFolder);
        }

        /// <summary>
        /// Starts the background timer to periodically relocate data files.
        /// </summary>
        /// <param name="interval">Relocation interval (e.g., 1 hour).</param>
        public void StartPeriodicRelocation(TimeSpan interval)
        {
            _relocationTimer = new Timer(RelocateDataFolder, null, interval, interval);
        }

        private void RelocateDataFolder(object? state)
        {
            try
            {
                string oldFolder = _currentSecureFolder;
                string newFolder = GenerateRandomFolderPath();

                EnsureFolderExists(newFolder);

                if (Directory.Exists(oldFolder))
                {
                    // Move all encrypted payload files to the new random location
                    foreach (var filePath in Directory.GetFiles(oldFolder))
                    {
                        string fileName = Path.GetFileName(filePath);
                        string destinationPath = Path.Combine(newFolder, fileName);
                        File.Move(filePath, destinationPath, overwrite: true);
                    }

                    // Remove the old directory after successful relocation
                    Directory.Delete(oldFolder, recursive: true);
                }

                _currentSecureFolder = newFolder;
                Console.WriteLine($"[Security] Storage location updated to: {_currentSecureFolder}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Security Error] Storage relocation failed: {ex.Message}");
            }
        }

        private string GenerateRandomFolderPath()
        {
            string randomSubFolder = $"sec_{Guid.NewGuid():N}";
            return Path.Combine(_baseStorageDirectory, randomSubFolder);
        }

        private static void EnsureFolderExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}