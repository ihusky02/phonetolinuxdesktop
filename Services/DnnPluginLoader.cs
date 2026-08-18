using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace phonetolinux.Services;

public class DnnPluginLoader
{
    public static object? ExecutePlugin(string dnnFilePath, string className, string methodName, object contextData)
    {
        try
        {
            if (!File.Exists(dnnFilePath))
            {
                Console.WriteLine($"[DNN Error] Nie znaleziono pliku wtyczki: {dnnFilePath}");
                return null;
            }

            string extractPath = Path.Combine(Path.GetTempPath(), "phonetolinux_plugins", Path.GetFileNameWithoutExtension(dnnFilePath));

            // 1. Rozpakowanie pliku .dnn (ZIP) do katalogu tymczasowego
            if (Directory.Exists(extractPath)) Directory.Exists(extractPath);
            Directory.CreateDirectory(extractPath);
            ZipFile.ExtractToDirectory(dnnFilePath, extractPath, overwriteFiles: true);

            // 2. Znalezienie pliku kodu źródłowego C# wewnątrz paczki
            string sourceFile = Directory.GetFiles(extractPath, "*.cs", SearchOption.AllDirectories).FirstOrDefault();
            if (sourceFile == null)
            {
                Console.WriteLine("[DNN Error] Brak pliku kodu źródłowego .cs wewnątrz paczki .dnn");
                return null;
            }

            string codeContent = File.ReadAllText(sourceFile);

            // 3. Kompilacja w locie za pomocą Roslyn
            var syntaxTree = CSharpSyntaxTree.ParseText(codeContent);
            
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .Cast<MetadataReference>()
                .ToList();

            var compilation = CSharpCompilation.Create(
                assemblyName: "DnnDynamicPlugin_" + Guid.NewGuid().ToString("N"),
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                Console.WriteLine("[DNN Compilation Error] Błędy kompilacji skryptu wtyczki:");
                foreach (var diagnostic in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                {
                    Console.WriteLine($" - {diagnostic.GetMessage()}");
                }
                return null;
            }

            // 4. Załadowanie zestawu i wywołanie metody z obiektem kontekstu
            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(ms.ToArray());

            Type? type = assembly.GetTypes().FirstOrDefault(t => t.Name == className || t.FullName == className);
            if (type == null)
            {
                Console.WriteLine($"[DNN Error] Nie znaleziono klasy '{className}' w wtyczce");
                return null;
            }

            MethodInfo? method = type.GetMethod(methodName);
            if (method == null)
            {
                Console.WriteLine($"[DNN Error] Nie znaleziono metody '{methodName}' w klasie '{className}'");
                return null;
            }

            object? instance = Activator.CreateInstance(type);
            return method.Invoke(instance, new object[] { contextData });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DNN Exception]: {ex.Message}");
            return null;
        }
    }
}