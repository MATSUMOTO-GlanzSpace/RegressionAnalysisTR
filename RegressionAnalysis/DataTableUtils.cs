using System.Data;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace RegressionAnalysis
{
	public static class DataTableUtils
	{
		public static DataTable ReadCsv(string path)
		{
			// TODD: エラーハンドリング追加
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

		public static string[] GetFieldNames(DataTable table)
		{
			return table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray();
		}
	}
}