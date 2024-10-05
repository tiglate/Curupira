using Curupira.WindowsService.Model;

namespace Curupira.WindowsService.Services
{
    public interface ISystemUsageService
    {
        SystemUsageModel GetSystemUsage();
    }
}
