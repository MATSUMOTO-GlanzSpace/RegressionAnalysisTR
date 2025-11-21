using System.Data;
using SalesAnalysisSource;

namespace RegressionAnalysis
{
	/**
	 * 回帰分析フォームクラス
	 */
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
		 * 分析データを読み込むボタンがクリックされたときに発生するイベント ハンドラー
		 * @param sender イベントの送信元
		 * @param e イベント データ
		 */
		private void BtnLoadAnalysisData_Click(object sender, EventArgs e)
		{
			// 既存のデータソースをクリア
			DgvAnalysisData.DataSource = null;

			// 分析元データソース取得
			var sourceForm = new SalesDataSourceForm();
			sourceForm.ShowDialog(this);
			if (sourceForm.DialogResult == DialogResult.OK)
			{
				// 取得結果を表示
				DgvAnalysisData.DataSource = sourceForm.LoadedAnalisysData;
				// 目的変数コンボボックス、説明変数チェックボックスリストにフィールド名を表示設定
				CmbResponseVariable.DataSource = null;
				ClbPredictorVariable.DataSource = null;
				if (sourceForm.LoadedAnalisysData != null)
				{
					// データテーブルのカラム名リストを取得
					var dataTable = sourceForm.LoadedAnalisysData.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
					// 目的変数コンボボックスリストにデータソースを設定
					CmbResponseVariable.DataSource = dataTable;
					// 説明変数チェックボックスリストにデータソースを設定
					ClbPredictorVariable.DataSource = dataTable;
				}
			}
		}
	}
}
