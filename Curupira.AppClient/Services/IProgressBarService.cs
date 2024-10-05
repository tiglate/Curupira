using System;

namespace Curupira.AppClient.Services
{
    public interface IProgressBarService : IDisposable
    {
        void Init(int maxTicks, string message);

        void SetMessage(string message);

        void ReportProgress(float percentage);
    }
}
