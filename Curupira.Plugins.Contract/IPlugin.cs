using System;
using System.Collections.Generic;
using System.Xml;

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
        /// Initializes the plug-in with configuration data from an XML element.
        /// </summary>
        /// <param name="config">The XML element containing configuration data.</param>
        void Init(XmlElement config);

        /// <summary>
        /// Executes the core functionality of the plug-in.
        /// </summary>
        /// <param name="commandLineArgs">Additional parameters from the command line.</param>
        /// <returns>True if execution was successful, false otherwise.</returns>
        bool Execute(IDictionary<string, string> commandLineArgs);

        /// <summary>
        /// Begins the asynchronous execution of the core functionality of the plug-in.
        /// </summary>
        /// <param name="commandLineArgs">Additional parameters from the command line.</param>
        /// <param name="callback">The AsyncCallback delegate to be called when the asynchronous operation completes.</param>
        /// <param name="state">A user-defined object that qualifies or contains information about the asynchronous operation.</param>
        /// <returns>An IAsyncResult that represents the asynchronous operation.</returns>
        IAsyncResult BeginExecute(IDictionary<string, string> commandLineArgs, AsyncCallback callback, object state);

        /// <summary>
        /// Ends the asynchronous execution of the plug-in.
        /// </summary>
        /// <param name="asyncResult">The IAsyncResult returned by the call to BeginExecute.</param>
        /// <returns>True if execution was successful, false otherwise.</returns>
        bool EndExecute(IAsyncResult asyncResult);

        /// <summary>
        /// Attempts to stop or kill the plug-in if it is stuck.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        bool Kill();

        /// <summary>
        /// Begins the asynchronous attempt to stop or kill the plug-in if it is stuck.
        /// </summary>
        /// <param name="callback">The AsyncCallback delegate to be called when the asynchronous operation completes.</param>
        /// <param name="state">A user-defined object that qualifies or contains information about the asynchronous operation.</param>
        /// <returns>An IAsyncResult that represents the asynchronous operation.</returns>
        IAsyncResult BeginKill(AsyncCallback callback, object state);

        /// <summary>
        /// Ends the asynchronous attempt to stop or kill the plug-in.
        /// </summary>
        /// <param name="asyncResult">The IAsyncResult returned by the call to BeginKill.</param>
        /// <returns>True if successful, false otherwise.</returns>
        bool EndKill(IAsyncResult asyncResult);

        /// <summary>
        /// Event triggered to report the progress of the plug-in's execution.
        /// </summary>
        event EventHandler<PluginProgressEventArgs> Progress;
    }
}