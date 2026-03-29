using System;
using System.Collections.Generic;

namespace KeywordInsightService.Models
{
    public sealed class KeywordMetricRow
    {
        public string Keyword { get; set; } = string.Empty;

        public long? AvgMonthlySearches { get; set; }

        public string? CompetitionLevel { get; set; }

        public long? CompetitionIndex { get; set; }

        public decimal? LowTopOfPageBid { get; set; }

        public decimal? HighTopOfPageBid { get; set; }

        public DateTime RetrievedAt { get; set; }

        public List<MonthlyVolumeRow> MonthlyVolumes { get; set; } = new List<MonthlyVolumeRow>();
    }
}
