using System;
using System.Configuration;
using System.IO;
using Microsoft.Owin.Hosting;
using NLog;

namespace Curupira.WindowsService
{
    public class AppRunner : IDisposable
    {
        private readonly ILogger _logger;
        private IDisposable _webApp;

        public AppRunner(ILogger logger = null)
        {
            _logger = logger ?? LogManager.GetCurrentClassLogger();
        }

        public virtual void StartServer()
        {
            var baseAddress = ConfigurationManager.AppSettings["BaseAddress"];

            _logger.Info("Starting OWIN server at {0}", baseAddress);
            _webApp = StartOwin(baseAddress);
            _logger.Info("OWIN server started.");
        }

        public virtual void StopServer()
        {
            if (_webApp != null)
            {
                _logger.Info("Stopping OWIN server...");
                _webApp.Dispose();
                _logger.Info("OWIN server stopped gracefully.");
            }
        }

        protected virtual IDisposable StartOwin(string baseAddress)
        {
            return WebApp.Start<Startup>(baseAddress);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopServer();
            }
        }

        public static void SetEnvironmentVariablesInDevMode(string[] commandLineArgs)
        {
#if DEBUG
            if (commandLineArgs.Length > 1 && commandLineArgs[0] == "--api-key")
            {
                Environment.SetEnvironmentVariable("API_KEY", commandLineArgs[1]);
            }
            else
            {
                var envFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");

                if (!File.Exists(envFile))
                {
                    return;
                }

                foreach (var pair in Infra.EnvFileParser.Parse(envFile))
                {
                    Environment.SetEnvironmentVariable(pair.Key, pair.Value);
                }
            }
#endif
        }
    }
}
