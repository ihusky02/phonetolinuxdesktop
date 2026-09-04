using System;
using System.IO;
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

            // Pobieramy kod źródłowy bezpośrednio z pliku (obsługuje pliki .cs lub pliki źródłowe przekazane jako ścieżka)
            string codeContent;
            
            // Jeśli plik to faktycznie archiwum ZIP (stare .dnn), możemy zachować fallback, 
            // ale domyślnie czytamy jako tekst/kod C# lub binarkę
            try
            {
                codeContent = File.ReadAllText(dnnFilePath);
            }
            catch
            {
                // Jeśli nie da się odczytać jako tekst, próbujemy załadować jako zwykłą bibliotekę DLL przez refleksję
                Assembly asm = Assembly.LoadFrom(dnnFilePath);
                Type? t = asm.GetTypes().FirstOrDefault(x => x.Name == className || x.FullName == className);
                MethodInfo? m = t?.GetMethod(methodName);
                object? inst = t != null ? Activator.CreateInstance(t) : null;
                return m?.Invoke(inst, new object[] { contextData });
            }

            // Kompilacja w locie za pomocą Roslyn bezpośrednio z zawartości pliku
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

            // Załadowanie zestawu i wywołanie metody z obiektem kontekstu
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