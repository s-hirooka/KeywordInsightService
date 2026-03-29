using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Data.Sqlite;
using KeywordInsightService.Models;

namespace KeywordInsightService.Data
{
    public sealed class KeywordMetricsRepository
    {
        private readonly SQLiteHelper _sqliteHelper;

        public KeywordMetricsRepository(SQLiteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
        }

        public int InsertOrIgnore(IEnumerable<KeywordMetricRow> rows)
        {
            int insertedCount = 0;

            using SqliteConnection connection = _sqliteHelper.CreateConnection();
            connection.Open();
            using SqliteTransaction transaction = connection.BeginTransaction();

            foreach (KeywordMetricRow row in rows)
            {
                using SqliteCommand command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText =
                    @"
INSERT OR IGNORE INTO KeywordMetricsHistory
(
    Keyword,
    AvgMonthlySearches,
    CompetitionLevel,
    CompetitionIndex,
    LowTopOfPageBid,
    HighTopOfPageBid,
    RetrievedAt
)
VALUES
(
    $Keyword,
    $AvgMonthlySearches,
    $CompetitionLevel,
    $CompetitionIndex,
    $LowTopOfPageBid,
    $HighTopOfPageBid,
    $RetrievedAt
);";

                command.Parameters.AddWithValue("$Keyword", row.Keyword);
                command.Parameters.AddWithValue("$AvgMonthlySearches", (object?)row.AvgMonthlySearches ?? DBNull.Value);
                command.Parameters.AddWithValue("$CompetitionLevel", (object?)row.CompetitionLevel ?? DBNull.Value);
                command.Parameters.AddWithValue("$CompetitionIndex", (object?)row.CompetitionIndex ?? DBNull.Value);
                command.Parameters.AddWithValue("$LowTopOfPageBid", (object?)row.LowTopOfPageBid ?? DBNull.Value);
                command.Parameters.AddWithValue("$HighTopOfPageBid", (object?)row.HighTopOfPageBid ?? DBNull.Value);
                command.Parameters.AddWithValue("$RetrievedAt", NormalizeRetrievedAt(row.RetrievedAt));

                insertedCount += command.ExecuteNonQuery();
            }

            transaction.Commit();
            return insertedCount;
        }

        public IReadOnlyList<KeywordMetricRow> GetAll()
        {
            List<KeywordMetricRow> rows = new List<KeywordMetricRow>();

            using SqliteConnection connection = _sqliteHelper.CreateConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                @"
SELECT
    Keyword,
    AvgMonthlySearches,
    CompetitionLevel,
    CompetitionIndex,
    LowTopOfPageBid,
    HighTopOfPageBid,
    RetrievedAt
FROM KeywordMetricsHistory
ORDER BY RetrievedAt DESC, Keyword ASC;";

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new KeywordMetricRow
                {
                    Keyword = reader.GetString(0),
                    AvgMonthlySearches = reader.IsDBNull(1) ? null : reader.GetInt64(1),
                    CompetitionLevel = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CompetitionIndex = reader.IsDBNull(3) ? null : reader.GetInt64(3),
                    LowTopOfPageBid = reader.IsDBNull(4) ? null : Convert.ToDecimal(reader.GetDouble(4), CultureInfo.InvariantCulture),
                    HighTopOfPageBid = reader.IsDBNull(5) ? null : Convert.ToDecimal(reader.GetDouble(5), CultureInfo.InvariantCulture),
                    RetrievedAt = DateTime.ParseExact(reader.GetString(6), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                });
            }

            return rows;
        }

        private static string NormalizeRetrievedAt(DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}
