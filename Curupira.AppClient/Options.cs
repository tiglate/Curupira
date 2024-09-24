using CommandLine;
using System.Text.Json;

namespace Curupira.AppClient
{
    public class Options
    {
        [Option('p', "plugin", Required = true, HelpText = "The name of the plugin to execute.")]
        public string Plugin { get; set; }

        [Option('l', "level", Default = "Info", HelpText = "The log level (optional). Default is Info.")]
        public string Level { get; set; }

        [Option('n', "no-logo", Default = false, HelpText = "It hides the application logo.")]
        public bool NoLogo { get; set; }

        [Option('b', "no-progressbar", Default = false, HelpText = "It hides the progress bar.")]
        public bool NoProgressBar { get; set; }

        [Option("params", HelpText = "Additional parameters specific to the plugin (optional).")]
        public string Params { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
