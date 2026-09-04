using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PhoneToLinux.Core;

namespace PhoneToLinux.Desktop
{
    /// <summary>
    /// Scans the specified directory, dynamically loads plugin libraries (.dll),
    /// and routes incoming requests to the appropriate plugin handlers.
    /// </summary>
    public class PluginManager
    {
        private readonly Dictionary<string, IPhonePlugin> _plugins = new();

        /// <summary>
        /// Searches the specified library directory for plugin files with the .dll extension
        /// and registers them in application memory.
        /// Ignores subdirectories to prevent interference with secure storage paths.
        /// </summary>
        /// <param name="libraryDirectoryPath">Path to the Library directory containing plugins.</param>
        public void LoadPlugins(string libraryDirectoryPath)
        {
            if (!Directory.Exists(libraryDirectoryPath)) return;

            // Restrict file search to the top directory only to isolate from subfolders/SecureStorage
            foreach (var file in Directory.GetFiles(libraryDirectoryPath, "*.dll", SearchOption.TopDirectoryOnly))
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
                                Console.WriteLine($"[PluginManager Library] Loaded plugin for endpoint: {plugin.Endpoint}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Failed to load plugin from file {file}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles incoming queries by routing them to the corresponding plugin based on the endpoint.
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