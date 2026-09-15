using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace phonetolinux.Services;

/// <summary>
/// Dynamic plugin loader supporting runtime Roslyn compilation for C# source files 
/// and direct assembly loading for precompiled binaries.
/// </summary>
public class DnnPluginLoader
{
    /// <summary>
    /// Compiles or loads a plugin file and executes the specified target method.
    /// </summary>
    /// <param name="dnnFilePath">Path to the C# source script (.cs) or assembly binary (.dll).</param>
    /// <param name="className">Target class name implementing the plugin logic.</param>
    /// <param name="methodName">Target method name to execute.</param>
    /// <param name="contextData">Context payload passed as an argument to the method.</param>
    /// <returns>Execution result object or null on failure.</returns>
    public static object? ExecutePlugin(string dnnFilePath, string className, string methodName, object contextData)
    {
        try
        {
            if (!File.Exists(dnnFilePath))
            {
                Console.WriteLine($"[DNN Error] Plugin file not found: {dnnFilePath}");
                return null;
            }

            string codeContent;

            // Attempt to read as raw source code text or fall back to loading compiled DLL assembly
            try
            {
                codeContent = File.ReadAllText(dnnFilePath);
            }
            catch
            {
                // Fallback: direct reflection invocation for precompiled binaries (.dll)
                Assembly asm = Assembly.LoadFrom(dnnFilePath);
                Type? t = asm.GetTypes().FirstOrDefault(x => x.Name == className || x.FullName == className);
                MethodInfo? m = t?.GetMethod(methodName);
                object? inst = t != null ? Activator.CreateInstance(t) : null;
                return m?.Invoke(inst, new object[] { contextData });
            }

            // On-the-fly compilation using Roslyn compiler directly from source file contents
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
                Console.WriteLine("[DNN Compilation Error] Failed to compile plugin script:");
                foreach (var diagnostic in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                {
                    Console.WriteLine($" - {diagnostic.GetMessage()}");
                }
                return null;
            }

            // Load generated assembly in-memory and invoke target method with context payload
            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(ms.ToArray());

            Type? type = assembly.GetTypes().FirstOrDefault(t => t.Name == className || t.FullName == className);
            if (type == null)
            {
                Console.WriteLine($"[DNN Error] Class '{className}' not found in plugin");
                return null;
            }

            MethodInfo? method = type.GetMethod(methodName);
            if (method == null)
            {
                Console.WriteLine($"[DNN Error] Method '{methodName}' not found in class '{className}'");
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