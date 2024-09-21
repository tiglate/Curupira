using CommandLine;

namespace Curupira.AppClient
{
    public class Options
    {
        [Option('p', "plugin", Required = true, HelpText = "The name of the plugin to execute.")]
        public string Plugin { get; set; }

        [Option('l', "level", Default = "Info", HelpText = "The log level (optional). Default is Info.")]
        public string Level { get; set; }

        [Option("params", HelpText = "Additional parameters specific to the plugin (optional).")]
        public string Params { get; set; }
    }
}
