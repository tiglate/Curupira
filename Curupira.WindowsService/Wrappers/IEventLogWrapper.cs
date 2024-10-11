using Curupira.WindowsService.Model;
using System;

namespace Curupira.WindowsService.Wrappers
{
    public interface IEventLogWrapper : IDisposable
    {
        int GetEntryCount();
        CustomEventLogEntry GetEntry(int index);
    }
}
