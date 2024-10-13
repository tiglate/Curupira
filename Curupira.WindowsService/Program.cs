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
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            AppRunner.SetEnvironmentVariables(args);

            if (Environment.UserInteractive)
            {
                // Run in console mode
                StartAsConsoleApp();
            }
            else
            {
                // Run as Windows Service
                ServiceBase.Run(new ControlService());
            }
        }

        private static void StartAsConsoleApp()
        {
            try
            {
                using (var runner = new AppRunner(_logger))
                {
                    runner.StartServer();
                    Console.WriteLine("Press [Enter] to stop the server...");
                    Console.ReadLine();
                    runner.StopServer();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while running the server in console mode.");
            }
        }
    }
}