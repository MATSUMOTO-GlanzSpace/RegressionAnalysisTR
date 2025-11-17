using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace RegressionAnalysis
{
	/**
     * データ結合基底クラス
     */
	public abstract class DataMerger
    {
		/**
         * 結合データテーブル取得メソッド
         * @return 結合データテーブル
         */
		public DataTable GetMergedDataTable()
        {
            try
            {
				// 派生クラスの実装を呼び出し
				return GetMergedDataTableCore();
            }
			catch (Exception ex)
            {
				// エラーログ出力
				LogError(ex);
				// 空のDataTableを返す
				return new DataTable();
            }
        }

		/**
         * 派生クラスで実装する結合ロジック
         * @return 結合データテーブル
         */
		protected abstract DataTable GetMergedDataTableCore();

		/**
         * エラーログ出力メソッド
         * @param ex 例外オブジェクト
         */
		protected virtual void LogError(Exception ex)
        {
            // 標準出力にエラーログ
            Console.WriteLine($"[DataMerger Error] {ex.Message}");
        }
    }

	/**
     * 1. CSVファイル結合クラス
     * @param salesCsvPath 売上CSVファイルパス
     * @param weatherCsvPath 天気CSVファイルパス
     * @param unitsCsvPath 単位CSVファイルパス
     */
	public class CsvDataMerger(string salesCsvPath, string weatherCsvPath, string unitsCsvPath) : DataMerger
    {

		/**
         * CSVファイル結合ロジック実装
         * @return 結合データテーブル
         */
		protected override DataTable GetMergedDataTableCore()
        {
            // CSV読み込み
            var salesTable = ReadCsv(salesCsvPath);
            var weatherTable = ReadCsv(weatherCsvPath);
            var unitsTable = ReadCsv(unitsCsvPath);

            // LINQで結合
            var query = from sales in salesTable.AsEnumerable()
                        join weather in weatherTable.AsEnumerable()
                          on new { Year = sales.Field<string>("年"), Month = sales.Field<string>("月") }
                          equals new { Year = weather.Field<string>("年"), Month = weather.Field<string>("月") }
                        join unit in unitsTable.AsEnumerable()
                          on sales.Field<string>("品種") equals unit.Field<string>("品種")
                        select new
                        {
                            部門 = sales.Field<string>("部門"),
                            中分類 = sales.Field<string>("中分類"),
                            品種 = sales.Field<string>("品種"),
                            年 = sales.Field<string>("年"),
                            月 = sales.Field<string>("月"),
                            販売量 = sales.Field<string>("販売量"),
                            平均気温 = weather.Field<string>("平均気温(℃)"),
                            最低気温 = weather.Field<string>("最低気温(℃)"),
                            最高気温 = weather.Field<string>("最高気温(℃)"),
                            降水量合計 = weather.Field<string>("降水量の合計(mm)"),
                            日照時間 = weather.Field<string>("日照時間(時間)"),
                            単位 = unit.Field<string>("単位")
                        };

            // DataTable生成
            DataTable mergedTable = new();
            mergedTable.Columns.Add("部門");
            mergedTable.Columns.Add("中分類");
            mergedTable.Columns.Add("品種");
            mergedTable.Columns.Add("年");
            mergedTable.Columns.Add("月");
            mergedTable.Columns.Add("販売量");
            mergedTable.Columns.Add("平均気温(℃)");
            mergedTable.Columns.Add("最低気温(℃)");
            mergedTable.Columns.Add("最高気温(℃)");
            mergedTable.Columns.Add("降水量の合計(mm)");
            mergedTable.Columns.Add("日照時間(時間)");
            mergedTable.Columns.Add("単位");

            foreach (var row in query)
            {
                mergedTable.Rows.Add(row.部門, row.中分類, row.品種, row.年, row.月, row.販売量,
                    row.平均気温, row.最低気温, row.最高気温, row.降水量合計, row.日照時間, row.単位);
            }
            return mergedTable;
        }

		/**
         * CSVファイル読み込みメソッド
         * @param path CSVファイルパス
         * @return 読み込んだDataTable
         */
		private static DataTable ReadCsv(string path)
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
    }

	/**
     * 2. MySQLテーブル結合クラス
     * @param connectionString MySQL接続文字列
     * @param salesTable 売上テーブル名
     * @param weatherTable 天気テーブル名
     * @param unitsTable 単位テーブル名
     */
	public class MySqlDataMerger(string connectionString, string salesTable, string weatherTable, string unitsTable) : DataMerger
    {

		/**
         * MySQLテーブル結合ロジック実装
         * @return 結合データテーブル
         */
		protected override DataTable GetMergedDataTableCore()
        {
			// MySQL接続とデータ取得
			using var conn = new MySqlConnection(connectionString);
            conn.Open();
			// 結合クエリ実行
			string sql = $@"
                SELECT s.department AS 部門, s.secondary_item AS 中分類, s.varety AS 品種, s.year AS 年, s.month AS 月, s.sales AS 売上,
                       w.average_temperature AS 平均気温, w.lowest_temperature AS 最低気温, w.maximum_temperature AS 最高気温,
                       w.precipitation AS 降水量合計, w.sunshine_hours AS 日照時間, u.unit AS 単位
                FROM {salesTable} s
                INNER JOIN {weatherTable} w ON s.year = w.year AND s.month = w.month
                INNER JOIN {unitsTable} u ON s.varety = u.varety
            ";
			// データ取得
			using var cmd = new MySqlCommand(sql, conn);
            using var adapter = new MySqlDataAdapter(cmd);
            DataTable table = new();
            adapter.Fill(table);
			// 結合データテーブルを返す
			return table;
        }
    }

    /**
     * ユーティリティクラス
     */
    public static class DataTableUtils
    {
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
