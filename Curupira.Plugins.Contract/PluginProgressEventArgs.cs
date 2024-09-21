using System;

namespace Curupira.Plugins.Contract
{
    /// <summary>
    /// Provides data for the PluginProgress event.
    /// </summary>
    public class PluginProgressEventArgs : EventArgs
    {
        /// <summary>
        /// The completion percentage of the plug-in's execution.
        /// </summary>
        public int Percentage { get; }

        /// <summary>
        /// A message relevant to the progress of the plug-in's execution.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the PluginProgressEventArgs class.
        /// </summary>
        /// <param name="percentage">The completion percentage of the plug-in's execution.</param>
        /// <param name="message">A message relevant to the progress of the plug-in's execution.</param>
        public PluginProgressEventArgs(int percentage, string message)
        {
            Percentage = percentage;
            Message = message;
        }
    }
}
