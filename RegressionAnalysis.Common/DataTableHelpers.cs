using System.Data;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using MySqlConnector;

namespace RegressionAnalysis.Common
{
	/// <summary>
	/// DataTableユーティリティクラス
	/// </summary>
	public static class DataTableHelpers
	{
		/// <summary>
        /// CSVファイル読み込みメソッド
        /// </summary>
        /// <param name="path">CSVファイルパス</param>
        /// <returns>読み込んだDataTable</returns>
		public static DataTable ReadCsv(string path)
		{
			DataTable table = new();
			using (var parser = new TextFieldParser(path, Encoding.UTF8))
			{
				// CSV設定
				parser.TextFieldType = FieldType.Delimited;
				parser.SetDelimiters(",");
				parser.HasFieldsEnclosedInQuotes = true;

				// ヘッダー行
				if (!parser.EndOfData) {
					string[] headers = parser.ReadFields() ?? [];
					foreach (var h in headers) table.Columns.Add(h);
				}

				// データ行
				while (!parser.EndOfData)
				{
					string[]? fields = parser.ReadFields();
					if (fields != null) table.Rows.Add(fields);
				}
			}
			return table;
		}

		/// <summary>
		/// DataTableからフィールド名取得メソッド
		/// </summary>
		/// <param name="table">DataTableオブジェクト</param>
		/// <returns>フィールド名配列</returns>
		public static string[] GetFieldNames(DataTable table)
		{
			return table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray();
		}

		/// <summary>
		/// 指定フィールドの重複なしDataTable取得メソッド
		/// </summary>
		/// <param name="source">元DataTableオブジェクト</param>
		/// <param name="fieldName">フィールド名</param>
		/// <returns>指定フィールドの重複なしDataTableオブジェクト</returns>
		public static DataTable GetDistinctFieldTable(DataTable source, string fieldName)
		{
			// 指定フィールドの重複なし値リスト取得
			var distinctValues = source.AsEnumerable()
				.Select(row => row.Field<string>(fieldName))
				.Where(val => !string.IsNullOrEmpty(val))
				.Distinct()
				.ToList();

			// 重複なしDataTable作成
			var dt = new DataTable();
			dt.Columns.Add(fieldName);
			foreach (var val in distinctValues)
			{
				dt.Rows.Add(val);
			}
			return dt;
		}

		/// <summary>
		/// DataTableを二次元配列に変換するメソッド
		/// </summary>
		/// <param name="table">DataTableオブジェクト</param>
		/// <returns>二次元配列</returns>
		public static double[][] ToJaggedArray(DataTable table)
		{
			// 二次元配列に変換
			var arr = new double[table.Rows.Count][];
			for (int i = 0; i < table.Rows.Count; i++)
			{
				arr[i] = new double[table.Columns.Count];
				for (int j = 0; j < table.Columns.Count; j++)
					// 型変換して格納
					arr[i][j] = Convert.ToDouble(table.Rows[i][j]);
			}
			return arr;
		}

		/// <summary>
		/// MySQLデータベースから接続データベースの全テーブル名を取得するメソッド
		/// </summary>
		/// <param name="conn">MySqlConnectionオブジェクト</param>
		/// <returns>テーブル名リスト</returns>
		public static List<string> GetAllTableNames(MySqlConnection conn)
		{
			// テーブル名リスト
			var tableNames = new List<string>();
			// SQLクエリで情報スキーマからテーブル名を取得
			using var cmd = conn.CreateCommand();
			cmd.CommandText = @"
				SELECT table_name
				FROM information_schema.tables
				WHERE table_schema = @db
				  AND table_type = 'BASE TABLE'
				ORDER BY table_name;";
			cmd.Parameters.AddWithValue("@db", conn.Database);

			// クエリ実行
			using var reader = cmd.ExecuteReader();
			// 結果をリストに追加
			while (reader.Read())
			{
				tableNames.Add(reader.GetString(0));
			}
			// テーブル名リストを返す
			return tableNames;
		}

	}
}