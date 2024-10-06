using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using Curupira.Plugins.ServiceManager;
using Curupira.Plugins.Contract;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Curupira.Tests.Plugins.ServiceManager
{
    [TestClass]
    public class ServiceManagerPluginTests
    {
        private Mock<ILogProvider> _loggerMock;
        private Mock<IPluginConfigParser<ServiceManagerPluginConfig>> _configParserMock;
        private Mock<IServiceControllerFactory> _serviceControllerFactoryMock;
        private Mock<IServiceController> _serviceControllerMock;
        private Mock<IProcessManager> _processManagerMock;
        private Mock<ServiceManagerPlugin> _serviceManagerPluginMock;
        private ServiceManagerPluginConfig _pluginConfig;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogProvider>();
            _configParserMock = new Mock<IPluginConfigParser<ServiceManagerPluginConfig>>();
            _serviceControllerFactoryMock = new Mock<IServiceControllerFactory>();
            _serviceControllerMock = new Mock<IServiceController>();
            _processManagerMock = new Mock<IProcessManager>();

            _pluginConfig = new ServiceManagerPluginConfig();
            _configParserMock.Setup(p => p.Execute()).Returns(_pluginConfig);

            _serviceManagerPluginMock = new Mock<ServiceManagerPlugin>(
                _loggerMock.Object,
                _configParserMock.Object,
                _serviceControllerFactoryMock.Object,
                _processManagerMock.Object
            )
            { CallBase = true };  // Allows calling real methods unless they are explicitly mocked

            _serviceManagerPluginMock.Object.Init();
        }

        [TestMethod]
        public async Task Execute_ShouldLogError_WhenBundleIsMissing()
        {
            // Arrange
            var commandLineArgs = new Dictionary<string, string>();

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when the 'bundle' argument is missing.");
            _loggerMock.Verify(l => l.Error(It.Is<string>(msg => msg.Contains("Missing 'bundle' argument"))), Times.Once);
        }

        [TestMethod]
        public async Task Execute_ShouldLogError_WhenBundleIsNotFound()
        {
            // Arrange
            var commandLineArgs = new Dictionary<string, string> { { "bundle", "nonexistent_bundle" } };

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when the specified bundle is not found.");
            _loggerMock.Verify(l => l.Error(It.Is<string>(msg => msg.Contains("Bundle 'nonexistent_bundle' not found"))), Times.Once);
        }

        [TestMethod]
        public async Task Execute_ShouldStartService_WhenBundleContainsStartAction()
        {
            // Arrange
            var serviceName = "TestService";
            var serviceAction = new ServiceAction(serviceName, Curupira.Plugins.ServiceManager.Action.Start);
            var bundle = new Bundle("start_bundle");
            bundle.Services.Add(serviceAction);
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "start_bundle" } };

            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Stopped);
            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName)).Returns(_serviceControllerMock.Object);

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result, "The plugin should return true when the service is started successfully.");
            _serviceControllerMock.Verify(s => s.Start(), Times.Once);
            _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.Contains($"Service '{serviceName}' started successfully"))), Times.Once);
        }

        [TestMethod]
        public async Task Execute_ShouldReturnTrue_WhenStoppingFailsButKillingProcessSucceeds()
        {
            // Arrange
            var serviceName = "TestService";
            var serviceAction = new ServiceAction(serviceName, Curupira.Plugins.ServiceManager.Action.StopOrKill);
            var bundle = new Bundle("stopOrKill_bundle");
            bundle.Services.Add(serviceAction);
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "stopOrKill_bundle" } };

            // Setup serviceControllerMock behavior to simulate a TimeoutException when trying to stop the service
            _serviceControllerMock.Setup(s => s.CanStop).Returns(true);
            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Running);
            _serviceControllerMock.Setup(s => s.Stop()).Throws(new System.ServiceProcess.TimeoutException());

            // Simulate successful process killing
            _serviceControllerMock.Setup(s => s.ProcessId).Returns(12345);
            _processManagerMock.Setup(p => p.Kill(12345)).Verifiable();

            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName)).Returns(_serviceControllerMock.Object);

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result, "The plugin should return true when stopping the service fails but killing the process succeeds.");
            _serviceControllerMock.Verify(s => s.Stop(), Times.Once);
            _processManagerMock.Verify(p => p.Kill(12345), Times.Once);
            _loggerMock.Verify(l => l.Warn(It.Is<string>(msg => msg.Contains($"Service '{serviceName}' did not stop within the timeout. Attempting to kill."))), Times.Once);
            _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.Contains($"Service '{serviceName}' process killed successfully."))), Times.Once);
        }

        [TestMethod]
        public async Task Execute_ShouldReturnFalse_WhenStoppingFailsAndKillingProcessFails()
        {
            // Arrange
            var serviceName = "TestService";
            var serviceAction = new ServiceAction(serviceName, Curupira.Plugins.ServiceManager.Action.StopOrKill);
            var bundle = new Bundle("stopOrKill_bundle");
            bundle.Services.Add(serviceAction);
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "stopOrKill_bundle" } };

            // Setup serviceControllerMock behavior to simulate a TimeoutException when trying to stop the service
            _serviceControllerMock.Setup(s => s.CanStop).Returns(true);
            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Running);
            _serviceControllerMock.Setup(s => s.Stop()).Throws(new System.ServiceProcess.TimeoutException());

            // Simulate failure to kill the process
            _serviceControllerMock.Setup(s => s.ProcessId).Returns(12345);
            _processManagerMock.Setup(p => p.Kill(12345)).Throws(new Exception("Failed to kill process"));

            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName)).Returns(_serviceControllerMock.Object);

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when stopping the service fails and killing the process also fails.");
            _serviceControllerMock.Verify(s => s.Stop(), Times.Once);
            _processManagerMock.Verify(p => p.Kill(12345), Times.Once);
            _loggerMock.Verify(l => l.Warn(It.Is<string>(msg => msg.Contains($"Service '{serviceName}' did not stop within the timeout. Attempting to kill."))), Times.Once);
            _loggerMock.Verify(l => l.Error(It.Is<string>(msg => msg.Contains($"Failed to kill the process for service '{serviceName}'."))), Times.Once);
        }

        [TestMethod]
        public async Task Execute_ShouldLogWarningAndReturnFalse_WhenServiceIsAlreadyRunning()
        {
            // Arrange
            var serviceName = "TestService";
            var serviceAction = new ServiceAction(serviceName, Curupira.Plugins.ServiceManager.Action.Start);
            var bundle = new Bundle("start_bundle");
            bundle.Services.Add(serviceAction);
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "start_bundle" } };

            // Setup serviceControllerMock behavior to simulate a service that is already running
            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Running);  // Service is already running

            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName)).Returns(_serviceControllerMock.Object);

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when the service is already running.");

            // Verify that the warning message is logged when the service is already running
            _loggerMock.Verify(l => l.Warn(It.Is<string>(msg => msg.Contains($"Service '{serviceName}' is already running."))), Times.Once);
        }

        [TestMethod]
        public async Task Execute_ShouldStopService_WhenBundleContainsStopAction()
        {
            // Arrange
            var serviceName = "TestService";
            var serviceAction = new ServiceAction(serviceName, Curupira.Plugins.ServiceManager.Action.Stop);
            var bundle = new Bundle("stop_bundle");
            bundle.Services.Add(serviceAction);
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "stop_bundle" } };

            _serviceControllerMock.Setup(s => s.CanStop).Returns(true);
            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Running);
            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName)).Returns(_serviceControllerMock.Object);

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result, "The plugin should return true when the service is stopped successfully.");
            _serviceControllerMock.Verify(s => s.Stop(), Times.Once);
            _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.Contains($"Service '{serviceName}' stopped successfully"))), Times.Once);
        }

        [TestMethod]
        public async Task Execute_ShouldReturnTrue_WhenServiceStopsSuccessfully()
        {
            // Arrange
            var serviceName = "TestService";
            var serviceAction = new ServiceAction(serviceName, Curupira.Plugins.ServiceManager.Action.StopOrKill);
            var bundle = new Bundle("stopOrKill_bundle");
            bundle.Services.Add(serviceAction);
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "stopOrKill_bundle" } };

            // Setup serviceControllerMock behavior to simulate that the service can be stopped
            _serviceControllerMock.Setup(s => s.CanStop).Returns(true);
            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Running);

            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName)).Returns(_serviceControllerMock.Object);

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result, "The plugin should return true when the service stops successfully.");
            _serviceControllerMock.Verify(s => s.Stop(), Times.Once);
            _serviceControllerMock.Verify(s => s.WaitForStatus(ServiceControllerStatus.Stopped, It.IsAny<TimeSpan>()), Times.Once);
            _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.Contains($"Service '{serviceName}' stopped successfully."))), Times.Once);
        }

        [TestMethod]
        public async Task Execute_ShouldLogServiceStatus_WhenBundleContainsStatusAction()
        {
            // Arrange
            var serviceName = "TestService";
            var serviceAction = new ServiceAction(serviceName, Curupira.Plugins.ServiceManager.Action.Status);
            var bundle = new Bundle("status_bundle") { LogFile = "C:\\temp\\services_status.txt" };
            bundle.Services.Add(serviceAction);
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "status_bundle" } };

            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Running);
            _serviceControllerMock.Setup(s => s.ServiceName).Returns(serviceName);
            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName)).Returns(_serviceControllerMock.Object);

            // Mock the WriteLogFile method
            _serviceManagerPluginMock
                .Protected()
                .Setup("WriteLogFile", ItExpr.IsAny<string>(), ItExpr.IsAny<string>())
                .Callback<string, string>((logFile, content) =>
                {
                    Assert.IsTrue(logFile.Contains("services_status.txt"));
                    Assert.IsTrue(content.Contains(serviceName));
                });

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result, "The plugin should return true when the service status is logged successfully.");
            _serviceManagerPluginMock
                .Protected()
                .Verify("WriteLogFile", Times.Once(), ItExpr.IsAny<string>(), ItExpr.IsAny<string>());
        }

        [TestMethod]
        public async Task Execute_ShouldReturnFalse_WhenStopFailsWithException()
        {
            // Arrange
            var serviceName = "TestService";
            var serviceAction = new ServiceAction(serviceName, Curupira.Plugins.ServiceManager.Action.Stop);
            var bundle = new Bundle("kill_bundle");
            bundle.Services.Add(serviceAction);
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "kill_bundle" } };

            // Setup serviceControllerMock behavior
            _serviceControllerMock.Setup(s => s.CanStop).Returns(true);
            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Running);

            // Simulate a TimeoutException when trying to stop the service
            _serviceControllerMock.Setup(s => s.Stop()).Throws(new System.ServiceProcess.TimeoutException());

            // Mock the process manager
            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName)).Returns(_serviceControllerMock.Object);

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when stopping the service fails.");

            // Verify that the TimeoutException is logged as part of the exception
            _loggerMock.Verify(l => l.Error(It.Is<Exception>(ex => ex is System.ServiceProcess.TimeoutException),
                It.Is<string>(msg => msg.Contains("An error occurred during service management."))), Times.Once);
        }
        
        [TestMethod]
        public async Task Execute_ShouldLogWarningAndReturnTrue_WhenServiceCannotBeStoppedOrKilled()
        {
            // Arrange
            var serviceName = "TestService";
            var serviceAction = new ServiceAction(serviceName, Curupira.Plugins.ServiceManager.Action.StopOrKill);
            var bundle = new Bundle("stopOrKill_bundle");
            bundle.Services.Add(serviceAction);
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "stopOrKill_bundle" } };

            // Setup serviceControllerMock behavior to simulate that the service cannot be stopped
            _serviceControllerMock.Setup(s => s.CanStop).Returns(false);  // The service cannot be stopped
            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Running);  // Service is running

            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName)).Returns(_serviceControllerMock.Object);

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result, "The plugin should return true even if the service cannot be stopped.");
            _loggerMock.Verify(l => l.Warn(It.Is<string>(msg => msg.Contains($"Service '{serviceName}' is not running or cannot be stopped."))), Times.Once);
        }

        [TestMethod]
        public async Task Execute_ShouldLogWarning_WhenServiceCannotBeStopped()
        {
            // Arrange
            var serviceName = "TestService";
            var serviceAction = new ServiceAction(serviceName, Curupira.Plugins.ServiceManager.Action.Stop);
            var bundle = new Bundle("stop_bundle");
            bundle.Services.Add(serviceAction);
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "stop_bundle" } };

            // Setup serviceControllerMock behavior to simulate a service that cannot be stopped
            _serviceControllerMock.Setup(s => s.CanStop).Returns(false);  // Service cannot be stopped
            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Running); // Service is running

            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName)).Returns(_serviceControllerMock.Object);

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result, "The plugin should return true even if the service cannot be stopped (as per current logic).");

            // Verify that the warning message is logged when the service cannot be stopped
            _loggerMock.Verify(l => l.Warn(It.Is<string>(msg => msg.Contains($"Service '{serviceName}' is not running or cannot be stopped."))), Times.Once);
        }

        [TestMethod]
        public async Task Execute_ShouldLogErrorAndReturnFalse_WhenLogFileIsMissing()
        {
            // Arrange
            var serviceName = "TestService";
            var serviceAction = new ServiceAction(serviceName, Curupira.Plugins.ServiceManager.Action.Status);
            var bundle = new Bundle("status_bundle")
            {
                LogFile = null  // Simulating missing logFile
            };
            bundle.Services.Add(serviceAction);
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "status_bundle" } };

            // Setup serviceControllerMock behavior to simulate a service in the 'Status' action
            _serviceControllerMock.Setup(s => s.ServiceName).Returns(serviceName);
            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Running);

            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName)).Returns(_serviceControllerMock.Object);

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when logFile is missing.");
            _loggerMock.Verify(l => l.Error(It.Is<string>(msg => msg.Contains("To read the status of a service, you need to inform the logFile attribute of the bundle in the config file."))), Times.Once);
        }

        [TestMethod]
        public async Task Execute_ShouldLogErrorAndReturnFalse_WhenIOExceptionIsThrown()
        {
            // Arrange
            var serviceName = "TestService";
            var serviceAction = new ServiceAction(serviceName, Curupira.Plugins.ServiceManager.Action.Status);
            var bundle = new Bundle("status_bundle")
            {
                LogFile = "log.txt"  // Log file is provided
            };
            bundle.Services.Add(serviceAction);
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "status_bundle" } };

            // Setup serviceControllerMock behavior
            _serviceControllerMock.Setup(s => s.ServiceName).Returns(serviceName);
            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Running);

            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName)).Returns(_serviceControllerMock.Object);

            // Simulate an IOException when writing to the log file
            _serviceManagerPluginMock.Protected().Setup("WriteLogFile", ItExpr.IsAny<string>(), ItExpr.IsAny<string>())
                .Throws(new System.IO.IOException("Disk is full"));

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when an IOException occurs.");
            _loggerMock.Verify(l => l.Error(It.IsAny<IOException>(), It.Is<string>(msg => msg.Contains($"Error trying save the status of the service '{serviceName}' into log.txt"))), Times.Once);
        }

        [TestMethod]
        public async Task Execute_ShouldLogErrorAndReturnFalse_WhenGeneralExceptionIsThrown()
        {
            // Arrange
            var serviceName = "TestService";
            var serviceAction = new ServiceAction(serviceName, Curupira.Plugins.ServiceManager.Action.Status);
            var bundle = new Bundle("status_bundle")
            {
                LogFile = "log.txt"  // Log file is provided
            };
            bundle.Services.Add(serviceAction);
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "status_bundle" } };

            // Setup serviceControllerMock behavior
            _serviceControllerMock.Setup(s => s.ServiceName).Returns(serviceName);
            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Running);

            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName)).Returns(_serviceControllerMock.Object);

            // Simulate a general Exception when writing to the log file
            _serviceManagerPluginMock.Protected().Setup("WriteLogFile", ItExpr.IsAny<string>(), ItExpr.IsAny<string>())
                .Throws(new Exception("Unexpected error"));

            // Act
            var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when a general exception occurs.");
            _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(msg => msg.Contains($"Error trying to get the service '{serviceName}' status"))), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldCancelExecution_WhenCancellationTokenIsTriggered_WithMultipleServices()
        {
            // Arrange
            var serviceName1 = "TestService1";
            var serviceName2 = "TestService2";
            var serviceAction1 = new ServiceAction(serviceName1, Curupira.Plugins.ServiceManager.Action.StopOrKill);
            var serviceAction2 = new ServiceAction(serviceName2, Curupira.Plugins.ServiceManager.Action.StopOrKill);

            var bundle = new Bundle("long_execution_bundle");
            bundle.Services.Add(serviceAction1);
            bundle.Services.Add(serviceAction2); // Add multiple services to make the process longer
            _pluginConfig.Bundles.Add(bundle.Id, bundle);

            var commandLineArgs = new Dictionary<string, string> { { "bundle", "long_execution_bundle" } };

            // Setup serviceControllerMock behavior
            _serviceControllerMock.Setup(s => s.CanStop).Returns(true);
            _serviceControllerMock.Setup(s => s.Status).Returns(ServiceControllerStatus.Running);
            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName1)).Returns(_serviceControllerMock.Object);
            _serviceControllerFactoryMock.Setup(f => f.Build(serviceName2)).Returns(_serviceControllerMock.Object);

            // Simulate long-running operations by adding manual delays in the Stop method
            _serviceControllerMock.Setup(s => s.Stop())
                .Callback(() => Task.Delay(2000).Wait());  // Simulate long stop

            using (var sourceToken = new CancellationTokenSource())
            {
                sourceToken.CancelAfter(500);

                var result = await _serviceManagerPluginMock.Object.ExecuteAsync(commandLineArgs, sourceToken.Token).ConfigureAwait(false);

                Assert.IsFalse(result, "The plugin should return false when the cancellation token is triggered.");
            }

            _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.Contains("Plugin execution cancelled."))), Times.Once);
        }
    }
}
