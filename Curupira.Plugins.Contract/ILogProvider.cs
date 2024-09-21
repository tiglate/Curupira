using System;

namespace Curupira.Plugins.Contract
{
    /// <summary>
    /// Provides a common interface for logging messages within the framework.
    /// </summary>
    public interface ILogProvider
    {
        /// <summary>
        /// Logs a message at the Trace level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional arguments for formatting the message.</param>
        void Trace(string message, params object[] args);

        /// <summary>
        /// Logs a message at the Debug level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional arguments for formatting the message.</param>
        void Debug(string message, params object[] args);

        /// <summary>
        /// Logs a message at the Information level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional arguments for formatting the message.</param>
        void Info(string message, params object[] args);

        /// <summary>
        /// Logs a message at the Warning level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional arguments for formatting the message.</param>
        void Warn(string message, params object[] args);

        /// <summary>
        /// Logs a message at the Error level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional arguments for formatting the message.</param>
        void Error(string message, params object[] args);

        /// <summary>
        /// Logs a message at the Fatal level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional arguments for formatting the message.</param>
        void Fatal(string message, params object[] args);

        /// <summary>
        /// Logs an exception at the Fatal level.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">An optional message to include with the exception log.</param>
        /// <param name="args">Optional arguments for formatting the message.</param>rgs">Optional arguments for formatting the message.</param>
        void Fatal(Exception exception, string message = null, params object[] args);

        /// <summary>
        /// Logs an exception at the Error level.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">An optional message to include with the exception log.</param>
        /// <param name="args">Optional arguments for formatting the message.</param>
        void Error(Exception exception, string message = null, params object[] args);
    }
}