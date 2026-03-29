using System;
using System.Windows.Forms;
using KeywordInsightService.Data;
using KeywordInsightService.Forms;
using KeywordInsightService.Models;

namespace KeywordInsightService
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            AppSettings settings = AppSettings.LoadOrCreate(AppDomain.CurrentDomain.BaseDirectory);
            SQLiteHelper sqliteHelper = new SQLiteHelper(AppDomain.CurrentDomain.BaseDirectory, settings.Database.DatabaseFileName);
            sqliteHelper.InitializeDatabase();

            Application.Run(new FrmGoogleAdsKeywordTool(settings, sqliteHelper));
        }
    }
}
