﻿using System.Collections.Generic;
using System.IO;

namespace Curupira.WindowsService.Infra
{
    public static class EnvFileParser
    {
        /// <summary>
        /// Parses a .env file and returns a dictionary of environment variables.
        /// </summary>
        /// <param name="filePath">The path to the .env file.</param>
        /// <returns>A dictionary where keys are variable names and values are their corresponding values.</returns>
        public static Dictionary<string, string> Parse(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new System.ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            var environmentVariables = new Dictionary<string, string>();

            foreach (var line in File.ReadAllLines(filePath))
            {
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
                    continue;

                // Split the line into key-value pair
                int equalsIndex = line.IndexOf('=');
                if (equalsIndex > 0)
                {
                    string key = line.Substring(0, equalsIndex).Trim();
                    string value = line.Substring(equalsIndex + 1).Trim();

                    if (value.Contains("#"))
                    {
                        value = value.Substring(0, value.IndexOf("#")).Trim();
                    }

                    environmentVariables[key] = value;
                }
            }

            return environmentVariables;
        }
    }
}
