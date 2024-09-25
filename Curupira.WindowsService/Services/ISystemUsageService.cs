using Curupira.WindowsService.Model;

namespace Curupira.WindowsService.Service
{
    public interface ISystemUsageService
    {
        SystemUsageModel GetSystemUsage();
    }
}
