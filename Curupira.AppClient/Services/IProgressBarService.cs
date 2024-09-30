namespace Curupira.AppClient.Services
{
    public interface IProgressBarService
    {
        void Init(int maxTicks, string message);

        void SetMessage(string message);

        void ReportProgress(float percentage);
    }
}
