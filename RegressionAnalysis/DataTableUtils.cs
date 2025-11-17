using System.Data;
using System.Text;

namespace RegressionAnalysis
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
			// CSVファイルをDataTableに読み込み
			DataTable table = new();
			using (var reader = new StreamReader(path, Encoding.UTF8))
			{
				// ヘッダー行読み込み
				string? headerLine = reader.ReadLine();
				if (headerLine == null) return table;
				var headers = headerLine.Split(',');
				foreach (var h in headers) table.Columns.Add(h);
				// データ行読み込み
				string? line;
				while ((line = reader.ReadLine()) != null)
				{
					var fields = line.Split(',');
					table.Rows.Add(fields);
				}
			}
			// 読み込んだDataTableを返す
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