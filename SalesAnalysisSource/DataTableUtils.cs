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
					string[] headers = parser.ReadFields() ?? Array.Empty<string>();
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
	}
}