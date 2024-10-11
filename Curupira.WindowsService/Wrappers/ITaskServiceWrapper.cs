using Microsoft.Win32.TaskScheduler;
using System;

namespace Curupira.WindowsService.Wrappers
{
    public interface ITaskServiceWrapper : IDisposable
    {
        TaskFolder RootFolder { get; }
    }
}
