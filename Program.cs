using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Avalonia;

namespace phonetolinux
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // --- AUTOMATYCZNA KOMPILACJA WSZYSTKICH WTYCZEK .cs DO LIBRARY ---
            CompileAllPluginSourcesToLibrary();
            // ----------------------------------------------------------------

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        private static void CompileAllPluginSourcesToLibrary()
        {
            try
            {
                string projectRoot = "/home/stanislaw/phonetolinux";
                string libraryDir = Path.Combine(projectRoot, "Library");
                string sourceDir = Path.Combine(projectRoot, "PluginSource");

                Directory.CreateDirectory(libraryDir);
                Directory.CreateDirectory(sourceDir);

                Console.WriteLine($"[LibrarySystem] Główny folder Library: {libraryDir}");
                Console.WriteLine($"[LibrarySystem] Folder źródłowy PluginSource: {sourceDir}");

                var csFiles = Directory.GetFiles(sourceDir, "*.cs");
                Console.WriteLine($"[LibrarySystem] Znaleziono plików .cs do skompilowania: {csFiles.Length}");

                if (csFiles.Length == 0) return;

                // Pobieramy referencje raz dla wszystkich kompilacji
                var baseReferences = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                    .Select(a => MetadataReference.CreateFromFile(a.Location))
                    .Cast<MetadataReference>()
                    .ToList();

                baseReferences.Add(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
                baseReferences.Add(MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location));

                // Kompilujemy każdy plik .cs osobno jako niezależną wtyczkę .dll
                foreach (var csFile in csFiles)
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(csFile);
                    string outputDllPath = Path.Combine(libraryDir, $"{fileNameWithoutExt}.dll");

                    var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(csFile));

                    var compilation = CSharpCompilation.Create(
                        fileNameWithoutExt,
                        new[] { syntaxTree },
                        baseReferences,
                        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    );

                    using (var fs = new FileStream(outputDllPath, FileMode.Create))
                    {
                        var result = compilation.Emit(fs);
                        if (result.Success)
                        {
                            Console.WriteLine($"[LibrarySystem] Sukces! Skompilowano wtyczkę -> {fileNameWithoutExt}.dll");
                        }
                        else
                        {
                            Console.WriteLine($"[LibrarySystem Błąd kompilacji dla {fileNameWithoutExt}.cs]:");
                            foreach (var diagnostic in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                            {
                                Console.WriteLine($" - {diagnostic.GetMessage()}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LibrarySystem Wyjątek]: {ex.Message}");
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}