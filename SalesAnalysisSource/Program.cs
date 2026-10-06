using SalesAnalysisSource;
using RegressionAnalysis.Common;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
		// 接続文字列を一度だけMySqlConnectionFactoryにセット
		var connStr = ConfigurationHelper.GetConnectionString();
        MySqlConnectionFactory.SetConnectionString(connStr);
		// メインフォームを起動
		Application.Run(new SalesDataSourceForm());
    }    
}