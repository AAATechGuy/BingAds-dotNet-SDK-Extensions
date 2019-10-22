using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ConsoleApp
{
    internal static class AssemblyResolverUtility
    {
        internal static void SetDefaultAssemblyResolver()
        {
            if (ConfigurationManager.AppSettings[nameof(SetDefaultAssemblyResolver)] == "disable") { return; }

            // this is to prevent conflict between multiple versions of the same assembly.
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolveEventHandler;
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveEventHandler;
        }

        private static readonly ResolveEventHandler AssemblyResolveEventHandler = (sender, resolveEventArgs) =>
        {
            var assemblyName = GetAssemblyName(resolveEventArgs.Name);
            var assembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains(assemblyName)).FirstOrDefault();
            var assemblyDllPath1 = $"{assemblyName}.dll";
            assembly = assembly ?? (File.Exists(assemblyDllPath1) ? Assembly.LoadFrom(assemblyDllPath1) : null);
            var assemblyDllPath2 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"{assemblyName}.dll");
            assembly = assembly ?? (File.Exists(assemblyDllPath2) ? Assembly.LoadFrom(assemblyDllPath2) : null);
            return assembly;
        };

        internal static string GetAssemblyName(string fullAssemblyName)
        {
            const string AssemblyNamePattern = "^([^,]+)([,].+)*$";
            return Regex.Match(fullAssemblyName, AssemblyNamePattern).Groups[1].Value;
        }
    }
}