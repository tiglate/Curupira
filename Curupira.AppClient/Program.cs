using Curupira.AppClient.Infra.IoC;
using NLog;
using NLog.Config;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Curupira.AppClient
{
    [ExcludeFromCodeCoverage]
    static class Program
    {
        static Program()
        {
#if !DEBUG
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyFromLibFolder;
#endif
        }

        static async Task<int> Main(string[] args)
        {
            // Load config files and set up assembly resolution only in release mode
#if !DEBUG
            ConfigureAppSettings();
            ConfigureNLog();
#endif

            using (var container = AutofacContainerBuilder.Configure())
            {
                using (var runner = new AppRunner(container))
                {
                    return await runner.RunAsync(args).ConfigureAwait(false);
                }
            }
        }

#pragma warning disable S1144 // Unused private types or members should be removed

        private static void ConfigureAppSettings()
        {
            // Override the default app.config location
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\conf\Curupira.exe.config");
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configPath);
        }

        private static void ConfigureNLog()
        {
            // Load NLog configuration from the specified path
            string nlogConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\conf\NLog.config");
            LogManager.Configuration = new XmlLoggingConfiguration(nlogConfigPath);
        }

        private static Assembly ResolveAssemblyFromLibFolder(object sender, ResolveEventArgs args)
        {
            // Extract the assembly name
            var assemblyName = new AssemblyName(args.Name).Name + ".dll";
            var libPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\lib", assemblyName);

            // Check if the DLL exists in the lib directory
            if (File.Exists(libPath))
            {
#pragma warning disable S3885
                return Assembly.LoadFrom(libPath);
#pragma warning restore S3885
            }

            return null; // Return null if the assembly is not found
        }

#pragma warning restore S1144 // Unused private types or members should be removed
    }
}
