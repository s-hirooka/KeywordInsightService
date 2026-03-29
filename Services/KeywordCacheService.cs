using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KeywordInsightService.Models;

namespace KeywordInsightService.Services
{
    public sealed class KeywordCacheService
    {
        private readonly string _cacheDirectory;
        private readonly int _cacheTtlHours;

        public KeywordCacheService(string baseDirectory, int cacheTtlHours)
        {
            _cacheDirectory = Path.Combine(baseDirectory, "Cache");
            _cacheTtlHours = cacheTtlHours;
            Directory.CreateDirectory(_cacheDirectory);
        }

        public IReadOnlyList<KeywordMetricRow>? TryGet(GoogleAdsKeywordRequest request)
        {
            string path = GetCacheFilePath(request);
            if (!File.Exists(path))
            {
                return null;
            }

            string json = File.ReadAllText(path);
            KeywordCacheContainer? container = JsonSerializer.Deserialize<KeywordCacheContainer>(json, CreateJsonOptions());
            if (container == null)
            {
                return null;
            }

            if (container.CreatedAt.AddHours(_cacheTtlHours) < DateTime.Now)
            {
                return null;
            }

            return container.Rows;
        }

        public void Save(GoogleAdsKeywordRequest request, IReadOnlyList<KeywordMetricRow> rows)
        {
            string path = GetCacheFilePath(request);
            KeywordCacheContainer container = new KeywordCacheContainer
            {
                CreatedAt = DateTime.Now,
                Rows = rows.ToList()
            };

            string json = JsonSerializer.Serialize(container, CreateJsonOptions());
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        private string GetCacheFilePath(GoogleAdsKeywordRequest request)
        {
            string normalized = string.Join(
                "|",
                request.CustomerId.Trim(),
                request.LanguageId.Trim(),
                string.Join(",", request.GeoTargetIds.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)),
                string.Join("\n", request.Keywords.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).Select(x => x.Trim().ToLowerInvariant())));

            using SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalized));
            string hash = Convert.ToHexString(hashBytes);
            return Path.Combine(_cacheDirectory, $"{hash}.json");
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        private sealed class KeywordCacheContainer
        {
            public DateTime CreatedAt { get; set; }

            public List<KeywordMetricRow> Rows { get; set; } = new List<KeywordMetricRow>();
        }
    }
}
