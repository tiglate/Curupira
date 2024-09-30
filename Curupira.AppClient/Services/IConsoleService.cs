namespace Curupira.AppClient.Services
{
    public interface IConsoleService
    {
        void Clear();

        void WriteCentered(string text, bool newLine = true);

        void WriteLine();

        void WriteLine(string value);
    }
}
