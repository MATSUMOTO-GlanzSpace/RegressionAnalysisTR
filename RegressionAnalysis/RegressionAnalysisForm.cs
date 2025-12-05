using System.Data;
using System.Diagnostics; // ファイル先頭に追加
using SalesAnalysisSource;
using RegressionAnalysis.Common;

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
				DgvAnalysisData.DataSource = sourceForm.LoadedAnalysisData;
				// 目的変数コンボボックス、説明変数チェックボックスリストにフィールド名を表示設定
				CmbResponseVariable.DataSource = null;
				ClbPredictorVariable.DataSource = null;
				if (sourceForm.LoadedAnalysisData != null)
				{
					// 目的変数コンボボックスリストにデータソースを設定
					CmbResponseVariable.DataSource = sourceForm.LoadedAnalysisData.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
					// 説明変数チェックボックスリストにデータソースを設定
					ClbPredictorVariable.DataSource = sourceForm.LoadedAnalysisData.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
					LblRecordCount.Text = $"レコード数: {sourceForm.LoadedAnalysisData.Rows.Count:N0}";
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

			// DataMergerの静的メソッドで有効な行のみ抽出
			var filteredTable = DataMerger.FilterValidRows(allData, responseName, predictorNames);
			if (filteredTable == null || filteredTable.Rows.Count == 0)
			{
				MessageBox.Show("有効なデータが存在しません。選択を修正してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			// 目的変数テーブル（1列のみ）
			var responseTable = filteredTable.DefaultView.ToTable(false, responseName);

			// 説明変数テーブル（複数列）
			var predictorTable = filteredTable.DefaultView.ToTable(false, predictorNames.ToArray());

			// 線形回帰分析実行
			var result = LinearRegressionAnalyzer.Analyze(responseTable, predictorTable);

			// 分析結果の利用（DataGridView等に表示）
			// 定数項と説明変数の係数表示
			DgvAnalysisResult.DataSource = result.VariableStats;
			DgvAnalysisResult.Refresh(); // DataGridViewの内容を明示的に再描画（列情報が変わってる可能性考慮）
			// 重決定経緯数(R²)、補正(R²)表示
			LblRSquared.Text = $"R²= {result.RSquared:F4} (補正R²= {result.AdjustedRSquared:F4})";
		}
	}
}
