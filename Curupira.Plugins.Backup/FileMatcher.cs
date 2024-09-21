using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Curupira.Plugins.Backup
{
    internal class FileMatcher
    {
        private readonly IList<string> _patterns;
        private readonly string _rootDir;

        public FileMatcher(string rootDir, IList<string> patterns)
        {
            _rootDir = $"{Path.GetFullPath(rootDir).TrimEnd(Path.DirectorySeparatorChar)}{Path.DirectorySeparatorChar}";
            _patterns = patterns.Select(p => p.ToLower().Replace('\\', Path.DirectorySeparatorChar)).ToList();
        }

        public bool IsMatch(string path)
        {
            // Normalize the path to match the root directory and patterns
            string relativePath = GetRelativePath(path);

            if (relativePath == null)
            {
                return false; // If the file isn't under the root directory
            }

            foreach (var pattern in _patterns)
            {
                if (PatternMatch(relativePath, pattern))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetRelativePath(string fullPath)
        {
            string normalizedFullPath = Path.GetFullPath(fullPath).ToLower();

            if (normalizedFullPath.StartsWith(_rootDir.ToLower()))
            {
                return normalizedFullPath.Substring(_rootDir.Length).TrimStart(Path.DirectorySeparatorChar);
            }

            return null; // Path is not under the root directory
        }

        private bool PatternMatch(string relativePath, string pattern)
        {
            string[] pathSegments = relativePath.Split(Path.DirectorySeparatorChar);
            string[] patternSegments = pattern.Split(Path.DirectorySeparatorChar);

            int pathLength = pathSegments.Length;
            int patternLength = patternSegments.Length;

            for (int i = 0; i < patternLength; i++)
            {
                if (i >= pathLength)
                {
                    return false;
                }

                string patternSegment = patternSegments[i];
                string pathSegment = pathSegments[i];

                if (patternSegment == "*")
                {
                    // Matches any directory or file name
                    continue;
                }

                if (patternSegment.Contains("*"))
                {
                    // Handle wildcard patterns like *.txt or folder/*.txt
                    if (!WildcardMatch(pathSegment, patternSegment))
                    {
                        return false;
                    }
                }
                else if (!patternSegment.Equals(pathSegment, StringComparison.OrdinalIgnoreCase))
                {
                    return false; // Non-wildcard segments must match exactly
                }
            }

            // Check for exact match or wildcard match if there's still pattern left
            return pathLength == patternLength || patternSegments.Last() == "*";
        }

        private bool WildcardMatch(string fileName, string pattern)
        {
            string[] patternParts = pattern.Split('*');
            int lastIndex = 0;

            foreach (var part in patternParts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                int index = fileName.IndexOf(part, lastIndex, StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                {
                    return false;
                }

                lastIndex = index + part.Length;
            }

            return true;
        }
    }
}
