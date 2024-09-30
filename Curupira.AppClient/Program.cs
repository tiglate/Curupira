using Curupira.AppClient.Infra.IoC;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Curupira.AppClient
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            using (var container = AutofacContainerBuilder.Configure())
            {
                var runner = new AppRunner(container);
                return await runner.RunAsync(args);
            }
        }
    }
}