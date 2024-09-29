using System;

namespace Curupira.Plugins.Common
{
    public static class FileSystemHelper
    {
        public static string GetFirstExistingDirectoryOrRoot(string directoryPath)
        {
            string currentDirectory = directoryPath;
            while (!string.IsNullOrEmpty(currentDirectory) && !DirectoryExists(currentDirectory))
            {
                if (currentDirectory != null && currentDirectory.EndsWith(":"))
                {
                    return string.Empty;
                }
                var aux = GetParentDirectory(currentDirectory);
                if (aux == currentDirectory || string.IsNullOrEmpty(aux))
                {
                    break;
                }
                else
                {
                    currentDirectory = aux;
                }
            }
            return currentDirectory;
        }

        /// <summary>
        /// Checks if a directory exists, handling both local paths and network shares.
        /// </summary>
        public static bool DirectoryExists(string path)
        {
            return Alphaleonis.Win32.Filesystem.Directory.Exists(path);
        }

        /// <summary>
        /// Gets the parent directory of the specified path.
        /// </summary>
        public static string GetParentDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            path = path.TrimEnd('/', '\\');
            int lastSeparatorIndex = path.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
            if (path.StartsWith(@"\\") && lastSeparatorIndex <= 2)
            {
                return path;
            }
            if (lastSeparatorIndex == -1 && path.EndsWith(":", StringComparison.Ordinal))
            {
                return path;
            }
            return path.Substring(0, lastSeparatorIndex);
        }
    }
}