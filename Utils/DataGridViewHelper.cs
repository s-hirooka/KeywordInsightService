using System.Windows.Forms;

namespace KeywordInsightService.Utils
{
    public static class DataGridViewHelper
    {
        public static void InitializeKeywordMetricsGrid(DataGridView grid)
        {
            grid.AutoGenerateColumns = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.Columns.Clear();

            grid.Columns.Add(CreateTextColumn("Keyword", "Keyword", "キーワード", 220));
            grid.Columns.Add(CreateTextColumn("AvgMonthlySearches", "AvgMonthlySearches", "月間検索数", 120));
            grid.Columns.Add(CreateTextColumn("CompetitionLevel", "CompetitionLevel", "競合性", 120));
            grid.Columns.Add(CreateTextColumn("CompetitionIndex", "CompetitionIndex", "競合指数", 120));
            grid.Columns.Add(CreateTextColumn("LowTopOfPageBid", "LowTopOfPageBid", "低単価", 120));
            grid.Columns.Add(CreateTextColumn("HighTopOfPageBid", "HighTopOfPageBid", "高単価", 120));
            grid.Columns.Add(CreateTextColumn("RetrievedAt", "RetrievedAt", "取得日時", 180));
        }

        private static DataGridViewTextBoxColumn CreateTextColumn(string name, string dataPropertyName, string headerText, int minimumWidth)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = name,
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                MinimumWidth = minimumWidth,
                SortMode = DataGridViewColumnSortMode.Automatic
            };
        }
    }
}
