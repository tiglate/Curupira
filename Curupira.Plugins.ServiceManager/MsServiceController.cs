using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management;
using System.ServiceProcess;

namespace Curupira.Plugins.ServiceManager
{
    [ExcludeFromCodeCoverage]
    public class MsServiceController : IServiceController
    {
        private readonly ServiceController _serviceController;

        public MsServiceController(string name)
        {
            _serviceController = new ServiceController(name);
        }

        public MsServiceController(string name, string machineName)
        {
            _serviceController = new ServiceController(name, machineName);
        }

        public bool CanPauseAndContinue => _serviceController.CanPauseAndContinue;

        public bool CanShutdown => _serviceController.CanShutdown;

        public bool CanStop => _serviceController.CanStop;

        public ServiceControllerStatus Status => _serviceController.Status;

        public string DisplayName { get => _serviceController.DisplayName; set => _serviceController.DisplayName = value; }

        public string MachineName { get => _serviceController.MachineName; set => _serviceController.MachineName = value; }

        public string ServiceName { get => _serviceController.ServiceName; set => _serviceController.ServiceName = value; }

        public ServiceController[] ServicesDependedOn => _serviceController.ServicesDependedOn;

        public ServiceType ServiceType => _serviceController.ServiceType;

        public ServiceStartMode StartType => _serviceController.StartType;

        public void Start()
        {
            _serviceController.Start();
        }

        public void Refresh()
        {
            _serviceController.Refresh();
        }

        public void Stop()
        {
            _serviceController.Stop();
        }

        public void WaitForStatus(ServiceControllerStatus desiredStatus)
        {
            _serviceController.WaitForStatus(desiredStatus);
        }

        public void WaitForStatus(ServiceControllerStatus desiredStatus, TimeSpan timeout)
        {
            _serviceController.WaitForStatus(desiredStatus, timeout);
        }

        public int ProcessId
        {
            get
            {
                try
                {
                    string query = $"SELECT ProcessId FROM Win32_Service WHERE Name = '{_serviceController.ServiceName}'";
                    using (var searcher = new ManagementObjectSearcher(query))
                    {
                        foreach (var obj in searcher.Get().Cast<ManagementObject>())
                        {
                            return Convert.ToInt32(obj["ProcessId"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error retrieving process ID: {ex.Message}");
                }

                return -1; // Return -1 if not found or any error occurs
            }
        }

        public void Dispose()
        {
            _serviceController.Dispose();
        }
    }
}
