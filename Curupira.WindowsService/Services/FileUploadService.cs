using System;
using System.Configuration;
using System.IO;

namespace Curupira.WindowsService.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly string _uploadDirectory;

        public FileUploadService()
        {
            _uploadDirectory = ConfigurationManager.AppSettings["UploadDirectory"];

            if (string.IsNullOrEmpty(_uploadDirectory))
            {
                throw new ConfigurationErrorsException("Upload directory is not configured.");
            }

            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }
        }

        public void UploadFile(Stream fileStream, string fileName)
        {
            if (fileStream == null || string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File stream or file name cannot be null or empty.");
            }

            var fileExtension = Path.GetExtension(fileName);
            if (fileExtension != ".zip" && fileExtension != ".exe")
            {
                throw new InvalidOperationException("Only .zip and .exe files are allowed.");
            }

            var destinationPath = Path.Combine(_uploadDirectory, fileName);

            using (var fileStreamDestination = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                fileStream.CopyTo(fileStreamDestination);
            }
        }
    }

}
