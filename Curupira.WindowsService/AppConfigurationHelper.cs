using NLog;
using NLog.Config;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace Curupira.WindowsService
{
    [ExcludeFromCodeCoverage]
    public static class AppConfigurationHelper
    {
        public static void ConfigureAppSettings(string configFileName)
        {
            // Override the default app.config location
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\conf", configFileName);
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configPath);
        }

        public static void ConfigureNLog()
        {
            // Load NLog configuration from the specified path
            var nlogConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\conf\NLog.config");
            LogManager.Configuration = new XmlLoggingConfiguration(nlogConfigPath);
        }

        public static Assembly ResolveAssemblyFromLibFolder(object sender, ResolveEventArgs args)
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
    }
}
