using Curupira.Plugins.Contract;
using Curupira.Plugins.FoldersCreator;
using System.Threading;

namespace Curupira.Plugins.Tests.FoldersCreator
{
    internal class FoldersCreatorPluginWithDelay : FoldersCreatorPlugin
    {
        public FoldersCreatorPluginWithDelay(ILogProvider logger, IPluginConfigParser<FoldersCreatorPluginConfig> configParser)
            : base(logger, configParser)
        {
        }

        protected override void OnProgress(PluginProgressEventArgs e)
        {
            base.OnProgress(e);
            Thread.Sleep(1000);
        }
    }
}
