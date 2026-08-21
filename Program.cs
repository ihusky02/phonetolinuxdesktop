using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Avalonia;
using PhoneToLinux.Security;

namespace phonetolinux
{
    /// <summary>
    /// Entry point of the application. Responsible for initializing secure local storage mechanisms,
    /// automatically compiling plugin source files from the PluginSource directory into dynamic .dll libraries,
    /// and starting the Avalonia desktop UI lifecycle.
    /// </summary>
    class Program
    {
        public static DynamicFolderManager? FolderManager { get; private set; }
        public static SecureStorageService? StorageService { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            // --- 1. INITIALIZE MOVING TARGET DEFENSE & SECURE STORAGE ---
            InitializeSecurityFramework();
            // ------------------------------------------------------------

            // --- 2. AUTOMATICALLY COMPILE ALL PLUGIN SOURCES TO LIBRARY AT STARTUP ---
            CompileAllPluginSourcesToLibrary();
            // --------------------------------------------------------------------

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        private static void InitializeSecurityFramework()
        {
            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Instantiate DynamicFolderManager and start hourly directory relocation
                FolderManager = new DynamicFolderManager(baseDirectory);
                FolderManager.StartPeriodicRelocation(TimeSpan.FromHours(1));

                // Generate a 256-bit AES master key for local session payload encryption
                byte[] masterKey = RandomNumberGenerator.GetBytes(32);
                StorageService = new SecureStorageService(masterKey);

                Console.WriteLine($"[Security] Dynamic storage initialized at: {FolderManager.CurrentSecureFolder}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Security Critical Error] Failed to initialize security framework: {ex.Message}");
            }
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

                Console.WriteLine($"[LibrarySystem] Main Library directory: {libraryDir}");
                Console.WriteLine($"[LibrarySystem] Source PluginSource directory: {sourceDir}");

                var csFiles = Directory.GetFiles(sourceDir, "*.cs", SearchOption.TopDirectoryOnly);
                Console.WriteLine($"[LibrarySystem] Found .cs files to compile: {csFiles.Length}");

                if (csFiles.Length == 0) return;

                // Retrieve base assembly references from the application domain
                var baseReferences = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                    .Select(a => MetadataReference.CreateFromFile(a.Location))
                    .Cast<MetadataReference>()
                    .ToList();

                // Explicitly include required system assemblies and packages for Roslyn compilation
                var requiredAssemblies = new[]
                {
                    typeof(object).Assembly,             // System.Runtime
                    typeof(Enumerable).Assembly,         // System.Linq
                    typeof(Uri).Assembly,                // System.Private.Uri
                    typeof(HttpClient).Assembly,         // System.Net.Http
                    typeof(HttpStatusCode).Assembly,     // System.Net.Primitives
                    typeof(JsonSerializer).Assembly,     // System.Text.Json
                    typeof(Process).Assembly             // System.Diagnostics.Process
                };

                foreach (var asm in requiredAssemblies)
                {
                    if (!string.IsNullOrEmpty(asm.Location))
                    {
                        var refPath = MetadataReference.CreateFromFile(asm.Location);
                        if (!baseReferences.Any(r => r.Display == refPath.Display))
                        {
                            baseReferences.Add(refPath);
                        }
                    }
                }

                // Compile each .cs file separately as an independent plugin .dll
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
                            Console.WriteLine($"[LibrarySystem] Success! Compiled plugin -> {fileNameWithoutExt}.dll");
                        }
                        else
                        {
                            Console.WriteLine($"[LibrarySystem Compilation error for {fileNameWithoutExt}.cs]:");
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
                Console.WriteLine($"[LibrarySystem Exception]: {ex.Message}");
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}