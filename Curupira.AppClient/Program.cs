using Curupira.AppClient.Infra.IoC;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Curupira.AppClient
{
    [ExcludeFromCodeCoverage]
    static class Program
    {
        static Program()
        {
#if !DEBUG
            System.AppDomain.CurrentDomain.AssemblyResolve += AppConfigurationHelper.ResolveAssemblyFromLibFolder;
#endif
        }

        static async Task<int> Main(string[] args)
        {
#if !DEBUG
            // Load config files and set up assembly resolution only in release mode
            AppConfigurationHelper.ConfigureAppSettings("Curupira.exe.config");
            AppConfigurationHelper.ConfigureNLog();
#endif

            using (var container = AutofacContainerBuilder.Configure())
            {
                using (var runner = new AppRunner(container))
                {
                    return await runner.RunAsync(args).ConfigureAwait(false);
                }
            }
        }
    }
}
