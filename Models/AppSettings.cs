using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeywordInsightService.Models
{
    public sealed class AppSettings
    {
        public GoogleAdsSettings GoogleAds { get; set; } = new GoogleAdsSettings();

        public LimitSettings Limits { get; set; } = new LimitSettings();

        public DatabaseSettings Database { get; set; } = new DatabaseSettings();

        public DefaultValueSettings Defaults { get; set; } = new DefaultValueSettings();

        public static AppSettings LoadOrCreate(string baseDirectory)
        {
            string path = Path.Combine(baseDirectory, "appsettings.json");
            AppSettings settings;

            if (!File.Exists(path))
            {
                settings = CreateDefault();
                string createdJson = JsonSerializer.Serialize(settings, CreateJsonSerializerOptions());
                File.WriteAllText(path, createdJson);
            }
            else
            {
                string json = File.ReadAllText(path);
                settings = JsonSerializer.Deserialize<AppSettings>(json, CreateJsonSerializerOptions()) ?? CreateDefault();
            }

            settings.GoogleAds.ApplyEnvironmentOverrides(baseDirectory);
            return settings;
        }

        public static AppSettings CreateDefault()
        {
            return new AppSettings
            {
                GoogleAds = new GoogleAdsSettings(),
                Limits = new LimitSettings(),
                Database = new DatabaseSettings(),
                Defaults = new DefaultValueSettings()
            };
        }

        private static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };
        }
    }

    public sealed class GoogleAdsSettings
    {
        public string DeveloperToken { get; set; } = string.Empty;

        public string OAuth2ClientId { get; set; } = string.Empty;

        public string OAuth2ClientSecret { get; set; } = string.Empty;

        public string OAuth2RefreshToken { get; set; } = string.Empty;

        public string LoginCustomerId { get; set; } = string.Empty;

        public string DefaultCustomerId { get; set; } = string.Empty;

        public bool UseGrpcCore { get; set; }

        public void ApplyEnvironmentOverrides(string baseDirectory)
        {
            Dictionary<string, string> envValues = DotEnvLoader.Load(baseDirectory);

            DeveloperToken = ResolveValue(envValues, "GOOGLE_ADS_DEVELOPER_TOKEN", DeveloperToken);
            OAuth2ClientId = ResolveValue(envValues, "GOOGLE_ADS_OAUTH2_CLIENT_ID", OAuth2ClientId);
            OAuth2ClientSecret = ResolveValue(envValues, "GOOGLE_ADS_OAUTH2_CLIENT_SECRET", OAuth2ClientSecret);
            OAuth2RefreshToken = ResolveValue(envValues, "GOOGLE_ADS_OAUTH2_REFRESH_TOKEN", OAuth2RefreshToken);
            LoginCustomerId = ResolveValue(envValues, "GOOGLE_ADS_LOGIN_CUSTOMER_ID", LoginCustomerId);
            DefaultCustomerId = ResolveValue(envValues, "GOOGLE_ADS_DEFAULT_CUSTOMER_ID", DefaultCustomerId);

            string useGrpcCoreText = ResolveValue(envValues, "GOOGLE_ADS_USE_GRPC_CORE", UseGrpcCore.ToString());
            if (bool.TryParse(useGrpcCoreText, out bool useGrpcCore))
            {
                UseGrpcCore = useGrpcCore;
            }
        }

        private static string ResolveValue(Dictionary<string, string> envValues, string key, string fallback)
        {
            if (envValues.TryGetValue(key, out string? envFileValue) && !string.IsNullOrWhiteSpace(envFileValue))
            {
                return envFileValue.Trim();
            }

            string? processValue = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrWhiteSpace(processValue))
            {
                return processValue.Trim();
            }

            return fallback;
        }
    }

    public sealed class LimitSettings
    {
        public int MaxKeywordsPerRequest { get; set; } = 30;

        public int MaxDailyApiCalls { get; set; } = 10;

        public int MinimumIntervalMilliseconds { get; set; } = 1200;

        public int CacheTtlHours { get; set; } = 720;
    }

    public sealed class DatabaseSettings
    {
        public string DatabaseFileName { get; set; } = "keyword_metrics.db";
    }

    public sealed class DefaultValueSettings
    {
        public string DefaultLanguageId { get; set; } = "1005";

        public string DefaultGeoTargetIds { get; set; } = "2392";

        public bool DefaultUseCache { get; set; } = true;
    }
}
