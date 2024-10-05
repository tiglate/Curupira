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
        public Logger InnerLogger { get; private set; }

        /// <summary>
        /// Initializes a new instance of the NLogProvider class.
        /// </summary>
        /// <param name="loggerName">The name of the NLog logger to use. If null or empty, the default logger will be used.</param>
        public NLogProvider(string loggerName = null)
        {
            InnerLogger = string.IsNullOrEmpty(loggerName) ? LogManager.GetCurrentClassLogger() : LogManager.GetLogger(loggerName);
        }

        /// <summary>
        /// Initializes a new instance of the NLogProvider class.
        /// </summary>
        /// <param name="logger">Instance of NLog.Logger class.</param>
        public NLogProvider(Logger logger)
        {
            InnerLogger = logger;
        }

        public virtual void Trace(string message, params object[] args) => InnerLogger.Trace(message, args);

        public virtual void Debug(string message, params object[] args) => InnerLogger.Debug(message, args);

        public virtual void Info(string message, params object[] args) => InnerLogger.Info(message, args);

        public virtual void Warn(string message, params object[] args) => InnerLogger.Warn(message, args);

        public virtual void Fatal(string message, params object[] args) => InnerLogger.Fatal(message, args);

        public virtual void Fatal(Exception exception, string message = null, params object[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                InnerLogger.Fatal(exception);
            }
            else
            {
                InnerLogger.Fatal(exception, message, args);
            }
        }

        public virtual void Error(string message, params object[] args) => InnerLogger.Error(message, args);

        public virtual void Error(Exception exception, string message = null, params object[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                InnerLogger.Error(exception);
            }
            else
            {
                InnerLogger.Error(exception, message, args);
            }
        }

        /// <summary>
        /// Logs the entry of a method along with its parameters.
        /// </summary>
        /// <param name="methodName">The name of the class containing the method.</param>
        /// <param name="methodName">The name of the method being entered.</param>
        /// <param name="parameters">An array of parameter names and their corresponding values, 
        /// alternating between name and value.</param>
        public virtual void TraceMethod(string className, string methodName, params object[] parameters)
        {
            var logMessage = new System.Text.StringBuilder($"{className}.{methodName}(");
            for (int i = 0; i < parameters.Length; i += 2)
            {
                if (i + 1 < parameters.Length)
                {
                    var paramName = parameters[i].ToString();
                    var paramValue = parameters[i + 1];

                    // Handle IDictionary parameters
                    if (paramValue is IDictionary dictionary)
                    {
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