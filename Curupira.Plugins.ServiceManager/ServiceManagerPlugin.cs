using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;

namespace Curupira.Plugins.ServiceManager
{
    public class ServiceManagerPlugin : BasePlugin<ServiceManagerPluginConfig>
    {
        private volatile bool _killed;

        public ServiceManagerPlugin(ILogProvider logger, IPluginConfigParser<ServiceManagerPluginConfig> configParser)
            : base("ServiceManagerPlugin", logger, configParser)
        {
        }

        public override bool Execute(IDictionary<string, string> commandLineArgs)
        {
            Logger.TraceMethod(nameof(ServiceManagerPlugin), nameof(Execute), nameof(commandLineArgs), commandLineArgs);

            _killed = false;

            // Check for the required "bundle" argument
            if (!commandLineArgs.TryGetValue("bundle", out string bundleId))
            {
                Logger.Error("Missing 'bundle' argument in command line.");
                return false;
            }

            // Check if the specified bundle exists
            if (!Config.Bundles.TryGetValue(bundleId, out var bundle))
            {
                Logger.Error($"Bundle '{bundleId}' not found in configuration.");
                return false;
            }

            var success = true;
            var processedServices = 0;
            var totalServices = bundle.Services.Count;

            foreach (var serviceAction in bundle.Services)
            {
                if (_killed)
                {
                    Logger.Info(FormatLogMessage(nameof(commandLineArgs), "Plugin execution cancelled."));
                    return false;
                }

                try
                {
                    using (var serviceController = new ServiceController(serviceAction.ServiceName))
                    {
                        var auxSuccess = true;
                        switch (serviceAction.Action)
                        {
                            case ActionEnum.Start:
                                auxSuccess = StartService(serviceAction, serviceController);
                                success = success && auxSuccess;
                                break;
                            case ActionEnum.Stop:
                                auxSuccess = StopService(serviceAction, serviceController);
                                success = success && auxSuccess;
                                break;
                            case ActionEnum.StopOrKill:
                                auxSuccess = StopOrKillService(serviceAction, serviceController);
                                success = success && auxSuccess;
                                break;
                            case ActionEnum.Status:
                                auxSuccess = GetServiceStatus(bundle.LogFile, serviceController);
                                success = success && auxSuccess;
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "An error occurred during service management.");
                    success = false;
                }

                processedServices++;
                int percentage = (int)((double)processedServices / totalServices * 100);
                OnProgress(new PluginProgressEventArgs(percentage, $"Processed {processedServices} of {totalServices} services"));
            }

            return success;
        }

        private bool StartService(ServiceAction serviceAction, ServiceController serviceController)
        {
            Logger.TraceMethod(nameof(ServiceManagerPlugin), nameof(StartService), nameof(serviceAction), serviceAction, nameof(serviceController), serviceController);

            if (serviceController.Status != ServiceControllerStatus.Running)
            {
                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running);
                Logger.Info($"Service '{serviceAction.ServiceName}' started successfully.");
            }
            else
            {
                Logger.Warn($"Service '{serviceAction.ServiceName}' is already running.");
                return false;
            }
            return true;
        }

        private bool StopService(ServiceAction serviceAction, ServiceController serviceController)
        {
            Logger.TraceMethod(nameof(ServiceManagerPlugin), nameof(StopService), nameof(serviceAction), serviceAction, nameof(serviceController), serviceController);

            if (serviceController.CanStop && serviceController.Status != ServiceControllerStatus.Stopped)
            {
                serviceController.Stop();
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                Logger.Info($"Service '{serviceAction.ServiceName}' stopped successfully.");
            }
            else
            {
                Logger.Warn($"Service '{serviceAction.ServiceName}' is not running or cannot be stopped.");
            }
            return true; //We may change that in the future
        }

        private bool StopOrKillService(ServiceAction serviceAction, ServiceController serviceController)
        {
            Logger.TraceMethod(nameof(ServiceManagerPlugin), nameof(StopOrKillService), nameof(serviceAction), serviceAction, nameof(serviceController), serviceController);

            if (serviceController.CanStop && serviceController.Status != ServiceControllerStatus.Stopped)
            {
                try
                {
                    // Attempt to stop the service gracefully
                    serviceController.Stop();
                    serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));

                    Logger.Info($"Service '{serviceAction.ServiceName}' stopped successfully.");
                }
                catch (System.TimeoutException)
                {
                    Logger.Warn($"Service '{serviceAction.ServiceName}' did not stop within the timeout. Attempting to kill.");

                    // If graceful stop fails, try to kill the associated process
                    if (TryKillServiceProcess(serviceAction.ServiceName))
                    {
                        Logger.Info($"Service '{serviceAction.ServiceName}' process killed successfully.");
                    }
                    else
                    {
                        Logger.Error($"Failed to kill the process for service '{serviceAction.ServiceName}'.");
                        return false;
                    }
                }
            }
            else
            {
                Logger.Warn($"Service '{serviceAction.ServiceName}' is not running or cannot be stopped.");
            }

            return true;
        }

        private bool GetServiceStatus(string logFile, ServiceController serviceController)
        {
            Logger.TraceMethod(nameof(ServiceManagerPlugin), nameof(GetServiceStatus), nameof(logFile), logFile, nameof(serviceController), serviceController);

            if (string.IsNullOrWhiteSpace(logFile))
            {
                Logger.Error($"To read the status of a service, you need to inform the logFile attribute of the bundle in the config file.");
                return false;
            }
            try
            {
                var now = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                var statusLogEntry = $"[{now}] {serviceController.ServiceName}: {serviceController.Status}{Environment.NewLine}";
                System.IO.File.AppendAllText(string.Format(logFile, DateTime.Now), statusLogEntry);
            }
            catch (System.IO.IOException ex)
            {
                Logger.Error(ex, $"Error trying save the status of the service '{serviceController.ServiceName}' into {logFile}.");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error trying to get the service '{serviceController.ServiceName}' status .");
                return false;
            }
            return true;
        }

        private bool TryKillServiceProcess(string serviceName)
        {
            Logger.TraceMethod(nameof(ServiceManagerPlugin), nameof(TryKillServiceProcess), nameof(serviceName), serviceName);

            try
            {
                var processId = GetServiceProcessId(serviceName);
                if (processId > 0)
                {
                    var process = Process.GetProcessById(processId);
                    process.Kill();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error trying to kill the process for service '{serviceName}'.");
                return false;
            }
        }

        public int GetServiceProcessId(string serviceName)
        {
            Logger.TraceMethod(nameof(ServiceManagerPlugin), nameof(GetServiceProcessId), nameof(serviceName), serviceName);

            try
            {
                string query = $"SELECT ProcessId FROM Win32_Service WHERE Name = '{serviceName}'";
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
                Console.WriteLine($"Error retrieving process ID: {ex.Message}");
            }

            return -1; // Return -1 if not found or any error occurs
        }

        public override bool Kill()
        {
            Logger.TraceMethod(nameof(ServiceManagerPlugin), nameof(Kill));

            _killed = true;
            return true;
        }

        public override void Dispose()
        {
            Logger.TraceMethod(nameof(ServiceManagerPlugin), nameof(Dispose));

            // This plugin doesn't have any resources to dispose.
        }
    }
}