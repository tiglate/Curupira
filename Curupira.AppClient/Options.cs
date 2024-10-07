using CommandLine;
using CommandLine.Text;
using Curupira.AppClient.Services;
using System.Text.Json;

namespace Curupira.AppClient
{
    public class Options
    {
        private readonly IConsoleService _consoleService;

        public Options(IConsoleService consoleService)
        {
            _consoleService = consoleService;
        }

        [Option('p', "plugin", HelpText = "The name of the plugin to execute.")]
        public string Plugin { get; set; }

        [Option('l', "level", HelpText = "The log level (optional). Default is Info.")]
        public string Level { get; set; }

        [Option('n', "no-logo", Default = false, HelpText = "It hides the application logo.")]
        public bool NoLogo { get; set; }

        [Option('b', "no-progressbar", Default = false, HelpText = "It hides the progress bar.")]
        public bool NoProgressBar { get; set; }

        [Option('a', "list-plugins", Default = false, HelpText = "List all plugins available.")]
        public bool ListPlugins { get; set; }

        [Option("params", HelpText = "Additional parameters specific to the plugin (optional).")]
        public string Params { get; set; }

        public bool IsValid(ParserResult<Options> result)
        {
            // Ensure either -p (plugin) or -a (list-plugins) is provided, but not both
            if (!ListPlugins && string.IsNullOrEmpty(Plugin))
            {
                // Neither -a nor -p was provided
                _consoleService.WriteLine("Error: You must provide either '-p' to specify a plugin or '-a' to list available plugins.");

                ShowDefaultHelp(result);

                return false;
            }

            if (ListPlugins && !string.IsNullOrEmpty(Plugin))
            {
                // Both -a and -p were provided
                _consoleService.WriteLine("Error: You cannot specify both '-a' (list plugins) and '-p' (plugin) at the same time.");

                ShowDefaultHelp(result);

                return false;
            }

            return true;
        }

        // Generate and show the default help text
        private void ShowDefaultHelp(ParserResult<Options> result)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            _consoleService.WriteLine(helpText);
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
