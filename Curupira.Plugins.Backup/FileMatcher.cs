using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Curupira.Plugins.Backup
{
    public class FileMatcher
    {
        private readonly IEnumerable<string> _patterns;
        private readonly string _rootDir;

        public FileMatcher(string rootDir, IEnumerable<string> patterns)
        {
            if (string.IsNullOrWhiteSpace(rootDir))
            {
                throw new ArgumentNullException(nameof(rootDir));
            }

            if (patterns == null)
            {
                throw new ArgumentNullException(nameof(patterns));
            }

            _rootDir = $"{Path.GetFullPath(rootDir).TrimEnd(Path.DirectorySeparatorChar)}{Path.DirectorySeparatorChar}";
            _patterns = patterns.Select(p => p.Replace('/', Path.DirectorySeparatorChar)).ToList();
        }

        public bool IsMatch(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return _patterns.Any(pattern => MatchesPattern(path, $"{_rootDir}{pattern.TrimEnd('\\', '/')}"));
        }

        private bool MatchesPattern(string path, string pattern)
        {
            // Escape special characters in the pattern
            string escapedPattern = Regex.Escape(pattern);

            // Replace wildcard characters with their regular expression equivalents
            string regexPattern = "^" + escapedPattern
                                        .Replace("\\*", ".*") // * matches zero or more characters
                                        .Replace("\\?", ".")  // ? matches any single character
                                 + "$";

            // Perform the regular expression match (case-insensitive)
            return Regex.IsMatch(path, regexPattern, RegexOptions.IgnoreCase);
        }
    }
}
