using ShellProgressBar;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.AppClient.Services
{
    [ExcludeFromCodeCoverage]
    public class ProgressBarService : IProgressBarService
    {
        private const string ProgressBarNotInitialized = "Progressbar not initialized. Please call Init() before using this method.";
        private ProgressBar _progressBar;

        public void Init(int maxTicks, string message)
        {
            var options = new ProgressBarOptions
            {
                CollapseWhenFinished = true,
                EnableTaskBarProgress = true,
            };
            _progressBar = new ProgressBar(maxTicks, message, options);
        }

        public void SetMessage(string message)
        {
            if (_progressBar == null)
            {
                throw new InvalidOperationException(ProgressBarNotInitialized);
            }

            _progressBar.Message = message;
        }

        public void ReportProgress(float percentage)
        {
            if (_progressBar == null)
            {
                throw new InvalidOperationException(ProgressBarNotInitialized);
            }
            _progressBar.AsProgress<float>().Report(percentage);
        }

        public void Dispose()
        {
            _progressBar?.Dispose();
        }
    }
}
