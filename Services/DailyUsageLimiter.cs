using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeywordInsightService.Services
{
    public sealed class DailyUsageLimiter
    {
        private readonly string _stateFilePath;
        private readonly int _maxDailyApiCalls;
        private readonly int _minimumIntervalMilliseconds;
        private readonly object _sync = new object();

        public DailyUsageLimiter(string baseDirectory, int maxDailyApiCalls, int minimumIntervalMilliseconds)
        {
            _stateFilePath = Path.Combine(baseDirectory, "daily_usage.json");
            _maxDailyApiCalls = maxDailyApiCalls;
            _minimumIntervalMilliseconds = minimumIntervalMilliseconds;
        }

        public DailyUsageState GetStateForToday()
        {
            DailyUsageStorage storage = Load();
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            if (!string.Equals(storage.Date, today, StringComparison.Ordinal))
            {
                return new DailyUsageState
                {
                    TodayCallCount = 0,
                    LastCalledAt = null
                };
            }

            return new DailyUsageState
            {
                TodayCallCount = storage.CallCount,
                LastCalledAt = storage.LastCalledAt
            };
        }

        public void EnsureCanCall()
        {
            DailyUsageState state = GetStateForToday();
            if (state.TodayCallCount >= _maxDailyApiCalls)
            {
                throw new InvalidOperationException($"本日の API 利用上限 {_maxDailyApiCalls} 回に達しました。明日以降に再実行してください。");
            }
        }

        public async Task WaitIfNeededAsync()
        {
            DailyUsageState state = GetStateForToday();
            if (!state.LastCalledAt.HasValue)
            {
                return;
            }

            TimeSpan elapsed = DateTime.Now - state.LastCalledAt.Value;
            int waitMilliseconds = _minimumIntervalMilliseconds - (int)elapsed.TotalMilliseconds;
            if (waitMilliseconds > 0)
            {
                await Task.Delay(waitMilliseconds).ConfigureAwait(false);
            }
        }

        public void RegisterCall()
        {
            lock (_sync)
            {
                string today = DateTime.Today.ToString("yyyy-MM-dd");
                DailyUsageStorage storage = Load();

                if (!string.Equals(storage.Date, today, StringComparison.Ordinal))
                {
                    storage = new DailyUsageStorage
                    {
                        Date = today,
                        CallCount = 0,
                        LastCalledAt = null
                    };
                }

                storage.CallCount += 1;
                storage.LastCalledAt = DateTime.Now;
                Save(storage);
            }
        }

        private DailyUsageStorage Load()
        {
            lock (_sync)
            {
                if (!File.Exists(_stateFilePath))
                {
                    return new DailyUsageStorage
                    {
                        Date = DateTime.Today.ToString("yyyy-MM-dd"),
                        CallCount = 0
                    };
                }

                string json = File.ReadAllText(_stateFilePath, Encoding.UTF8);
                DailyUsageStorage? storage = JsonSerializer.Deserialize<DailyUsageStorage>(json);
                return storage ?? new DailyUsageStorage
                {
                    Date = DateTime.Today.ToString("yyyy-MM-dd"),
                    CallCount = 0
                };
            }
        }

        private void Save(DailyUsageStorage storage)
        {
            string json = JsonSerializer.Serialize(storage, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_stateFilePath, json, Encoding.UTF8);
        }

        private sealed class DailyUsageStorage
        {
            public string Date { get; set; } = string.Empty;

            public int CallCount { get; set; }

            public DateTime? LastCalledAt { get; set; }
        }
    }

    public sealed class DailyUsageState
    {
        public int TodayCallCount { get; set; }

        public DateTime? LastCalledAt { get; set; }

        public string LastCalledAtText
        {
            get
            {
                return LastCalledAt.HasValue
                    ? LastCalledAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "-";
            }
        }
    }
}
