using System.Drawing;
using System.Windows.Forms;

namespace KeywordInsightService.Forms
{
    partial class FrmGoogleAdsKeywordTool
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtKeywords;
        private TextBox txtCustomerId;
        private TextBox txtLanguageId;
        private TextBox txtGeoTargetIds;
        private CheckBox chkUseCache;
        private Button btnFetchMetrics;
        private Button btnSaveToSQLite;
        private Button btnLoadFromSQLite;
        private Button btnExportCsv;
        private Button btnClearInput;
        private Button btnClearGrid;
        private Label lblStatus;
        private Label lblDailyLimitInfo;
        private DataGridView dgvKeywordMetrics;
        private Label lblKeywordsTitle;
        private Label lblCustomerIdTitle;
        private Label lblLanguageIdTitle;
        private Label lblGeoTargetIdsTitle;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            txtKeywords = new TextBox();
            txtCustomerId = new TextBox();
            txtLanguageId = new TextBox();
            txtGeoTargetIds = new TextBox();
            chkUseCache = new CheckBox();
            btnFetchMetrics = new Button();
            btnSaveToSQLite = new Button();
            btnLoadFromSQLite = new Button();
            btnExportCsv = new Button();
            btnClearInput = new Button();
            btnClearGrid = new Button();
            lblStatus = new Label();
            lblDailyLimitInfo = new Label();
            dgvKeywordMetrics = new DataGridView();
            lblKeywordsTitle = new Label();
            lblCustomerIdTitle = new Label();
            lblLanguageIdTitle = new Label();
            lblGeoTargetIdsTitle = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvKeywordMetrics).BeginInit();
            SuspendLayout();
            // 
            // txtKeywords
            // 
            txtKeywords.Location = new Point(12, 32);
            txtKeywords.Multiline = true;
            txtKeywords.Name = "txtKeywords";
            txtKeywords.ScrollBars = ScrollBars.Vertical;
            txtKeywords.Size = new Size(360, 120);
            txtKeywords.TabIndex = 0;
            // 
            // txtCustomerId
            // 
            txtCustomerId.Location = new Point(390, 32);
            txtCustomerId.Name = "txtCustomerId";
            txtCustomerId.Size = new Size(180, 23);
            txtCustomerId.TabIndex = 1;
            // 
            // txtLanguageId
            // 
            txtLanguageId.Location = new Point(390, 82);
            txtLanguageId.Name = "txtLanguageId";
            txtLanguageId.Size = new Size(180, 23);
            txtLanguageId.TabIndex = 2;
            // 
            // txtGeoTargetIds
            // 
            txtGeoTargetIds.Location = new Point(390, 132);
            txtGeoTargetIds.Name = "txtGeoTargetIds";
            txtGeoTargetIds.Size = new Size(280, 23);
            txtGeoTargetIds.TabIndex = 3;
            // 
            // chkUseCache
            // 
            chkUseCache.AutoSize = true;
            chkUseCache.Location = new Point(690, 134);
            chkUseCache.Name = "chkUseCache";
            chkUseCache.Size = new Size(96, 19);
            chkUseCache.TabIndex = 4;
            chkUseCache.Text = "キャッシュ利用";
            chkUseCache.UseVisualStyleBackColor = true;
            // 
            // btnFetchMetrics
            // 
            btnFetchMetrics.Location = new Point(12, 170);
            btnFetchMetrics.Name = "btnFetchMetrics";
            btnFetchMetrics.Size = new Size(150, 32);
            btnFetchMetrics.TabIndex = 5;
            btnFetchMetrics.Text = "取得して表示";
            btnFetchMetrics.UseVisualStyleBackColor = true;
            btnFetchMetrics.Click += btnFetchMetrics_Click;
            // 
            // btnSaveToSQLite
            // 
            btnSaveToSQLite.Location = new Point(168, 170);
            btnSaveToSQLite.Name = "btnSaveToSQLite";
            btnSaveToSQLite.Size = new Size(150, 32);
            btnSaveToSQLite.TabIndex = 6;
            btnSaveToSQLite.Text = "SQLite に保存";
            btnSaveToSQLite.UseVisualStyleBackColor = true;
            btnSaveToSQLite.Click += btnSaveToSQLite_Click;
            // 
            // btnLoadFromSQLite
            // 
            btnLoadFromSQLite.Location = new Point(324, 170);
            btnLoadFromSQLite.Name = "btnLoadFromSQLite";
            btnLoadFromSQLite.Size = new Size(150, 32);
            btnLoadFromSQLite.TabIndex = 7;
            btnLoadFromSQLite.Text = "SQLite から読込";
            btnLoadFromSQLite.UseVisualStyleBackColor = true;
            btnLoadFromSQLite.Click += btnLoadFromSQLite_Click;
            // 
            // btnExportCsv
            // 
            btnExportCsv.Location = new Point(480, 170);
            btnExportCsv.Name = "btnExportCsv";
            btnExportCsv.Size = new Size(120, 32);
            btnExportCsv.TabIndex = 8;
            btnExportCsv.Text = "CSV 出力";
            btnExportCsv.UseVisualStyleBackColor = true;
            btnExportCsv.Click += btnExportCsv_Click;
            // 
            // btnClearInput
            // 
            btnClearInput.Location = new Point(606, 170);
            btnClearInput.Name = "btnClearInput";
            btnClearInput.Size = new Size(120, 32);
            btnClearInput.TabIndex = 9;
            btnClearInput.Text = "入力クリア";
            btnClearInput.UseVisualStyleBackColor = true;
            btnClearInput.Click += btnClearInput_Click;
            // 
            // btnClearGrid
            // 
            btnClearGrid.Location = new Point(732, 170);
            btnClearGrid.Name = "btnClearGrid";
            btnClearGrid.Size = new Size(120, 32);
            btnClearGrid.TabIndex = 10;
            btnClearGrid.Text = "一覧クリア";
            btnClearGrid.UseVisualStyleBackColor = true;
            btnClearGrid.Click += btnClearGrid_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(12, 216);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(55, 15);
            lblStatus.TabIndex = 11;
            lblStatus.Text = "待機中";
            // 
            // lblDailyLimitInfo
            // 
            lblDailyLimitInfo.AutoSize = true;
            lblDailyLimitInfo.Location = new Point(12, 240);
            lblDailyLimitInfo.Name = "lblDailyLimitInfo";
            lblDailyLimitInfo.Size = new Size(145, 15);
            lblDailyLimitInfo.TabIndex = 12;
            lblDailyLimitInfo.Text = "本日のAPI利用回数: 0 / 10";
            // 
            // dgvKeywordMetrics
            // 
            dgvKeywordMetrics.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvKeywordMetrics.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvKeywordMetrics.Location = new Point(12, 268);
            dgvKeywordMetrics.Name = "dgvKeywordMetrics";
            dgvKeywordMetrics.Size = new Size(1140, 380);
            dgvKeywordMetrics.TabIndex = 13;
            // 
            // lblKeywordsTitle
            // 
            lblKeywordsTitle.AutoSize = true;
            lblKeywordsTitle.Location = new Point(12, 14);
            lblKeywordsTitle.Name = "lblKeywordsTitle";
            lblKeywordsTitle.Size = new Size(109, 15);
            lblKeywordsTitle.TabIndex = 14;
            lblKeywordsTitle.Text = "キーワード(1行1件)";
            // 
            // lblCustomerIdTitle
            // 
            lblCustomerIdTitle.AutoSize = true;
            lblCustomerIdTitle.Location = new Point(390, 14);
            lblCustomerIdTitle.Name = "lblCustomerIdTitle";
            lblCustomerIdTitle.Size = new Size(76, 15);
            lblCustomerIdTitle.TabIndex = 15;
            lblCustomerIdTitle.Text = "Customer ID";
            // 
            // lblLanguageIdTitle
            // 
            lblLanguageIdTitle.AutoSize = true;
            lblLanguageIdTitle.Location = new Point(390, 64);
            lblLanguageIdTitle.Name = "lblLanguageIdTitle";
            lblLanguageIdTitle.Size = new Size(73, 15);
            lblLanguageIdTitle.TabIndex = 16;
            lblLanguageIdTitle.Text = "Language ID";
            // 
            // lblGeoTargetIdsTitle
            // 
            lblGeoTargetIdsTitle.AutoSize = true;
            lblGeoTargetIdsTitle.Location = new Point(390, 114);
            lblGeoTargetIdsTitle.Name = "lblGeoTargetIdsTitle";
            lblGeoTargetIdsTitle.Size = new Size(122, 15);
            lblGeoTargetIdsTitle.TabIndex = 17;
            lblGeoTargetIdsTitle.Text = "Geo Target IDs(カンマ)";
            // 
            // FrmGoogleAdsKeywordTool
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1164, 660);
            Controls.Add(lblGeoTargetIdsTitle);
            Controls.Add(lblLanguageIdTitle);
            Controls.Add(lblCustomerIdTitle);
            Controls.Add(lblKeywordsTitle);
            Controls.Add(dgvKeywordMetrics);
            Controls.Add(lblDailyLimitInfo);
            Controls.Add(lblStatus);
            Controls.Add(btnClearGrid);
            Controls.Add(btnClearInput);
            Controls.Add(btnExportCsv);
            Controls.Add(btnLoadFromSQLite);
            Controls.Add(btnSaveToSQLite);
            Controls.Add(btnFetchMetrics);
            Controls.Add(chkUseCache);
            Controls.Add(txtGeoTargetIds);
            Controls.Add(txtLanguageId);
            Controls.Add(txtCustomerId);
            Controls.Add(txtKeywords);
            MinimumSize = new Size(1180, 699);
            Name = "FrmGoogleAdsKeywordTool";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Google Ads Keyword Tool";
            ((System.ComponentModel.ISupportInitialize)dgvKeywordMetrics).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
