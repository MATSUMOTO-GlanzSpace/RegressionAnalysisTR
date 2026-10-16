using System.Data;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace RegressionAnalysis.Common
{
	/// <summary>
	/// CSVユーティリティクラス
	/// </summary>
	public static class CsvHelpers
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
				if (!parser.EndOfData)
				{
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
		/// CSVエスケープ処理
		/// </summary>
		/// <param name="s">エスケープ対象の文字列</param>
		/// <returns>エスケープ後の文字列</returns>
		/// <remarks>
		/// 文字列にカンマ、改行、ダブルクォーテーションが含まれる場合、
		/// ダブルクォーテーションで囲み、ダブルクォーテーション自体は2つに置換します。
		/// </remarks>
		public static string EscapeCsv(string s)
		{
			// カンマ、改行、ダブルクォーテーションが含まれる場合はエスケープ処理
			if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
				return $"\"{s.Replace("\"", "\"\"")}\"";
			return s;
		}
	}
}
