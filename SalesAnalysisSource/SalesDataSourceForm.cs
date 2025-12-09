using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;
using static RegressionAnalysis.Common.DataTableHelpers;
using static RegressionAnalysis.Common.ConfigurationHelper;
using SalesAnalysisSource; // 追加

// TODO: エラーハンドリングを強化することを検討してください
// TODO: テーブル名のバリデーションを追加することを検討してください
// TODO: SQLインジェクション対策が必要かも？
// 注意: テーブル名を直接SQLに埋め込むのはセキュリティリスクがあるため、信頼できる入力のみを使用してください。
// ここでは簡略化のために直接埋め込んでいますが、実際のアプリケーションでは注意が必要です。
// 注意: プレースホルダーはテーブル名には使用できません。


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

			// コンボボックスの選択リストをバインド（初期状態は件数:0）
			CmbTxtFilterVerietyDB.DataSource = new DataTable();
			// コンボボックスの選択リストをバインド（初期状態は件数:0）
			CmbTxtFilterVerietyCSV.DataSource = new DataTable();
		}

		// プロパティ定義
		/// <summary>
		/// CSVソースが選択されているかどうか
		/// </summary>
		public bool IsSelectCSVSource => TabControlSourceType.SelectedTab == tabPageCSV;

		/// <summary>
		/// DBソースが選択されているかどうか
		/// </summary>
		public bool IsSelectDBSource => TabControlSourceType.SelectedTab == tabPageDataBase;

		/// <summary>
		/// 読み込まれた分析データ
		/// </summary>
		public DataTable? LoadedAnalysisData { get; private set; }

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
			// 処理中カーソルに変更
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				// DBデータソース(MySQL)からの取得
				var mysqlMerger = new MySqlSalesDataMerger(
					TxtSalesTableName.Text,
					TxtWeatherTableName.Text,
					TxtUnitsTableName.Text
					);
				// 品種によるフィルター条件設定
				if (!string.IsNullOrEmpty(CmbTxtFilterVerietyDB.Text))
					mysqlMerger.AddFilterCondition("", $"{TxtSalesTableName.Text}.variety", $"='{CmbTxtFilterVerietyDB.Text}'");
				// 取得実行
				LoadedAnalysisData = mysqlMerger.GetMergedDataTable();
			}
			finally
			{
				// 元のカーソルに戻す
				Cursor.Current = Cursors.Default;
			}
		}
		/// <summary>
		/// (CSV)分析データを読み込むボタンがクリックされたときに発生するイベント ハンドラー
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		private void BtnLoadCSVAnalysisData_Click(object sender, EventArgs e)
		{
			// 処理中カーソルに変更
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				// CSVデータソースからの取得
				var csvMerger = new CsvSalesDataMerger(
					TxtSalseCSVFileName.Text,
					TxtWeatherCSVFileName.Text,
					TxtUnitsCSVFileName.Text
					);
				// 品種によるフィルター条件設定
				if (!string.IsNullOrEmpty(CmbTxtFilterVerietyCSV.Text))
					csvMerger.AddFilterCondition("", "sales.品種", $"='{CmbTxtFilterVerietyCSV.Text}'");
				// 取得実行
				LoadedAnalysisData = csvMerger.GetMergedDataTable();
			}
			finally
			{
				// 元のカーソルに戻す
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// 品種フィルターリスト表示直前の初回処理にてsalesVerietyDBを設定する
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		private void CmbTxtFilterVerietyDB_DropDown(object sender, EventArgs e)
		{
			// 処理中カーソルに変更
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				// 販売テーブル名が未設定の場合は空のリストを設定して終了
				if (string.IsNullOrEmpty(TxtSalesTableName.Text))
				{
					CmbTxtFilterVerietyDB.DataSource = new DataTable();
					CmbTxtFilterVerietyDB.DisplayMember = null;
					CmbTxtFilterVerietyDB.ValueMember = null;
					return;
				}
				// 既にデータソースが設定されている場合は処理しない
				var dataTable = CmbTxtFilterVerietyDB.DataSource as DataTable;
				if (dataTable == null || dataTable.Rows.Count == 0)
				{
					// データベースから品種リストを取得してコンボボックスに設定
					var dt = new DataTable();
					try
					{
						// MySQLデータベースから品種リストを重複なく取得
						using (var conn = MySqlConnectionFactory.CreateOpenConnection())
						using (var cmd = new MySqlCommand(
							$"SELECT DISTINCT `variety` FROM `{TxtSalesTableName.Text}` WHERE `variety` IS NOT NULL AND `variety` <> ''",
							conn))
						using (var adapter = new MySqlDataAdapter(cmd))
						{
							adapter.Fill(dt);
						}
						// コンボボックスに品種をソートして設定
						CmbTxtFilterVerietyDB.DataSource = new DataView(dt) { Sort = "variety ASC" };
						CmbTxtFilterVerietyDB.DisplayMember = "variety";
						CmbTxtFilterVerietyDB.ValueMember = "variety";
					}
					catch (Exception ex)
					{
						// エラー表示
						MessageBox.Show(this, $"データベースからの品種リストの取得に失敗しました。\n{ex.Message}",
							"エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}
				}
			}
			finally
			{
				// 元のカーソルに戻す
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// 品種フィルターリスト表示直前の初回処理にてsalesVerietyCSVを設定する
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		private void CmbTxtFilterVerietyCSV_DropDown(object sender, EventArgs e)
		{
			// 処理中カーソルに変更
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				if (string.IsNullOrEmpty(TxtSalseCSVFileName.Text))
				{
					// 販売CSVファイル名が未設定の場合は空のリストを設定して終了
					CmbTxtFilterVerietyCSV.DataSource = new DataTable();
					CmbTxtFilterVerietyCSV.DisplayMember = null;
					CmbTxtFilterVerietyCSV.ValueMember = null;
					return;
				}
				// 既にデータソースが設定されている場合は処理しない
				var dataTable = CmbTxtFilterVerietyCSV.DataSource as DataTable;
				if (dataTable == null || dataTable.Rows.Count == 0)
				{
					// 読込んだテーブルから品種リストを取得してコンボボックスに設定
					DataTable table;
					try
					{
						// CSVファイルの読み込み
						table = ReadCsv(TxtSalseCSVFileName.Text);
						// 品種リストから重複なく品種を取得
						var dt = GetDistinctFieldTable(table, "品種");
						// 品種をコンボボックスにソートして設定
						CmbTxtFilterVerietyCSV.DataSource = new DataView(dt) { Sort = "品種 ASC" };
						CmbTxtFilterVerietyCSV.DisplayMember = "品種";
						CmbTxtFilterVerietyCSV.ValueMember = "品種";
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, $"販売CSVファイルの読み込みに失敗しました。\n{ex.Message}",
							"エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}
				}
			}
			finally
			{
				// 元のカーソルに戻す
				Cursor.Current = Cursors.Default;
			}
		}
	}
}
