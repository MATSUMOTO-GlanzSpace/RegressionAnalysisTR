using System.Data;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace SalesAnalysisSource
{
	/**
	 * DataTableユーティリティクラス
	 */
	public static class DataTableUtils
	{
		/**
         * CSVファイル読み込みメソッド
         * @param path CSVファイルパス
         * @return 読み込んだDataTable
         */
		public static DataTable ReadCsv(string path)
		{
			DataTable table = new();
			using (var parser = new TextFieldParser(path, Encoding.UTF8))
			{
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

		/**
		 * DataTableからフィールド名取得メソッド
		 * @param table DataTableオブジェクト
		 * @return フィールド名配列
		 */
		public static string[] GetFieldNames(DataTable table)
		{
			return table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray();
		}

		/**
		 * 指定フィールドの重複なしDataTable取得メソッド
		 * @param source 元DataTableオブジェクト
		 * @param fieldName フィールド名
		 * @return 指定フィールドの重複なしDataTableオブジェクト
		 */
		public static DataTable GetDistinctFieldTable(DataTable source, string fieldName)
		{
			var distinctValues = source.AsEnumerable()
				.Select(row => row.Field<string>(fieldName))
				.Where(val => !string.IsNullOrEmpty(val))
				.Distinct()
				.ToList();

			var dt = new DataTable();
			dt.Columns.Add(fieldName);
			foreach (var val in distinctValues)
			{
				dt.Rows.Add(val);
			}
			return dt;
		}
	}
}