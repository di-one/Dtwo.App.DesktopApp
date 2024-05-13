using Dtwo.API;
using Dtwo.API.Dofus2.AnkamaGames.Network.Messages;
using Dtwo.API.View.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dtwo.App.DesktopApp.ComponentsProvider
{
    internal class ComponentProviderLoader
    {
        private static readonly string THEMES_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "themes");
        private static HashSet<string> m_loadedDependencies = new HashSet<string>();

        public static ComponentsProviderSettings? LoadComponentsProvider(string name, out Assembly? asm)
        {
            string path = name;
            asm = null;

            if (string.IsNullOrEmpty(name) || ThemeExists(name) == false)
            {
                LogManager.LogError($"Theme {name} does not exist", 1);

                string? firstName;
                string? firstPath = GetFirstThemePath(out firstName);

                if (firstPath == null || firstName == null)
                {
                    LogManager.LogError("No theme found", 1);
                    return null;
                }

                path = firstPath;
                name = firstName;
            }

            string asmPath = Path.Combine(path, $"{name}.dll");

            try
            {
                asm = Assembly.LoadFrom(asmPath);

                if (asm == null)
                {
                    LogManager.LogError($"Error while loading theme {name} : Assembly is null", 1);
                    return null;
                }

                RegisterDependencies(asm);
            }

            catch (Exception e)
            {
                LogManager.LogError($"Error while loading theme {name} : {e.Message}", 1);
                return null;
            }

            ComponentsProviderSettings? settings;

            try
            {
                settings = FindSettings(name);
            }
            catch (Exception e)
            {
                LogManager.LogError($"Error while loading theme {name} : {e.Message}", 1);
                return null;
            }

            return settings;
        }

        private static ComponentsProviderSettings? FindSettings(string name)
        {
            var asm = Assembly.Load(name);

            var types = asm.GetTypes();
            foreach (var type in types)
            {
                if (typeof(ComponentsProviderSettings).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                {
                    var instance = Activator.CreateInstance(type);
                    return instance as ComponentsProviderSettings;
                }
            }

            LogManager.LogError("No ComponentsProvider found", 1);
            return null;
        }

        private static void RegisterDependencies(Assembly assembly)
        {
            foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
            {
                if (referencedAssembly.Name == null)
                {
                    // error
                    LogManager.LogWarning("Referenced assembly name is null");
                    continue;
                }

                m_loadedDependencies.Add(referencedAssembly.Name);
            }
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }


        private static Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name).Name;

            if (assemblyName == null)
            {
                LogManager.LogWarning("Assembly name is null");
                return null;
            }

            if (m_loadedDependencies.Contains(assemblyName))
            {
                string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "themes");
                string assemblyPath = Path.Combine(basePath, assemblyName + ".dll");

                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
            }

            return null;
        }

        private static bool ThemeExists(string name)
        {
            string path = Path.Combine(THEMES_PATH, name);
            if (Directory.Exists(path) == false)
            {
                return false;
            }

            var files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                if (file.Contains(name) && Path.GetExtension(file).ToLower() == ".dll")
                {
                    return true;
                }
            }

            // Todo check wwroot

            return false;
        }

        private static string? GetFirstThemePath(out string? name)
        {
            var themes = Directory.GetDirectories(THEMES_PATH);
            name = null;

            if (themes.Length == 0)
            {
                return null;
            }

            var path = themes[0];
            name = Path.GetFileName(path);
            return path;
        }

        
    }
}
