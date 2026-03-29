using System;
using System.Collections.Generic;
using System.IO;

namespace KeywordInsightService.Models
{
    public static class DotEnvLoader
    {
        public static Dictionary<string, string> Load(string baseDirectory)
        {
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string envPath = Path.Combine(baseDirectory, ".env");
            if (!File.Exists(envPath))
            {
                return values;
            }

            foreach (string rawLine in File.ReadAllLines(envPath))
            {
                string line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                int separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = line.Substring(0, separatorIndex).Trim();
                string value = line.Substring(separatorIndex + 1).Trim();

                if ((value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)) ||
                    (value.StartsWith("'", StringComparison.Ordinal) && value.EndsWith("'", StringComparison.Ordinal)))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                values[key] = value;
            }

            return values;
        }
    }
}
