using System;
using System.Collections.Generic;
using System.ServiceProcess;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;

namespace Curupira.Plugins.ServiceManager
{
    public class ServiceManagerPlugin : BasePlugin<ServiceManagerPluginConfig>
    {
        private volatile bool _killed;
        private readonly IServiceControllerFactory _serviceControllerFactory;
        private readonly IProcessManager _processManager;

        public ServiceManagerPlugin(ILogProvider logger, IPluginConfigParser<ServiceManagerPluginConfig> configParser, IServiceControllerFactory serviceControllerFactory, IProcessManager processManager)
            : base("ServiceManagerPlugin", logger, configParser)
        {
            _serviceControllerFactory = serviceControllerFactory;
            _processManager = processManager;
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
                    using (var serviceController = _serviceControllerFactory.Build(serviceAction.ServiceName))
                    {
                        var auxSuccess = true;
                        switch (serviceAction.Action)
                        {
                            case Action.Start:
                                auxSuccess = StartService(serviceAction, serviceController);
                                success = success && auxSuccess;
                                break;
                            case Action.Stop:
                                auxSuccess = StopService(serviceAction, serviceController);
                                success = success && auxSuccess;
                                break;
                            case Action.StopOrKill:
                                auxSuccess = StopOrKillService(serviceAction, serviceController);
                                success = success && auxSuccess;
                                break;
                            case Action.Status:
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

        protected virtual bool StartService(ServiceAction serviceAction, IServiceController serviceController)
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

        protected virtual bool StopService(ServiceAction serviceAction, IServiceController serviceController)
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

        protected virtual bool StopOrKillService(ServiceAction serviceAction, IServiceController serviceController)
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
                catch (System.ServiceProcess.TimeoutException)
                {
                    Logger.Warn($"Service '{serviceAction.ServiceName}' did not stop within the timeout. Attempting to kill.");

                    // If graceful stop fails, try to kill the associated process
                    if (TryKillServiceProcess(serviceController))
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

        protected virtual bool GetServiceStatus(string logFile, IServiceController serviceController)
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
                WriteLogFile(string.Format(logFile, DateTime.Now), statusLogEntry);
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

        protected virtual void WriteLogFile(string logFile, string content)
        {
            System.IO.File.AppendAllText(logFile, content);
        }

        protected virtual bool TryKillServiceProcess(IServiceController serviceController)
        {
            Logger.TraceMethod(nameof(ServiceManagerPlugin), nameof(TryKillServiceProcess), nameof(serviceController), serviceController);

            try
            {
                var processId = serviceController.ProcessId;
                if (processId > 0)
                {
                    _processManager.Kill(serviceController.ProcessId);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error trying to kill the process for service '{serviceController.ServiceName}'.");
                return false;
            }
        }

        public override bool Kill()
        {
            Logger.TraceMethod(nameof(ServiceManagerPlugin), nameof(Kill));

            _killed = true;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Logger.TraceMethod(nameof(ServiceManagerPlugin), nameof(Dispose));
            }
            // This plugin doesn't have any resources to dispose.
        }
    }
}