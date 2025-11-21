using System.Data;
using System.Text;
using Microsoft.VisualBasic.FileIO;

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

	}
}