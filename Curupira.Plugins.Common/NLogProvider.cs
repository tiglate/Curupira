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
        public Logger Log { get; private set; }

        /// <summary>
        /// Initializes a new instance of the NLogProvider class.
        /// </summary>
        /// <param name="loggerName">The name of the NLog logger to use. If null or empty, the default logger will be used.</param>
        public NLogProvider(string loggerName = null)
        {
            Log = string.IsNullOrEmpty(loggerName) ? LogManager.GetCurrentClassLogger() : LogManager.GetLogger(loggerName);
        }

        /// <summary>
        /// Initializes a new instance of the NLogProvider class.
        /// </summary>
        /// <param name="logger">Instance of NLog.Logger class.</param>
        public NLogProvider(Logger logger)
        {
            Log = logger;
        }

        public virtual void Trace(string message, params object[] args) => Log.Trace(message, args);

        public virtual void Debug(string message, params object[] args) => Log.Debug(message, args);

        public virtual void Info(string message, params object[] args) => Log.Info(message, args);

        public virtual void Warn(string message, params object[] args) => Log.Warn(message, args);

        public virtual void Fatal(string message, params object[] args) => Log.Fatal(message, args);

        public virtual void Fatal(Exception exception, string message = null, params object[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                Log.Fatal(exception);
            }
            else
            {
                Log.Fatal(exception, message, args);
            }
        }

        public virtual void Error(string message, params object[] args) => Log.Error(message, args);

        public virtual void Error(Exception exception, string message = null, params object[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                Log.Error(exception);
            }
            else
            {
                Log.Error(exception, message, args);
            }
        }

        public virtual void TraceMethod(string className, string methodName, params object[] parameters)
        {
            var logMessage = new System.Text.StringBuilder($"{className}.{methodName}(");

            for (int i = 0; i < parameters.Length; i += 2)
            {
                if (i + 1 < parameters.Length)
                {
                    var paramName = parameters[i].ToString();
                    var paramValue = parameters[i + 1];

                    logMessage.Append($"{paramName} = {FormatParameterValue(paramValue)}, ");
                }
            }

            RemoveTrailingComma(logMessage);
            logMessage.Append(")");

            Trace(logMessage.ToString());
        }

        private static string FormatParameterValue(object paramValue)
        {
            if (paramValue is IDictionary dictionary)
            {
                return FormatDictionary(dictionary);
            }

            return FormatBasicValue(paramValue);
        }

        private static string FormatDictionary(IDictionary dictionary)
        {
            var dictLog = new System.Text.StringBuilder("{ ");

            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];
                dictLog.Append($"\"{key}\" => {FormatBasicValue(value)}, ");
            }

            RemoveTrailingComma(dictLog);
            dictLog.Append(" }");

            return dictLog.ToString();
        }

        private static string FormatBasicValue(object value)
        {
            if (value is string || value is char)
            {
                return $"\"{value}\"";
            }

            return value?.ToString() ?? "null";
        }

        private static void RemoveTrailingComma(System.Text.StringBuilder sb)
        {
            if (sb.Length > 0 && sb[sb.Length - 2] == ',')
            {
                sb.Length -= 2;
            }
        }
    }
}