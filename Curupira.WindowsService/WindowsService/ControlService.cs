using System.Diagnostics.CodeAnalysis;
using System.ServiceProcess;

namespace Curupira.WindowsService.WindowsService
{
    [ExcludeFromCodeCoverage]
    public partial class ControlService : ServiceBase
    {
        private readonly AppRunner _runner;

        public ControlService()
        {
            InitializeComponent();
            _runner = new AppRunner();
        }

        protected override void OnStart(string[] args)
        {
            _runner.StartServer();
        }

        protected override void OnStop()
        {
            _runner.StopServer();
        }
    }
}
