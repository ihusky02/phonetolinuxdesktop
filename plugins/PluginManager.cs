using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PhoneToLinux.Core;

namespace PhoneToLinux.Desktop
{
    /// <summary>
    /// Odpowiada za skanowanie folderu Library, dynamiczne ładowanie bibliotek wtyczek (.dll)
    /// oraz kierowanie żądań przychodzących do odpowiednich handlerów.
    /// </summary>
    public class PluginManager
    {
        private readonly Dictionary<string, IPhonePlugin> _plugins = new();

        /// <summary>
        /// Przeszukuje wskazany katalog Library w poszukiwaniu plików wtyczek z rozszerzeniem .dll,
        /// a następnie rejestruje je w pamięci aplikacji.
        /// </summary>
        /// <param name="libraryDirectoryPath">Ścieżka do folderu Library z wtyczkami.</param>
        public void LoadPlugins(string libraryDirectoryPath)
        {
            if (!Directory.Exists(libraryDirectoryPath)) return;

            // Iterujemy przez wszystkie pliki .dll w folderze Library
            foreach (var file in Directory.GetFiles(libraryDirectoryPath, "*.dll"))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(file);
                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeof(IPhonePlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            if (Activator.CreateInstance(type) is IPhonePlugin plugin)
                            {
                                _plugins[plugin.Endpoint] = plugin;
                                Console.WriteLine($"[PluginManager Library] Załadowano wtyczkę dla endpointu: {plugin.Endpoint}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Błąd] Nie udało się załadować wtyczki z pliku {file}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Obsługuje zapytanie poprzez przekazanie go do odpowiedniej wtyczki na podstawie endpointu.
        /// </summary>
        public string ExecutePlugin(string endpoint, string queryParams)
        {
            if (_plugins.TryGetValue(endpoint, out var plugin))
            {
                return plugin.Execute(queryParams);
            }
            return "{\"error\":\"Plugin not found\"}";
        }
    }
}