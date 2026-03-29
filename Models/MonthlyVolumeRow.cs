namespace KeywordInsightService.Models
{
    public sealed class MonthlyVolumeRow
    {
        public int Year { get; set; }

        public string Month { get; set; } = string.Empty;

        public long? MonthlySearches { get; set; }
    }
}
