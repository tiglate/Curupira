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
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyFromLibFolder;
#endif
        }

        static async Task<int> Main(string[] args)
        {
            // Load config files and set up assembly resolution only in release mode
#if !DEBUG
            ConfigureAppSettings();
            ConfigureNLog();
#endif

            using (var container = AutofacContainerBuilder.Configure())
            {
                using (var runner = new AppRunner(container))
                {
                    return await runner.RunAsync(args).ConfigureAwait(false);
                }
            }
        }

#pragma warning restore S1144 // Unused private types or members should be removed
    }
}
