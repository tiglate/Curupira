using Autofac;
using CommandLine;
using Curupira.AppClient.Services;
using Curupira.Plugins.Contract;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Curupira.AppClient
{
    public class AppRunner : IDisposable
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IConsoleService _consoleService;
        private readonly IAutofacHelper _autofacHelper;

        public AppRunner(IContainer container)
        {
            _lifetimeScope = container.BeginLifetimeScope();
            _consoleService = _lifetimeScope.Resolve<IConsoleService>();
            _autofacHelper = _lifetimeScope.Resolve<IAutofacHelper>();
        }

        ~AppRunner()
        {
            Dispose(false);
        }

        public async Task<int> RunAsync(string[] args)
        {
            var result = ParseArguments(args);

            return await result.MapResult(
                async options =>
                {
                    if (!options.IsValid(result))
                    {
                        return 1; // Return 1 if validation fails
                    }

                    return await RunAsync(options).ConfigureAwait(false);  // Successful parsing
                },
                errors => Task.FromResult(HandleParseError(errors))   // Error handling
            ).ConfigureAwait(false);
        }

        protected virtual async Task<int> RunAsync(Options options)
        {
            ApplyLogLevel(options.Level);

            if (options.ListPlugins)
            {
                ListAvailablePlugins();
                return await Task.FromResult(0);
            }

            if (!options.NoLogo)
            {
                ShowBanner();
            }

            var pluginExecutor = _lifetimeScope.Resolve<IPluginExecutor>();
            return await pluginExecutor.ExecutePluginAsync(options).ConfigureAwait(false) ? 0 : 1;
        }

        protected virtual ParserResult<Options> ParseArguments(string[] args)
        {
            return Parser.Default.ParseArguments(() => new Options(_consoleService), args);
        }

        protected virtual void ListAvailablePlugins()
        {
            var implementations = _autofacHelper.GetNamedImplementationsOfInterface<IPlugin>();

            foreach (var (Name, _) in implementations)
            {
                _consoleService.WriteLine(Name);
            }
        }

        protected virtual void ShowBanner()
        {
            _consoleService.Clear();
            _consoleService.WriteLine();
            _consoleService.WriteCentered(@" ██████╗██╗   ██╗██████╗ ██╗   ██╗██████╗ ██╗██████╗  █████╗ ");
            _consoleService.WriteCentered(@"██╔════╝██║   ██║██╔══██╗██║   ██║██╔══██╗██║██╔══██╗██╔══██╗");
            _consoleService.WriteCentered(@"██║     ██║   ██║██████╔╝██║   ██║██████╔╝██║██████╔╝███████║");
            _consoleService.WriteCentered(@"██║     ██║   ██║██╔══██╗██║   ██║██╔═══╝ ██║██╔══██╗██╔══██║");
            _consoleService.WriteCentered(@"╚██████╗╚██████╔╝██║  ██║╚██████╔╝██║     ██║██║  ██║██║  ██║");
            _consoleService.WriteCentered(@" ╚═════╝ ╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚═╝     ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝");
            _consoleService.WriteCentered(@"                                                             ");
        }

        protected virtual int HandleParseError(IEnumerable<Error> errs)
        {
            if (errs.IsHelp())
            {
                return 0; // Error code for success
            }
            _consoleService.WriteLine("Use --help to view usage instructions.");
            return 1; // Error code for failure
        }

        protected virtual void ApplyLogLevel(string logLevelSetting)
        {
            logLevelSetting = !string.IsNullOrWhiteSpace(logLevelSetting)
                ? logLevelSetting
                : ConfigurationManager.AppSettings["LogLevel"];

            LogLevel logLevel;
            switch (logLevelSetting?.ToUpper())
            {
                case "OFF":
                    logLevel = LogLevel.Off;
                    break;
                case "TRACE":
                    logLevel = LogLevel.Trace;
                    break;
                case "DEBUG":
                    logLevel = LogLevel.Debug;
                    break;
                case "INFO":
                    logLevel = LogLevel.Info;
                    break;
                case "WARN":
                    logLevel = LogLevel.Warn;
                    break;
                case "ERROR":
                    logLevel = LogLevel.Error;
                    break;
                case "FATAL":
                    logLevel = LogLevel.Fatal;
                    break;
                default:
                    logLevel = LogLevel.Info; // Default to Info level if not specified or invalid
                    break;
            }

            var config = LogManager.Configuration;
            var consoleRule = config.FindRuleByName("consoleRule");
            consoleRule?.SetLoggingLevels(logLevel, LogLevel.Fatal);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _lifetimeScope?.Dispose();
            }
        }
    }
}
