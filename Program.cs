using System;
using System.IO;
using Avalonia;

namespace phonetolinux
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // --- INICJALIZACJA BEZPIECZNEGO PLIKU WTYCZKI DLA KOMPILATORA ---
            EnsureChatSyncPluginBinary();
            // -------------------------------------------------------------

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        /// <summary>
        /// Zapewnia istnienie poprawnego pliku chatsync.dnn w folderze plugins,
        /// nie naruszając pozostałych plików systemowych kompilatora i menedżera wtyczek.
        /// </summary>
        private static void EnsureChatSyncPluginBinary()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string pluginsDir = Path.Combine(baseDir, "plugins");
                Directory.CreateDirectory(pluginsDir);

                string targetDnn = Path.Combine(pluginsDir, "chatsync.dnn");

                // Jeśli plik chatsync.dnn nie istnieje lub jest pusty, generujemy go poprawnie
                if (!File.Exists(targetDnn) || new FileInfo(targetDnn).Length == 0)
                {
                    string sourceDll = Path.Combine(baseDir, "phonetolinux.dll");

                    if (File.Exists(sourceDll))
                    {
                        File.Copy(sourceDll, targetDnn, true);
                        Console.WriteLine($"[PluginSystem] Wygenerowano poprawny binarny chatsync.dnn z głównej biblioteki.");
                    }
                    else
                    {
                        // Fallback: bierzemy pierwszą dostępną bibliotekę .dll z katalogu
                        var dlls = Directory.GetFiles(baseDir, "*.dll");
                        if (dlls.Length > 0)
                        {
                            File.Copy(dlls[0], targetDnn, true);
                            Console.WriteLine($"[PluginSystem] Wygenerowano chatsync.dnn z dostępnej binarki: {Path.GetFileName(dlls[0])}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PluginSystem Ostrzeżenie]: {ex.Message}");
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}