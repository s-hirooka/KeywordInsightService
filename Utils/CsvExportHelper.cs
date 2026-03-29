using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using KeywordInsightService.Models;

namespace KeywordInsightService.Utils
{
    public static class CsvExportHelper
    {
        public static void ExportKeywordMetrics(string filePath, IEnumerable<KeywordMetricRow> rows, Encoding encoding)
        {
            using StreamWriter writer = new StreamWriter(filePath, false, encoding);
            writer.WriteLine("Keyword,AvgMonthlySearches,CompetitionLevel,CompetitionIndex,LowTopOfPageBid,HighTopOfPageBid,RetrievedAt");

            foreach (KeywordMetricRow row in rows)
            {
                string line = string.Join(",",
                    Escape(row.Keyword),
                    Escape(row.AvgMonthlySearches?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    Escape(row.CompetitionLevel ?? string.Empty),
                    Escape(row.CompetitionIndex?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    Escape(row.LowTopOfPageBid?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    Escape(row.HighTopOfPageBid?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                    Escape(row.RetrievedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));

                writer.WriteLine(line);
            }
        }

        private static string Escape(string value)
        {
            string escaped = value.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }
    }
}
