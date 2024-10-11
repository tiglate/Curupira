using System.Diagnostics.CodeAnalysis;
using System.ServiceProcess;

namespace Curupira.WindowsService.WindowsService
{
    [ExcludeFromCodeCoverage]
    public partial class ControlService : ServiceBase
    {
        public ControlService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Program.StartServer();
        }

        protected override void OnStop()
        {
            Program.StopServer();
        }
    }
}
