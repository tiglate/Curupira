using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Curupira.Plugins.Tests.Common
{
    // Helper class to test the BasePlugin
    public class TestPlugin : BasePlugin<object>
    {
        public TestPlugin(string pluginName, ILogProvider logger, IPluginConfigParser<object> configParser)
            : base(pluginName, logger, configParser) { }

        public void RaiseProgress(PluginProgressEventArgs e)
        {
            OnProgress(e);
        }

        public string FormatTestLogMessage(string method, string message, bool includeTimestamp = false)
        {
            return FormatLogMessage(method, message, includeTimestamp);
        }

        protected override void Dispose(bool disposing)
        {
            //Nothing to clean up
        }

        public override Task<bool> ExecuteAsync(IDictionary<string, string> commandLineArgs, CancellationToken cancelationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}
