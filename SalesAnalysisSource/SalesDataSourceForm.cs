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
using SalesAnalysisSource;

// TODO: エラーハンドリングを強化することを検討してください
// TODO: テーブル名のバリデーションを追加することを検討してください
// TODO: テーブル名、カラム名のエスケープ処理を追加する、囲み文字のＤＢＭＳ方言の吸収することを検討してください
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
			CmbTxtFilterVarietyDB.DataSource = new DataTable();
			// コンボボックスの選択リストをバインド（初期状態は件数:0）
			CmbTxtFilterVarietyCSV.DataSource = new DataTable();
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
				if (!string.IsNullOrEmpty(CmbTxtFilterVarietyDB.Text))
					mysqlMerger.AddFilterCondition("", $"{TxtSalesTableName.Text}.variety", $"='{CmbTxtFilterVarietyDB.Text}'");
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
				if (!string.IsNullOrEmpty(CmbTxtFilterVarietyCSV.Text))
					csvMerger.AddFilterCondition("", "sales.品種", $"='{CmbTxtFilterVarietyCSV.Text}'");
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
		/// 品種フィルターリスト表示直前の初回処理にてsalesVarietyDBを設定する
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		private void CmbTxtFilterVarietyDB_DropDown(object sender, EventArgs e)
		{
			// 処理中カーソルに変更
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				// 販売テーブル名が未設定の場合は空のリストを設定して終了
				if (string.IsNullOrEmpty(TxtSalesTableName.Text))
				{
					CmbTxtFilterVarietyDB.DataSource = new DataTable();
					CmbTxtFilterVarietyDB.DisplayMember = null;
					CmbTxtFilterVarietyDB.ValueMember = null;
					return;
				}
				// 既にデータソースが設定されている場合は処理しない
				if (CmbTxtFilterVarietyDB.DataSource is not DataTable dataTable || dataTable.Rows.Count == 0)
				{
					// データベースから品種リストを取得してコンボボックスに設定
					var dt = new DataTable();
					try
					{
						using (var conn = MySqlConnectionFactory.CreateOpenConnection())
						{
							// 1. テーブル名ホワイトリスト取得
							var tableWhitelist = DbSchemaHelper.GetTableWhitelist_MySql(conn);

							// 2. 入力テーブル名の検証
							string tableName = TxtSalesTableName.Text;
							if (!tableWhitelist.Contains(tableName))
							{
								MessageBox.Show(this, "不正なテーブル名です。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
								return;
							}

							// 3. SQL生成（テーブル名のみ埋め込み、値はパラメータ化）
							string sql = $"SELECT DISTINCT `variety` FROM `{tableName}` WHERE `variety` IS NOT NULL AND `variety` <> @empty";

							// 4. SQL実行
							using var cmd = new MySqlCommand(sql, conn);
							cmd.Parameters.AddWithValue("@empty", "");
							// データ取得
							using var adapter = new MySqlDataAdapter(cmd);
							adapter.Fill(dt);
						}
						// コンボボックスに品種をソートして設定
						CmbTxtFilterVarietyDB.DataSource = new DataView(dt) { Sort = "variety ASC" };
						CmbTxtFilterVarietyDB.DisplayMember = "variety";
						CmbTxtFilterVarietyDB.ValueMember = "variety";
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
		/// 品種フィルターリスト表示直前の初回処理にてsalesVarietyCSVを設定する
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		private void CmbTxtFilterVarietyCSV_DropDown(object sender, EventArgs e)
		{
			// 処理中カーソルに変更
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				if (string.IsNullOrEmpty(TxtSalseCSVFileName.Text))
				{
					// 販売CSVファイル名が未設定の場合は空のリストを設定して終了
					CmbTxtFilterVarietyCSV.DataSource = new DataTable();
					CmbTxtFilterVarietyCSV.DisplayMember = null;
					CmbTxtFilterVarietyCSV.ValueMember = null;
					return;
				}
				// 既にデータソースが設定されている場合は処理しない
				if (CmbTxtFilterVarietyCSV.DataSource is not DataTable dataTable || dataTable.Rows.Count == 0)
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
						CmbTxtFilterVarietyCSV.DataSource = new DataView(dt) { Sort = "品種 ASC" };
						CmbTxtFilterVarietyCSV.DisplayMember = "品種";
						CmbTxtFilterVarietyCSV.ValueMember = "品種";
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

		/// <summary>
		/// 指定した接続からテーブル名のホワイトリストを取得（MySQL用）
		/// </summary>
		/// <param name="conn">MySQL接続オブジェクト</param>
		/// <returns>テーブル名のホワイトリスト</returns>
		/// <remarks>
		/// この関数はテスト的に実装されたものであり、
		/// 実際は、SalesAnalysisSource.DbSchemaHelper.GetTableWhitelist_MySqlを使用することを推奨します。
		/// </remarks>
		private static HashSet<string> GetTableWhitelist(MySqlConnection conn)
		{
			var whitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			using var cmd = new MySqlCommand("SHOW TABLES FROM salesdb", conn);
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				whitelist.Add(reader.GetString(0));
			}
			return whitelist;
		}
	}
}
