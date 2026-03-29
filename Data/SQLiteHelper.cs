using System.IO;
using Microsoft.Data.Sqlite;

namespace KeywordInsightService.Data
{
    public sealed class SQLiteHelper
    {
        public string DatabasePath { get; }

        public SQLiteHelper(string baseDirectory, string databaseFileName)
        {
            DatabasePath = Path.Combine(baseDirectory, databaseFileName);
        }

        public SqliteConnection CreateConnection()
        {
            return new SqliteConnection($"Data Source={DatabasePath}");
        }

        public void InitializeDatabase()
        {
            string? directory = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using SqliteConnection connection = CreateConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                @"
CREATE TABLE IF NOT EXISTS KeywordMetricsHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Keyword TEXT NOT NULL,
    AvgMonthlySearches INTEGER NULL,
    CompetitionLevel TEXT NULL,
    CompetitionIndex INTEGER NULL,
    LowTopOfPageBid REAL NULL,
    HighTopOfPageBid REAL NULL,
    RetrievedAt TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS UX_KeywordMetricsHistory_Keyword_RetrievedAt
    ON KeywordMetricsHistory (Keyword, RetrievedAt);
";
            command.ExecuteNonQuery();
        }
    }
}
