using RegressionAnalysis.Common;

namespace SalesAnalysisSource
{
	/// <summary>
	/// 販売分析データソース選択フォームクラス
	/// </summary>
	public partial class SalesDataSourceForm : Form
	{
		// メンバー変数定義
		/// <summary>
		/// ファイル選択ダイアログ
		/// 使い回すことで、複数回開いたときに前回のディレクトリを覚えておける
		/// </summary>
		private readonly OpenFileDialog selectFileDialog = new();

		// コンストラクタ
		/// <summary>
		/// SalesDataSourceForm クラスの新しいインスタンスを初期化します
		/// </summary>
		public SalesDataSourceForm()
		{
			InitializeComponent();

			// ファイル選択ダイアログのフィルター設定
			selectFileDialog.Filter = "CSVファイル (*.csv)|*.csv|すべてのファイル (*.*)|*.*";

			// NOTE: 品種フィルター用コンボボックスは廃止。マージャーオブジェクトを生成して呼び出し元で処理を行う。
		}

		/// <summary>
		/// DataMerger（呼出し側でGetMergedDataTable実行するために保持する）
		/// </summary>
		public DataMerger? DataMerger { get; private set; }

		// イベントハンドラー定義
		/// <summary>
		/// 販売CSVファイルを選択するボタンがクリックされたときに発生するイベント ハンドラー
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		private void BtnSelectSalesCSVFile_Click(object sender, EventArgs e)
		{
			selectFileDialog.Title = "販売CSVファイルを選択してください";
			selectFileDialog.FileName = TxtSalseCSVFileName.Text;
			if (selectFileDialog.ShowDialog(this) == DialogResult.OK)
				TxtSalseCSVFileName.Text = selectFileDialog.FileName;
		}
		/// <summary>
		/// 気象CSVファイルを選択するボタンがクリックされたときに発生するイベント ハンドラー
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		private void BtnSelectWeatherCSVFile_Click(object sender, EventArgs e)
		{
			selectFileDialog.Title = "気象CSVファイルを選択してください";
			selectFileDialog.FileName = TxtWeatherCSVFileName.Text;
			if (selectFileDialog.ShowDialog(this) == DialogResult.OK)
				TxtWeatherCSVFileName.Text = selectFileDialog.FileName;
		}
		/// <summary>
		/// 単位CSVファイルを選択するボタンがクリックされたときに発生するイベント ハンドラー
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		private void BtnSelectUnitsCSVFile_Click(object sender, EventArgs e)
		{
			selectFileDialog.Title = "単位CSVファイルを選択してください";
			selectFileDialog.FileName = TxtUnitsCSVFileName.Text;
			if (selectFileDialog.ShowDialog(this) == DialogResult.OK)
				TxtUnitsCSVFileName.Text = selectFileDialog.FileName;
		}
		/// <summary>
		/// (DB)分析データを読み込むボタンがクリックされるときに発生するイベント ハンドラー
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		private void BtnLoadDBAnalysisData_Click(object sender, EventArgs e)
		{
			// DBデータソース(MySQL)マージャー生成（実際のデータ取得は呼出し側で遅延実行する）
			var mysqlMerger = new MySqlSalesDataMerger(
				TxtSalesTableName.Text,
				TxtWeatherTableName.Text,
				TxtUnitsTableName.Text
				);
			// DataMerger を保持
			DataMerger = mysqlMerger;
		}
		/// <summary>
		/// (CSV)分析データを読み込むボタンがクリックされたときに発生するイベント ハンドラー
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		private void BtnLoadCSVAnalysisData_Click(object sender, EventArgs e)
		{
			// CSVデータソースマージャー生成（実際のデータ取得は呼出し側で遅延実行する）
			var csvMerger = new CsvSalesDataMerger(
				TxtSalseCSVFileName.Text,
				TxtWeatherCSVFileName.Text,
				TxtUnitsCSVFileName.Text
				);
			// DataMerger を保持
			DataMerger = csvMerger;
		}
	}
}
