using System.IO;

namespace Curupira.WindowsService.Services
{
    public interface IFileUploadService
    {
        void UploadFile(Stream fileStream, string fileName);
    }
}
