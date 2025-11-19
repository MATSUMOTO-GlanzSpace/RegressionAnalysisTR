using System.Data;
using SalesAnalysisSource;

namespace RegressionAnalysis
{
	public partial class RegressionAnalysisForm : Form
	{
		/**
		 * RegressionAnalysisForm クラスの新しいインスタンスを初期化します
		 */
		public RegressionAnalysisForm()
		{
			InitializeComponent();
		}

		/**
		 * セールスCSVファイルを選択するボタンがクリックされたときに発生するイベント ハンドラー
		 * @param sender イベントの送信元
		 * @param e イベント データ
		 */
		private void BtnSelectSalesCSVFile_Click(object sender, EventArgs e)
		{

		}

		/**
		 * 天気CSVファイルを選択するボタンがクリックされたときに発生するイベント ハンドラー
		 * @param sender イベントの送信元
		 * @param e イベント データ
		 */
		private void BtnWeatherCSVFile_Click(object sender, EventArgs e)
		{

		}

		/**
		 * 単位CSVファイルを選択するボタンがクリックされたときに発生するイベント ハンドラー
		 * @param sender イベントの送信元
		 * @param e イベント データ
		 */
		private void BtnUnitsCSVFile_Click(object sender, EventArgs e)
		{

		}


		/**
		 * 分析データを読み込むボタンがクリックされたときに発生するイベント ハンドラー
		 * @param sender イベントの送信元
		 * @param e イベント データ
		 */
		private void BtnLoadAnalysisData_Click(object sender, EventArgs e)
		{
			DataTable? result;
			if (RbnCSVFile.Checked)
			{
				// for test - start
				string projectRoot = @"C:\Users\hi_ma\OneDrive\ドキュメント\Private\Study\統計分析\RegressionAnalysis\SalesAnalysisSource\database\";
				string salesCsvPath = projectRoot + "Sales.csv";
				string weatherCsvPath = projectRoot + "Weather.csv";
				string unitsCsvPath = projectRoot + "Units.csv";
				// for test - end

				var csvMerger = new CsvSalesDataMerger(salesCsvPath, weatherCsvPath, unitsCsvPath);
				csvMerger.AddFilterCondition("", "sales.品種", "='だいこん'");
				result = csvMerger.GetMergedDataTable();
			}
			else if (RbnDataBase.Checked)
			{
				var mysqlMerger = new MySqlSalesDataMerger(ConfigurationHelper.GetConnectionString(), "sales", "weather", "units");
				mysqlMerger.AddFilterCondition("", "sales.variety", "='だいこん'");
				result = mysqlMerger.GetMergedDataTable();
			}
			else
			{
				MessageBox.Show("読込分析データの選択が誤っています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			DgvAnalysisData.DataSource = result;
		}

		/**
		 * 分析を実行するボタンがクリックされたときに発生するイベント ハンドラー
		 * @param sender イベントの送信元
		 * @param e イベント データ
		 */
		private void BtnRunAnalysis_Click(object sender, EventArgs e)
		{

		}
	}
}
