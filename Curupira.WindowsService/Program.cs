using System;
using System.ServiceProcess;
using Curupira.WindowsService.WindowsService;
using Microsoft.Owin.Hosting;
using NLog;
using System.Configuration;
using System.IO;

namespace Curupira.WindowsService
{
    public class Program
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static IDisposable _webApp;

        public static void Main(string[] args)
        {
            SetEnvironmentVariablesInDevMode();

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

        // Start the app in console mode
        private static void StartAsConsoleApp()
        {
            try
            {
                StartServer();
                Console.WriteLine("Press [Enter] to stop the server...");
                Console.ReadLine();
                StopServer();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while running the server in console mode.");
            }
        }

        // Start the OWIN Web Server
        public static void StartServer()
        {
            string baseAddress = ConfigurationManager.AppSettings["BaseAddress"];
            logger.Info($"Starting OWIN server at {baseAddress}");

            // Start OWIN server
            _webApp = WebApp.Start<Startup>(baseAddress);
            logger.Info("OWIN server started.");
        }

        // Stop the OWIN Web Server gracefully
        public static void StopServer()
        {
            if (_webApp != null)
            {
                logger.Info("Stopping OWIN server...");
                _webApp.Dispose();
                logger.Info("OWIN server stopped gracefully.");
            }
        }

        private static void SetEnvironmentVariablesInDevMode()
        {
#if DEBUG
            var envFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");

            if (!File.Exists(envFile))
            {
                return;
            }

            foreach (var pair in Infra.EnvFileParser.Parse(envFile))
            {
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }
#endif
        }
    }
}
