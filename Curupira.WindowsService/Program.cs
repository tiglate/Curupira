using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceProcess;
using Curupira.WindowsService.WindowsService;
using NLog;

namespace Curupira.WindowsService
{
    [ExcludeFromCodeCoverage]
    static class Program
    {
        static Program()
        {
#if !DEBUG
            System.AppDomain.CurrentDomain.AssemblyResolve += AppConfigurationHelper.ResolveAssemblyFromLibFolder;
#endif
        }

        public static void Main(string[] args)
        {
#if !DEBUG
            // Load config files and set up assembly resolution only in release mode
            AppConfigurationHelper.ConfigureAppSettings("CurupiraService.exe.config");
            AppConfigurationHelper.ConfigureNLog();
#endif

            AppRunner.SetEnvironmentVariables(args);

            var logger = LogManager.GetCurrentClassLogger();

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("API_KEY")))
            {
                logger.Error("API_KEY environment variable is not set.");
                return;
            }

            if (Environment.UserInteractive)
            {
                // Run in console mode
                StartAsConsoleApp(logger);
            }
            else
            {
                // Run as Windows Service
                ServiceBase.Run(new ControlService());
            }
        }

        private static void StartAsConsoleApp(Logger logger)
        {
            try
            {
                using (var runner = new AppRunner(logger))
                {
                    runner.StartServer();
                    Console.WriteLine("Press [Enter] to stop the server...");
                    Console.ReadLine();
                    runner.StopServer();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while running the server in console mode.");
            }
        }
    }
}