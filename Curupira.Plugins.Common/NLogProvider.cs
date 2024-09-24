using System;
using System.Collections;
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

        /// <summary>
        /// Logs the entry of a method along with its parameters.
        /// </summary>
        /// <param name="methodName">The name of the class containing the method.</param>
        /// <param name="methodName">The name of the method being entered.</param>
        /// <param name="parameters">An array of parameter names and their corresponding values, 
        /// alternating between name and value.</param>
        public void TraceMethod(string className, string methodName, params object[] parameters)
        {
            var logMessage = new System.Text.StringBuilder($"{className}.{methodName}(");
            for (int i = 0; i < parameters.Length; i += 2)
            {
                if (i + 1 < parameters.Length)
                {
                    var paramName = parameters[i].ToString();
                    var paramValue = parameters[i + 1];

                    // Handle IDictionary parameters
                    if (paramValue is IDictionary)
                    {
                        var dictionary = (IDictionary)paramValue;
                        logMessage.Append($"{paramName} = {{ ");

                        foreach (var key in dictionary.Keys)
                        {
                            var value = dictionary[key];

                            // Enclose string and char values in quotes
                            if (value is string || value is char)
                            {
                                value = $"\"{value}\"";
                            }

                            logMessage.Append($"\"{key}\" => {value}, ");
                        }

                        // Remove the trailing comma and space if present
                        if (logMessage.Length > 0 && logMessage[logMessage.Length - 2] == ',')
                        {
                            logMessage.Length -= 2;
                        }

                        logMessage.Append(" }");
                    }
                    else
                    {
                        // Enclose string and char values in quotes for other types
                        if (paramValue is string || paramValue is char)
                        {
                            paramValue = $"\"{paramValue}\"";
                        }

                        logMessage.Append($"{paramName} = {paramValue}");
                    }

                    logMessage.Append(", ");
                }
            }

            // Remove the trailing comma and space if present
            if (logMessage.Length > 0 && logMessage[logMessage.Length - 2] == ',')
            {
                logMessage.Length -= 2;
            }

            logMessage.Append(")");

            Trace(logMessage.ToString());
        }
    }
}