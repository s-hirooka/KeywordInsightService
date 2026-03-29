using System.Collections.Generic;

namespace KeywordInsightService.Models
{
    public sealed class GoogleAdsKeywordRequest
    {
        public string CustomerId { get; set; } = string.Empty;

        public string LanguageId { get; set; } = string.Empty;

        public List<string> GeoTargetIds { get; set; } = new List<string>();

        public List<string> Keywords { get; set; } = new List<string>();
    }
}
