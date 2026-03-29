using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V22.Common;
using Google.Ads.GoogleAds.V22.Enums;
using Google.Ads.GoogleAds.V22.Resources;
using Google.Ads.GoogleAds.V22.Services;
using KeywordInsightService.Models;

namespace KeywordInsightService.Services
{
    public sealed class GoogleAdsKeywordMetricsService
    {
        private readonly AppSettings _appSettings;
        private readonly KeywordCacheService _keywordCacheService;
        private readonly DailyUsageLimiter _dailyUsageLimiter;

        public GoogleAdsKeywordMetricsService(
            AppSettings appSettings,
            KeywordCacheService keywordCacheService,
            DailyUsageLimiter dailyUsageLimiter)
        {
            _appSettings = appSettings;
            _keywordCacheService = keywordCacheService;
            _dailyUsageLimiter = dailyUsageLimiter;
        }

        public async Task<IReadOnlyList<KeywordMetricRow>> GetHistoricalMetricsAsync(
            GoogleAdsKeywordRequest request,
            bool useCache)
        {
            ValidateRequest(request);

            if (useCache)
            {
                IReadOnlyList<KeywordMetricRow>? cachedRows = _keywordCacheService.TryGet(request);
                if (cachedRows != null)
                {
                    return cachedRows;
                }
            }

            _dailyUsageLimiter.EnsureCanCall();
            await _dailyUsageLimiter.WaitIfNeededAsync().ConfigureAwait(false);

            IReadOnlyList<KeywordMetricRow> fetchedRows = await Task.Run(() =>
            {
                GoogleAdsClient client = CreateClient();
                KeywordPlanIdeaServiceClient keywordPlanIdeaService = client.GetService(Google.Ads.GoogleAds.Services.V22.KeywordPlanIdeaService);

                GenerateKeywordHistoricalMetricsRequest apiRequest = new GenerateKeywordHistoricalMetricsRequest
                {
                    CustomerId = request.CustomerId,
                    Language = LanguageConstantName.FromCriterion(request.LanguageId).ToString(),
                    KeywordPlanNetwork = KeywordPlanNetworkEnum.Types.KeywordPlanNetwork.GoogleSearch
                };

                apiRequest.Keywords.AddRange(request.Keywords);
                apiRequest.GeoTargetConstants.AddRange(
                    request.GeoTargetIds.Select(x => GeoTargetConstantName.FromCriterion(x).ToString()));

                GenerateKeywordHistoricalMetricsResponse response =
                    keywordPlanIdeaService.GenerateKeywordHistoricalMetrics(apiRequest);

                DateTime retrievedAt = DateTime.Now;
                List<KeywordMetricRow> rows = new List<KeywordMetricRow>();

                foreach (GenerateKeywordHistoricalMetricsResult result in response.Results)
                {
                    KeywordPlanHistoricalMetrics metrics = result.KeywordMetrics;
                    KeywordMetricRow row = new KeywordMetricRow
                    {
                        Keyword = result.Text,
                        AvgMonthlySearches = metrics.HasAvgMonthlySearches ? metrics.AvgMonthlySearches : null,
                        CompetitionLevel = metrics.Competition.ToString(),
                        CompetitionIndex = metrics.HasCompetitionIndex ? metrics.CompetitionIndex : null,
                        LowTopOfPageBid = metrics.HasLowTopOfPageBidMicros ? ConvertMicrosToDecimal(metrics.LowTopOfPageBidMicros) : null,
                        HighTopOfPageBid = metrics.HasHighTopOfPageBidMicros ? ConvertMicrosToDecimal(metrics.HighTopOfPageBidMicros) : null,
                        RetrievedAt = retrievedAt,
                        MonthlyVolumes = metrics.MonthlySearchVolumes
                            .Select(x => new MonthlyVolumeRow
                            {
                                Year = (int)x.Year,
                                Month = x.Month.ToString(),
                                MonthlySearches = x.MonthlySearches
                            })
                            .ToList()
                    };

                    rows.Add(row);
                }

                return (IReadOnlyList<KeywordMetricRow>)rows;
            }).ConfigureAwait(false);

            _dailyUsageLimiter.RegisterCall();

            if (useCache)
            {
                _keywordCacheService.Save(request, fetchedRows);
            }

            return fetchedRows;
        }

        private GoogleAdsClient CreateClient()
        {
            GoogleAdsConfig config = new GoogleAdsConfig
            {
                DeveloperToken = _appSettings.GoogleAds.DeveloperToken,
                OAuth2ClientId = _appSettings.GoogleAds.OAuth2ClientId,
                OAuth2ClientSecret = _appSettings.GoogleAds.OAuth2ClientSecret,
                OAuth2RefreshToken = _appSettings.GoogleAds.OAuth2RefreshToken,
                UseGrpcCore = _appSettings.GoogleAds.UseGrpcCore
            };

            string? loginCustomerId = NormalizeCustomerId(_appSettings.GoogleAds.LoginCustomerId);
            if (!string.IsNullOrWhiteSpace(loginCustomerId))
            {
                config.LoginCustomerId = loginCustomerId;
            }

            return new GoogleAdsClient(config);
        }

        private void ValidateRequest(GoogleAdsKeywordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerId))
            {
                throw new InvalidOperationException("Customer ID が未指定です。");
            }

            if (string.IsNullOrWhiteSpace(request.LanguageId))
            {
                throw new InvalidOperationException("Language ID が未指定です。");
            }

            if (request.Keywords.Count == 0)
            {
                throw new InvalidOperationException("キーワードが未指定です。");
            }

            if (request.GeoTargetIds.Count == 0)
            {
                throw new InvalidOperationException("Geo Target IDs が未指定です。");
            }

            if (ContainsPlaceholder(_appSettings.GoogleAds.DeveloperToken) ||
                ContainsPlaceholder(_appSettings.GoogleAds.OAuth2ClientId) ||
                ContainsPlaceholder(_appSettings.GoogleAds.OAuth2ClientSecret) ||
                ContainsPlaceholder(_appSettings.GoogleAds.OAuth2RefreshToken))
            {
                throw new InvalidOperationException(".env の Google Ads 認証情報を実値に置き換えてください。");
            }
        }

        private static bool ContainsPlaceholder(string value)
        {
            return string.IsNullOrWhiteSpace(value) || value.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);
        }

        private static string? NormalizeCustomerId(string? customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return null;
            }

            return customerId.Replace("-", string.Empty).Trim();
        }

        private static decimal ConvertMicrosToDecimal(long micros)
        {
            return decimal.Round(micros / 1000000m, 6);
        }
    }
}
