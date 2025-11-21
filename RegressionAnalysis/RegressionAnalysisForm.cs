using System.Data;
using System.Diagnostics; // ファイル先頭に追加
using SalesAnalysisSource;
using RegressionAnalysisLibrary;

namespace RegressionAnalysis
{
	/// <summary>
	/// 回帰分析フォームクラス
	/// </summary>
	public partial class RegressionAnalysisForm : Form
	{
		/// <summary>
		/// RegressionAnalysisForm クラスの新しいインスタンスを初期化します
		/// </summary>
		public RegressionAnalysisForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// 分析データを読み込むボタンがクリックされたときに発生するイベント ハンドラー
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
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
					// 目的変数コンボボックスリストにデータソースを設定
					CmbResponseVariable.DataSource = sourceForm.LoadedAnalisysData.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList(); ;
					// 説明変数チェックボックスリストにデータソースを設定
					ClbPredictorVariable.DataSource = sourceForm.LoadedAnalisysData.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList(); ;
					LblRecordCount.Text = $"レコード数: {sourceForm.LoadedAnalisysData.Rows.Count:N0}";
				}
			}
		}

		/// <summary>
		/// 分析実行ボタンがクリックされたときに発生するイベント ハンドラー
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		private void BtnRunAnalysis_Click(object sender, EventArgs e)
		{
			// DataTable取得
			var allData = DgvAnalysisData.DataSource as DataTable;
			if (allData == null) return;

			// 説明変数名リスト（チェックされた項目のみ）
			var predictorNames = ClbPredictorVariable.CheckedItems.Cast<string>().ToList();
			// 目的変数名
			var responseName = CmbResponseVariable.Text;

			// 目的変数名が説明変数名リストに含まれている場合はエラー
			if (predictorNames.Contains(responseName))
			{
				MessageBox.Show("目的変数が説明変数に含まれています。選択を修正してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// フィルタリング: 有効な行
			var validRows = allData.AsEnumerable()
				.Where(row =>
					// 目的変数に値があるか
					!row.IsNull(responseName) &&
					!string.IsNullOrWhiteSpace(row[responseName]?.ToString()) &&
					// 説明変数すべてに値があるか
					predictorNames.All(name =>
						!row.IsNull(name) &&
						!string.IsNullOrWhiteSpace(row[name]?.ToString())
					)
				).ToList();

			// validRowsから新しいDataTableを作成
			var filteredTable = validRows.Count > 0 ? validRows.CopyToDataTable() : allData.Clone();

			// 目的変数テーブル（1列のみ）
			var responseTable = filteredTable.DefaultView.ToTable(false, responseName);

			// 説明変数テーブル（複数列）
			var predictorTable = filteredTable.DefaultView.ToTable(false, predictorNames.ToArray());

			// 重回帰分析実行
			var analyzer = new LinearRegressionAnalyzer();
			var result = LinearRegressionAnalyzer.Analyze(responseTable,predictorTable);

			// 結果の利用例（DataGridView等に表示）
			DgvAnalysisResult.DataSource = result.VariableStats;
			LblRSquared.Text = $"重決定係数＝ R²: {result.RSquared:F4} (補正R²＝ {result.AdjustedRSquared:F4})";
		}
	}
}
