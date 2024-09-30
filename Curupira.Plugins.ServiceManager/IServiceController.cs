using System;
using System.ServiceProcess;

namespace Curupira.Plugins.ServiceManager
{
    public interface IServiceController : IDisposable
    {
        int ProcessId { get; }

        bool CanPauseAndContinue { get; }

        bool CanShutdown { get; }

        bool CanStop { get; }

        ServiceControllerStatus Status { get; }

        string DisplayName { get; set; }

        string MachineName { get; set; }

        string ServiceName { get; set; }

        ServiceController[] ServicesDependedOn { get; }

        ServiceType ServiceType { get; }

        ServiceStartMode StartType { get; }

        void Start();

        void Refresh();

        void Stop();

        void WaitForStatus(ServiceControllerStatus desiredStatus);

        void WaitForStatus(ServiceControllerStatus desiredStatus, TimeSpan timeout);
    }
}
