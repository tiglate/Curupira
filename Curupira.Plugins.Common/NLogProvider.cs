using System;
using Curupira.Plugins.Contract;
using NLog;

namespace Curupira.Plugins.Common
{
    /// <summary>
    /// Implementation of the ILogProvider interface using NLog.
    /// </summary>
    public class NLogProvider : ILogProvider
    {
        private readonly Logger _logger;

        /// <summary>
        /// Initializes a new instance of the NLogProvider class.
        /// </summary>
        /// <param name="loggerName">The name of the NLog logger to use. If null or empty, the default logger will be used.</param>
        public NLogProvider(string loggerName = null)
        {
            _logger = string.IsNullOrEmpty(loggerName) ? LogManager.GetCurrentClassLogger() : LogManager.GetLogger(loggerName);
        }

        public void Trace(string message, params object[] args) => _logger.Trace(message, args);

        public void Debug(string message, params object[] args) => _logger.Debug(message, args);

        public void Info(string message, params object[] args) => _logger.Info(message, args);

        public void Warn(string message, params object[] args) => _logger.Warn(message, args);

        public void Error(string message, params object[] args) => _logger.Error(message, args);

        public void Fatal(string message, params object[] args) => _logger.Fatal(message, args);

        public void Fatal(Exception exception, string message = null, params object[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                _logger.Fatal(exception);
            }
            else
            {
                _logger.Fatal(exception, message, args);
            }
        }

        public void Error(Exception exception, string message = null, params object[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                _logger.Error(exception);
            }
            else
            {
                _logger.Error(exception, message, args);
            }
        }
    }
}