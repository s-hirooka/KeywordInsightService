using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KeywordInsightService.Data;
using KeywordInsightService.Models;
using KeywordInsightService.Services;
using KeywordInsightService.Utils;

namespace KeywordInsightService.Forms
{
    public partial class FrmGoogleAdsKeywordTool : Form
    {
        private readonly AppSettings _appSettings;
        private readonly KeywordMetricsRepository _keywordMetricsRepository;
        private readonly KeywordCacheService _keywordCacheService;
        private readonly DailyUsageLimiter _dailyUsageLimiter;
        private readonly GoogleAdsKeywordMetricsService _googleAdsKeywordMetricsService;
        private readonly BindingList<KeywordMetricRow> _bindingList;

        public FrmGoogleAdsKeywordTool(AppSettings appSettings, SQLiteHelper sqliteHelper)
        {
            _appSettings = appSettings;
            _keywordMetricsRepository = new KeywordMetricsRepository(sqliteHelper);
            _keywordCacheService = new KeywordCacheService(AppDomain.CurrentDomain.BaseDirectory, _appSettings.Limits.CacheTtlHours);
            _dailyUsageLimiter = new DailyUsageLimiter(
                AppDomain.CurrentDomain.BaseDirectory,
                _appSettings.Limits.MaxDailyApiCalls,
                _appSettings.Limits.MinimumIntervalMilliseconds);
            _googleAdsKeywordMetricsService = new GoogleAdsKeywordMetricsService(_appSettings, _keywordCacheService, _dailyUsageLimiter);
            _bindingList = new BindingList<KeywordMetricRow>();

            InitializeComponent();
            InitializeGrid();
            InitializeDefaults();
            UpdateDailyLimitInfo();
        }

        private void InitializeDefaults()
        {
            txtCustomerId.Text = _appSettings.GoogleAds.DefaultCustomerId;
            txtLanguageId.Text = _appSettings.Defaults.DefaultLanguageId;
            txtGeoTargetIds.Text = _appSettings.Defaults.DefaultGeoTargetIds;
            chkUseCache.Checked = _appSettings.Defaults.DefaultUseCache;
            lblStatus.Text = "待機中";
        }

        private void InitializeGrid()
        {
            DataGridViewHelper.InitializeKeywordMetricsGrid(dgvKeywordMetrics);
            dgvKeywordMetrics.DataSource = _bindingList;
        }

        private async void btnFetchMetrics_Click(object sender, EventArgs e)
        {
            GoogleAdsKeywordRequest? request = null;

            try
            {
                List<string> keywords = GetKeywordsFromInput();
                if (keywords.Count == 0)
                {
                    MessageBox.Show("キーワードを1件以上入力してください。", "入力確認", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (keywords.Count > _appSettings.Limits.MaxKeywordsPerRequest)
                {
                    MessageBox.Show(
                        $"1回で取得できるキーワード数は最大{_appSettings.Limits.MaxKeywordsPerRequest}件です。",
                        "上限超過",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCustomerId.Text))
                {
                    MessageBox.Show("Customer ID を入力してください。", "入力確認", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SetBusyState(true, "Google Ads API から historical metrics を取得しています...");

                request = BuildRequest(keywords);
                IReadOnlyList<KeywordMetricRow> rows = await _googleAdsKeywordMetricsService.GetHistoricalMetricsAsync(
                    request,
                    chkUseCache.Checked);

                _bindingList.Clear();
                foreach (KeywordMetricRow row in rows.OrderBy(x => x.Keyword, StringComparer.OrdinalIgnoreCase))
                {
                    _bindingList.Add(row);
                }

                lblStatus.Text = $"{_bindingList.Count}件の historical metrics を取得しました。";
                UpdateDailyLimitInfo();
            }
            catch (Google.Ads.GoogleAds.V22.Errors.GoogleAdsException googleAdsException)
            {
                string detailMessage = BuildGoogleAdsExceptionMessage(googleAdsException, request);
                MessageBox.Show(detailMessage, "Google Ads 取得エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "取得エラー";
                UpdateDailyLimitInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Google Ads API からの取得に失敗しました。\r\n{ex.Message}", "取得エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "取得エラー";
                UpdateDailyLimitInfo();
            }
            finally
            {
                SetBusyState(false, lblStatus.Text);
            }
        }

        private void btnSaveToSQLite_Click(object sender, EventArgs e)
        {
            try
            {
                List<KeywordMetricRow> rows = _bindingList.ToList();
                if (rows.Count == 0)
                {
                    MessageBox.Show("保存対象のデータがありません。", "保存確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int savedCount = _keywordMetricsRepository.InsertOrIgnore(rows);
                lblStatus.Text = $"{savedCount}件を SQLite に保存しました。";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SQLite への保存に失敗しました。\r\n{ex.Message}", "保存エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "保存エラー";
            }
        }

        private void btnLoadFromSQLite_Click(object sender, EventArgs e)
        {
            try
            {
                IReadOnlyList<KeywordMetricRow> rows = _keywordMetricsRepository.GetAll();
                _bindingList.Clear();
                foreach (KeywordMetricRow row in rows)
                {
                    _bindingList.Add(row);
                }

                lblStatus.Text = $"{_bindingList.Count}件を SQLite から読み込みました。";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SQLite からの読込に失敗しました。\r\n{ex.Message}", "読込エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "読込エラー";
            }
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            try
            {
                List<KeywordMetricRow> rows = _bindingList.ToList();
                if (rows.Count == 0)
                {
                    MessageBox.Show("CSV 出力対象のデータがありません。", "出力確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV ファイル (*.csv)|*.csv";
                saveFileDialog.FileName = $"keyword_metrics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                CsvExportHelper.ExportKeywordMetrics(saveFileDialog.FileName, rows, Encoding.UTF8);
                lblStatus.Text = $"CSV を出力しました: {Path.GetFileName(saveFileDialog.FileName)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CSV 出力に失敗しました。\r\n{ex.Message}", "出力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "CSV 出力エラー";
            }
        }

        private void btnClearInput_Click(object sender, EventArgs e)
        {
            txtKeywords.Clear();
            lblStatus.Text = "キーワード入力をクリアしました。";
        }

        private void btnClearGrid_Click(object sender, EventArgs e)
        {
            _bindingList.Clear();
            lblStatus.Text = "一覧をクリアしました。";
        }

        private GoogleAdsKeywordRequest BuildRequest(IReadOnlyList<string> keywords)
        {
            return new GoogleAdsKeywordRequest
            {
                CustomerId = txtCustomerId.Text.Trim().Replace("-", string.Empty),
                LanguageId = txtLanguageId.Text.Trim(),
                GeoTargetIds = ParseGeoTargetIds(txtGeoTargetIds.Text),
                Keywords = keywords.ToList()
            };
        }

        private List<string> GetKeywordsFromInput()
        {
            return txtKeywords.Lines
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private List<string> ParseGeoTargetIds(string value)
        {
            return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void SetBusyState(bool isBusy, string statusText)
        {
            btnFetchMetrics.Enabled = !isBusy;
            btnSaveToSQLite.Enabled = !isBusy;
            btnLoadFromSQLite.Enabled = !isBusy;
            btnExportCsv.Enabled = !isBusy;
            btnClearInput.Enabled = !isBusy;
            btnClearGrid.Enabled = !isBusy;
            lblStatus.Text = statusText;
            Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
        }

        private void UpdateDailyLimitInfo()
        {
            DailyUsageState state = _dailyUsageLimiter.GetStateForToday();
            lblDailyLimitInfo.Text =
                $"本日のAPI利用回数: {state.TodayCallCount} / {_appSettings.Limits.MaxDailyApiCalls}  最終呼出: {state.LastCalledAtText}";
        }

        private string BuildGoogleAdsExceptionMessage(
            Google.Ads.GoogleAds.V22.Errors.GoogleAdsException googleAdsException,
            GoogleAdsKeywordRequest? request)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Google Ads API からの取得に失敗しました。");
            builder.AppendLine($"StatusCode: {googleAdsException.StatusCode}");
            builder.AppendLine($"Message: {googleAdsException.Message}");
            builder.AppendLine($"Request Customer ID: {request?.CustomerId ?? "-"}");
            builder.AppendLine($"Login Customer ID: {_appSettings.GoogleAds.LoginCustomerId}");
            builder.AppendLine($"Default Customer ID: {_appSettings.GoogleAds.DefaultCustomerId}");
            builder.AppendLine();

            if (googleAdsException.Failure != null && googleAdsException.Failure.Errors.Count > 0)
            {
                builder.AppendLine("Google Ads Error Details:");

                int index = 1;
                foreach (Google.Ads.GoogleAds.V22.Errors.GoogleAdsError error in googleAdsException.Failure.Errors)
                {
                    builder.AppendLine($"[{index}] ErrorCode: {error.ErrorCode}");
                    builder.AppendLine($"[{index}] Message: {error.Message}");

                    if (!string.IsNullOrWhiteSpace(error.Trigger?.StringValue))
                    {
                        builder.AppendLine($"[{index}] Trigger: {error.Trigger.StringValue}");
                    }

                    if (error.Location != null && error.Location.FieldPathElements.Count > 0)
                    {
                        string fieldPath = string.Join(" > ", error.Location.FieldPathElements.Select(x => x.FieldName));
                        builder.AppendLine($"[{index}] FieldPath: {fieldPath}");
                    }

                    builder.AppendLine();
                    index += 1;
                }
            }

            return builder.ToString();
        }
    }
}

