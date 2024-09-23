using System;
using System.ServiceProcess;
using Curupira.WindowsService.WindowsService;
using Microsoft.Owin.Hosting;
using NLog;
using System.Configuration;

namespace Curupira.WindowsService
{
    public class Program
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static IDisposable _webApp;

        public static void Main(string[] args)
        {
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
                StartOwinServer();
                Console.WriteLine("Press [Enter] to stop the server...");
                Console.ReadLine();
                StopOwinServer();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while running the server in console mode.");
            }
        }

        // Start the OWIN Web Server
        public static void StartOwinServer()
        {
            string baseAddress = ConfigurationManager.AppSettings["BaseAddress"];
            logger.Info($"Starting OWIN server at {baseAddress}");

            // Start OWIN server
            _webApp = WebApp.Start<Startup>(baseAddress);
            logger.Info("OWIN server started.");
        }

        // Stop the OWIN Web Server gracefully
        public static void StopOwinServer()
        {
            if (_webApp != null)
            {
                logger.Info("Stopping OWIN server...");
                _webApp.Dispose();
                logger.Info("OWIN server stopped gracefully.");
            }
        }
    }
}
