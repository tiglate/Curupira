using Autofac;
using CommandLine;
using Curupira.AppClient.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Curupira.AppClient
{
    public class AppRunner
    {
        private readonly IContainer _container;

        public AppRunner(IContainer container)
        {
            _container = container;
        }

        public async Task<int> RunAsync(string[] args)
        {
            var result = ParseArguments(args);

            return await result.MapResult(
                async options => await RunAsync(options).ConfigureAwait(false),  // Successful parsing
                errors => Task.FromResult(HandleParseError(errors))   // Error handling
            ).ConfigureAwait(false);
        }

        protected virtual async Task<int> RunAsync(Options options)
        {
            ApplyLogLevel(options.Level);

            using (var scope = _container.BeginLifetimeScope())
            {
                var pluginExecutor = scope.Resolve<IPluginExecutor>();
                return await pluginExecutor.ExecutePluginAsync(options).ConfigureAwait(false) ? 0 : 1;
            }
        }

        protected virtual ParserResult<Options> ParseArguments(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args);
        }

        protected virtual int HandleParseError(IEnumerable<Error> errs)
        {
            if (errs.IsHelp())
            {
                return 0; // Error code for success
            }
            Console.WriteLine("Use --help to view usage instructions.");
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
    }
}
