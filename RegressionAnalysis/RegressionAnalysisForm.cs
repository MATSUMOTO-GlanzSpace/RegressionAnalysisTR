using RegressionAnalysis.Common;
using SalesAnalysisSource;
using System.Data;
using System.Diagnostics; // ファイル先頭に追加
using System.Text;

namespace RegressionAnalysis
{
	/// <summary>
	/// 回帰分析フォームクラス
	/// </summary>
	public partial class RegressionAnalysisForm : Form
	{
		// マージャー保持（SalesDataSourceForm から移譲して保持する）
		public DataMerger? DataMerger { get; private set; }
		// 以前はソース種別やテーブル名を保持していたが、現在は DataMerger 側で管理する
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
				// SalesDataSourceForm から DataMerger を移譲して保持
				this.DataMerger = sourceForm.DataMerger;
				// ソース種別キーとテーブル名を保持（UI 上の列名表示に利用）
				var dm = this.DataMerger;
				if (dm != null)
				{
					var cols = dm.GetMergedDataTableColumnNames().ToList();
					CmbResponseVariable.DataSource = cols;
					ClbPredictorVariable.DataSource = cols;
					CmbFilterColumn.DataSource = cols;
					LblRecordCount.Text = "レコード数: (未生成)";
				}
			// DataMerger が列名を管理するためソース種別・テーブル名の保持は不要
				// グリッドはまだ未生成のためクリア
				DgvAnalysisData.DataSource = null;
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
			if (DgvAnalysisData.DataSource is not DataTable allData) return;

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

		/// <summary>
		/// DataGridViewの分析結果（DataTable）をCSVファイルに出力する
		/// </summary>
		/// <param name="dt">出力する分析結果のDataTable</param>
		/// <param name="exportCSVFilePath">出力先のCSVファイルパス</param>
		/// <returns>なし</returns>
		/// <remarks>
		/// DataTableの内容をCSV形式で指定されたファイルに保存します。
		/// ヘッダー行も含めて出力します。
		/// エスケープ処理も行います。
		/// </remarks>
		private static void ExportAnalysisResultToCsv(DataTable dt, string exportCSVFilePath)
		{
			// CSV出力処理
			try
			{
				// UTF-8 BOMなしで書き込み
				using var writer = new StreamWriter(exportCSVFilePath, false, Encoding.UTF8);
				// 列情報取得
				var columns = dt.Columns.Cast<DataColumn>().ToList();
				// ヘッダー出力
				writer.WriteLine(string.Join(",", columns.Select(c => EscapeCsv(c.ColumnName))));
				// データ出力
				foreach (DataRow row in dt.Rows)
				{
					// 各フィールドをCSVエスケープして結合
					var fields = columns.Select(c => EscapeCsv(row[c]?.ToString() ?? ""));
					writer.WriteLine(string.Join(",", fields));
				}
				// 完了メッセージ
				MessageBox.Show("CSVファイルに出力しました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				// エラーメッセージ
				MessageBox.Show($"CSV出力中にエラーが発生しました。\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			/// <summary>
			/// CSVエスケープ処理
			/// </summary>
			/// <param name="s">エスケープ対象の文字列</param>
			/// <returns>エスケープ後の文字列</returns>
			/// <remarks>
			/// 文字列にカンマ、改行、ダブルクォーテーションが含まれる場合、
			/// ダブルクォーテーションで囲み、ダブルクォーテーション自体は2つに置換します。
			/// </remarks>
			static string EscapeCsv(string s)
			{
				// カンマ、改行、ダブルクォーテーションが含まれる場合はエスケープ処理
				if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
					return $"\"{s.Replace("\"", "\"\"")}\"";
				return s;
			}
		}

		/// <summary>
		/// 分析結果をCSV出力するボタンがクリックされたときに発生するイベント ハンドラー
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		/// <remarks>
		/// DataGridViewのDataSourceがDataTableであり、かつ行数が0より大きい場合
		/// にCSV出力ダイアログを表示し、選択されたファイルパスにCSV出力を行います。
		/// エラー時にはメッセージボックスで通知します。
		/// </remarks>
		/// <returns>なし</returns>
		/// 
		private void BtnExportResultCSV_Click(object sender, EventArgs e)
		{
			// DataGridViewのDataSourceがDataTableであり、行数が0より大きいか確認
			if (DgvAnalysisResult.DataSource is not DataTable dt || dt.Rows.Count == 0)
			{
				MessageBox.Show("出力する分析結果がありません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			// 保存先ファイルダイアログ表示
			using var sfd = new SaveFileDialog
			{
				Title = "分析結果のCSV出力先を選択してください",
				Filter = "CSVファイル (*.csv)|*.csv|すべてのファイル (*.*)|*.*",
				FileName = "AnalysisResult.csv"
			};
			// ダイアログでOKが選択された場合、CSV出力処理を実行
			if (sfd.ShowDialog(this) == DialogResult.OK)
			{
				ExportAnalysisResultToCsv(dt, sfd.FileName);
			}
		}

		private void BtnSetAnalisysData_Click(object sender, EventArgs e)
		{
			// DataMerger が存在することを確認
			if (DataMerger == null)
			{
				MessageBox.Show("データソースが設定されていません。データを読み込んでください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// 前回のフィルタをクリア
			DataMerger.Filters.Clear();

			// フィルタ条件取得
			var column = CmbFilterColumn.Text?.Trim();
			var filter = TxtFilterString.Text?.Trim();
			if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(filter))
			{
				// 部分一致（LIKE '%value%') を常に適用する
				var esc = filter.Replace("'", "''");
				var condition = $"LIKE '%{esc}%'";
				// マージ後列名（UIのcolumn）を渡す
				DataMerger.AddFilterCondition("", column, condition);
			}

			// 実データ生成（マージ）
			var merged = DataMerger.GetMergedDataTable();
			DgvAnalysisData.DataSource = merged;

			// コンボボックス/チェックリストを更新
			CmbResponseVariable.DataSource = merged.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
			ClbPredictorVariable.DataSource = merged.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
			LblRecordCount.Text = $"レコード数: {merged.Rows.Count:N0}";
		}
	}
}
