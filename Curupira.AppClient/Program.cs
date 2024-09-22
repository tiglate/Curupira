using Autofac;
using CommandLine;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Curupira.AppClient
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);

            return await result.MapResult(
                async options => await RunApplicationAsync(options).ConfigureAwait(false),  // Successful parsing
                errors => Task.FromResult(HandleParseError(errors))   // Error handling
            ).ConfigureAwait(false);
        }

        private static async Task<int> RunApplicationAsync(Options options)
        {
            ApplyLogLevel(options.Level);
            ShowBanner();

            var container = Startup.ConfigureServices(options);
            using (var scope = container.BeginLifetimeScope())
            {
                var pluginExecutor = scope.Resolve<IPluginExecutor>();
                return await pluginExecutor.ExecutePluginAsync(options).ConfigureAwait(false) ? 0 : 1;
            }
        }

        private static int HandleParseError(IEnumerable<Error> errs)
        {
            if (errs.IsHelp())
            {
                return 0; // Error code for success
            }
            Console.WriteLine("Use --help to view usage instructions.");
            return 1; // Error code for failure
        }

        private static void ApplyLogLevel(string logLevelSetting)
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

        private static void ShowBanner()
        {
            Console.Clear();
            Console.WriteLine();
            WriteCentered(@" ██████╗██╗   ██╗██████╗ ██╗   ██╗██████╗ ██╗██████╗  █████╗ ");
            WriteCentered(@"██╔════╝██║   ██║██╔══██╗██║   ██║██╔══██╗██║██╔══██╗██╔══██╗");
            WriteCentered(@"██║     ██║   ██║██████╔╝██║   ██║██████╔╝██║██████╔╝███████║");
            WriteCentered(@"██║     ██║   ██║██╔══██╗██║   ██║██╔═══╝ ██║██╔══██╗██╔══██║");
            WriteCentered(@"╚██████╗╚██████╔╝██║  ██║╚██████╔╝██║     ██║██║  ██║██║  ██║");
            WriteCentered(@" ╚═════╝ ╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚═╝     ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝");
            WriteCentered(@"                                                             ");
            WriteCentered(" -=-=- Automation Tool -=-=-");
            Console.WriteLine();
        }

        /// <summary>
        /// Centers the given text within the console window.
        /// </summary>
        /// <param name="text">The text to center.</param>
        public static void WriteCentered(string text, bool newLine = true)
        {
            // Get the console window width
            int consoleWidth = Console.WindowWidth;

            // Calculate the padding required on each side
            int padding = (consoleWidth - text.Length) / 2;

            // Write the padding and then the text
            Console.Write(new string(' ', padding));
            if (newLine)
            {
                Console.WriteLine(text);
            }
            else
            {
                Console.Write(text);
            }
        }
    }
}