﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curupira.Plugins.Contract
{
    /// <summary>
    /// Defines the contract for a plug-in within the automation framework.
    /// </summary>
    public interface IPlugin : IDisposable
    {
        /// <summary>
        /// Gets the name of the plug-in.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Plug-in initializes routines
        /// </summary>
        void Init();

        /// <summary>
        /// Asynchronously executes the core functionality of the plug-in.
        /// </summary>
        /// <param name="commandLineArgs">Additional parameters from the command line.</param>
        /// <returns>A Task representing the asynchronous operation. The Task's result is true if execution was successful, false otherwise.</returns>
        Task<bool> ExecuteAsync(IDictionary<string, string> commandLineArgs);

        /// <summary>
        /// Asynchronously attempts to stop or kill the plug-in if it is stuck.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation. The Task's result is true if successful, false otherwise.</returns>
        Task<bool> KillAsync();

        /// <summary>
        /// Event triggered to report the progress of the plug-in's execution.
        /// </summary>
        event EventHandler<PluginProgressEventArgs> Progress;
    }
}