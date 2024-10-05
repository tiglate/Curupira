using Curupira.Plugins.Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Curupira.Plugins.Backup
{
    public class FileMatcher
    {
        private readonly IEnumerable<string> _patterns;
        private readonly string _rootDir;
        private readonly ILogProvider _logger;

        public FileMatcher(string rootDir, IEnumerable<string> patterns, ILogProvider logger = null)
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
            _logger = logger;
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

            try
            {
                var matchTimeout = TimeSpan.FromSeconds(1);

                // Perform the regular expression match (case-insensitive)
                return Regex.IsMatch(path, regexPattern, RegexOptions.IgnoreCase, matchTimeout);
            }
            catch (RegexMatchTimeoutException)
            {
                if (_logger != null)
                {
                    _logger.Warn($"Regex match timed out for pattern: {pattern}");
                }
                else
                {
                    Debug.WriteLine($"Regex match timed out for pattern: {pattern}");
                }
                return false;
            }
        }
    }
}
