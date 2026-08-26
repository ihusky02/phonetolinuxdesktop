using System;
using System.IO;

namespace PhoneToLinux.Compiler
{
    /// <summary>
    /// Narzędzie kompilatora odpowiedzialne za automatyczne przenoszenie skompilowanych 
    /// bibliotek wtyczek (.dll) do głównego folderu Library w projekcie.
    /// </summary>
    public class CompilerArchiveModifier
    {
        private static FileSystemWatcher? _watcher;

        /// <summary>
        /// Uruchamia monitorowanie folderu wyjściowego kompilatora w tle. 
        /// Gdy pojawia się nowa biblioteka .dll, automatycznie kopiuje ją do folderu Library.
        /// </summary>
        /// <param name="buildOutputDir">Folder, do którego kompilator zapisuje pliki .dll (np. bin/Debug/net8.0).</param>
        /// <param name="libraryTargetDir">Katalog docelowy Library w głównym folderze projektu.</param>
        public static void StartSilentWatcher(string buildOutputDir, string libraryTargetDir)
        {
            if (!Directory.Exists(buildOutputDir)) Directory.CreateDirectory(buildOutputDir);
            if (!Directory.Exists(libraryTargetDir)) Directory.CreateDirectory(libraryTargetDir);

            // Jeśli watcher już działał, wyłączamy go przed utworzeniem nowego
            _watcher?.Dispose();

            _watcher = new FileSystemWatcher(buildOutputDir)
            {
                Filter = "*.dll",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            _watcher.Created += (sender, e) =>
            {
                // Krótkie opóźnienie, aby kompilator zdążył zwolnić plik DLL
                System.Threading.Thread.Sleep(1000);
                CopyDllToLibrary(e.FullPath, libraryTargetDir);
            };

            _watcher.EnableRaisingEvents = true;
            Console.WriteLine($"[Cichy Obserwator Library] Monitorowanie aktywne dla folderu: {buildOutputDir}");
        }

        /// <summary>
        /// Kopiuje skompilowaną bibliotekę DLL bezpośrednio do folderu Library.
        /// </summary>
        private static void CopyDllToLibrary(string dllPath, string libraryDir)
        {
            try
            {
                if (!File.Exists(dllPath)) return;

                string fileName = Path.GetFileName(dllPath);
                string targetPath = Path.Combine(libraryDir, fileName);

                File.Copy(dllPath, targetPath, overwrite: true);
                Console.WriteLine($"[Cichy Obserwator Library] Sukces: Przeniesiono do Library -> {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Błąd Obserwatora Library]: {ex.Message}");
            }
        }
    }
}