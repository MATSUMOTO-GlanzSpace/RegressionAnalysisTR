using MySqlConnector;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using RegressionAnalysis.Common;

namespace SalesAnalysisSource
{
	/// <summary>
    /// 1. CSVファイル結合クラス
    /// </summary>
    /// <param name="salesCsvPath">売上CSVファイルパス</param>
    /// <param name="weatherCsvPath">天気CSVファイルパス</param>
    /// <param name="unitsCsvPath">単位CSVファイルパス</param>
	public class CsvSalesDataMerger(string salesCsvPath, string weatherCsvPath, string unitsCsvPath) : DataMerger
    {

		/// <summary>
        /// CSVファイル結合ロジック実装
        /// </summary>
        /// <returns>結合データテーブル</returns>
		protected override DataTable GetMergedDataTableCore()
        {
            // CSV読み込み
            DataTable salesTable, weatherTable, unitsTable;
            try
            {
                salesTable = DataTableHelpers.ReadCsv(salesCsvPath);
                weatherTable = DataTableHelpers.ReadCsv(weatherCsvPath);
                unitsTable = DataTableHelpers.ReadCsv(unitsCsvPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"CSVファイルの読み込みに失敗しました。(ファイルパス: {salesCsvPath}, {weatherCsvPath}, {unitsCsvPath})", ex);
            }

            // LINQで結合
            var query =
                from sales in salesTable.AsEnumerable()
                join weather in weatherTable.AsEnumerable()
                  on new { Year = sales.Field<string>("年"), Month = sales.Field<string>("月") }
                  equals new { Year = weather.Field<string>("年"), Month = weather.Field<string>("月") }
                join unit in unitsTable.AsEnumerable()
                  on sales.Field<string>("品種") equals unit.Field<string>("品種")
                select new
                {
                    sales,
                    部門 = sales.Field<string>("部門"),
                    大分類 = sales.Field<string>("大分類"),
                    中分類 = sales.Field<string>("中分類"),
                    品種 = sales.Field<string>("品種"),
                    年 = sales.Field<string>("年"),
                    月 = sales.Field<string>("月"),
                    売上 = sales.Field<string>("売上"),
                    平均気温 = weather.Field<string>("平均気温(℃)"),
                    最高気温 = weather.Field<string>("最高気温(℃)"),
                    最低気温 = weather.Field<string>("最低気温(℃)"),
                    降水量合計 = weather.Field<string>("降水量の合計(mm)"),
                    日照時間 = weather.Field<string>("日照時間(時間)"),
                    単位 = unit.Field<string>("単位")
                };

            // DataMergerの共通フィルタ述語を利用
            var predicate = BuildCsvFilterPredicate("sales");
            query = query.Where(x => predicate(x.sales));

            // DataTable生成
            DataTable mergedTable = new();
            mergedTable.Columns.Add("部門");
            mergedTable.Columns.Add("大分類");
            mergedTable.Columns.Add("中分類");
            mergedTable.Columns.Add("品種");
            mergedTable.Columns.Add("年");
            mergedTable.Columns.Add("月");
            mergedTable.Columns.Add("売上");
            mergedTable.Columns.Add("平均気温(℃)");
            mergedTable.Columns.Add("最高気温(℃)");
            mergedTable.Columns.Add("最低気温(℃)");
            mergedTable.Columns.Add("降水量の合計(mm)");
            mergedTable.Columns.Add("日照時間(時間)");
            mergedTable.Columns.Add("単位");
			// 結合データをDataTableに追加
			foreach (var row in query)
            {
                mergedTable.Rows.Add(row.部門, row.大分類, row.中分類, row.品種, row.年, row.月, row.売上,
                    row.平均気温, row.最高気温, row.最低気温, row.降水量合計, row.日照時間, row.単位);
            }
            return mergedTable;
        }
	}

	/// <summary>
    /// 2. MySQLテーブル結合クラス
    /// </summary>
    /// <param name="salesTable">売上テーブル名</param>
    /// <param name="weatherTable">天気テーブル名</param>
    /// <param name="unitsTable">単位テーブル名</param>
	public class MySqlSalesDataMerger(string salesTable, string weatherTable, string unitsTable) : DataMerger
    {

		/// <summary>
        /// MySQLテーブル結合ロジック実装
        /// </summary>
        /// <returns>結合データテーブル</returns>
		protected override DataTable GetMergedDataTableCore()
        {
			using var conn = MySqlConnectionFactory.CreateOpenConnection();
            // DataMergerの共通SQLフィルタ生成を利用
            var whereSql = BuildSqlWhereClause("sales", out var parameters);
            string sql = $@"
                SELECT s.department AS 部門, s.primary_item AS 大分類, s.secondary_item AS 中分類, s.variety AS 品種, s.year AS 年, s.month AS 月, s.sales AS 売上,
                       w.average_temperature AS 平均気温, w.maximum_temperature AS 最高気温, w.lowest_temperature AS 最低気温,
                       w.precipitation AS 降水量合計, w.sunshine_hours AS 日照時間, u.unit AS 単位
                FROM {salesTable} s
                INNER JOIN {weatherTable} w ON s.year = w.year AND s.month = w.month
                INNER JOIN {unitsTable} u ON s.variety = u.variety
                {whereSql}
            ";
			// SQL実行
			using var cmd = new MySqlCommand(sql, conn);
			foreach (var p in parameters) cmd.Parameters.Add(p);
            using var adapter = new MySqlDataAdapter(cmd);
            DataTable table = new();
            adapter.Fill(table);
			// 結合データテーブルを返す
			return table;
        }

		/// <summary>
		/// SQL用WHERE句生成（tableName指定、パラメータ化）
		/// </summary>
		/// <param name="tableName">テーブル名</param>
		/// <param name="parameters">パラメータリスト出力</param>
		/// <returns>WHERE句文字列</returns>
		public string BuildSqlWhereClause(string tableName, out List<MySqlParameter> parameters)
		{
			// WHERE句生成
			var clauses = new List<string>();
            parameters = [];
			int paramIndex = 0;
			foreach (var cond in Filters.OfType<FilterCondition>().Where(f => f.Field.StartsWith(tableName + ".")))
			{
				// フィルタ条件をWHERE句に変換
				var field = cond.Field.Substring(tableName.Length + 1);
				var paramName = "@p" + paramIndex;
                var condTrim = cond.Condition.Trim();
                if (condTrim.StartsWith("LIKE", StringComparison.OrdinalIgnoreCase))
                {
                    clauses.Add($"{(string.IsNullOrEmpty(cond.Logic) ? "" : cond.Logic + " ")}{tableName.Substring(0, 1)}.{field} LIKE {paramName}");
                    var pattern = condTrim.Substring(4).Trim().Trim('\'', '"');
                    parameters.Add(new MySqlParameter(paramName, pattern));
                }
                else
                {
                    clauses.Add($"{(string.IsNullOrEmpty(cond.Logic) ? "" : cond.Logic + " ")}{tableName.Substring(0, 1)}.{field} = {paramName}");
                    parameters.Add(new MySqlParameter(paramName, condTrim.Trim('=', '\'', '"')));
                }
				paramIndex++;
			}
			return clauses.Count > 0 ? ("WHERE " + string.Join(" ", clauses)) : "";
		}
	}
}
