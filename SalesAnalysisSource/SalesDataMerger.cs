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
    /// 売上データ結合クラス基底
    /// </summary>
    public abstract class SalesDataMerger : DataMerger
    {
        private static readonly string[] UnifiedMergedColumnNames =
        {
            "部門","大分類","中分類","品種","年","月","売上",
            "平均気温","最高気温","最低気温","降水量合計","日照時間","単位"
        };

        /// <summary>
        /// マージ後の列名一覧を返す
        /// </summary>
        public override string[] GetMergedDataTableColumnNames()
        {
            return (string[])UnifiedMergedColumnNames.Clone();
        }
    }

	/// <summary>
    /// 1. CSVファイル結合クラス
    /// </summary>
    /// <param name="salesCsvPath">売上CSVファイルパス</param>
    /// <param name="weatherCsvPath">天気CSVファイルパス</param>
    /// <param name="unitsCsvPath">単位CSVファイルパス</param>
    public class CsvSalesDataMerger(string salesCsvPath, string weatherCsvPath, string unitsCsvPath) : SalesDataMerger
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

            // DataMerger の Filters をマージ後の匿名オブジェクト上で評価する述語に変換して適用
            if (Filters != null && Filters.Count > 0)
            {
                // Shunting Yard Algorithm で RPN 変換
                var output = new List<object>();
                var ops = new Stack<string>();
                foreach (var filter in Filters)
                {
                    if (filter is FilterCondition cond)
                    {
                        // 論理演算子扱い
                        if (cond.Logic == "AND" || cond.Logic == "OR")
                        {
                            while (ops.Count > 0 && Precedence(ops.Peek()) >= Precedence(cond.Logic))
                                output.Add(ops.Pop());
                            ops.Push(cond.Logic);
                        }
                        else
                        {
                            output.Add(cond);
                        }
                    }
                    else if (filter is FilterGroupStart)
                    {
                        ops.Push("(");
                    }
                    else if (filter is FilterGroupEnd)
                    {
                        while (ops.Count > 0 && ops.Peek() != "(") output.Add(ops.Pop());
                        if (ops.Count > 0) ops.Pop();
                    }
                }
                while (ops.Count > 0) output.Add(ops.Pop());

                // RPN 評価で匿名オブジェクト用述語を作成
                Func<object, bool> mergedPredicate;
                {
                    var stack = new Stack<Func<object, bool>>();
                    foreach (var token in output)
                    {
                        if (token is FilterCondition condToken)
                        {
                            var field = condToken.Field;
                            var condTrim = condToken.Condition.Trim();
                            if (condTrim.StartsWith("LIKE", StringComparison.OrdinalIgnoreCase))
                            {
                                var pattern = condTrim.Substring(4).Trim().Trim('\'', '"');
                                var startsWithWildcard = pattern.StartsWith("%");
                                var endsWithWildcard = pattern.EndsWith("%");
                                var core = pattern.Trim('%');
                                stack.Push(obj =>
                                {
                                    if (obj == null) return false;
                                    // フィールド参照
                                    if (field.Contains('.'))
                                    {
                                        var parts = field.Split('.', 2);
                                        var tbl = parts[0];
                                        var col = parts[1];
                                        // sales/weather/unit の DataRow プロパティを経由
                                        var prop = obj.GetType().GetProperty(tbl);
                                        if (prop == null) return false;
                                        var row = prop.GetValue(obj) as DataRow;
                                        if (row == null) return false;
                                        var val = row.Table.Columns.Contains(col) ? row.Field<string>(col) : null;
                                        if (val == null) return false;
                                        if (startsWithWildcard && endsWithWildcard) return val.Contains(core);
                                        if (startsWithWildcard) return val.EndsWith(core);
                                        if (endsWithWildcard) return val.StartsWith(core);
                                        return val == core;
                                    }
                                    else
                                    {
                                        var p = obj.GetType().GetProperty(field);
                                        if (p == null) return false;
                                        var v = p.GetValue(obj)?.ToString();
                                        if (v == null) return false;
                                        if (startsWithWildcard && endsWithWildcard) return v.Contains(core);
                                        if (startsWithWildcard) return v.EndsWith(core);
                                        if (endsWithWildcard) return v.StartsWith(core);
                                        return v == core;
                                    }
                                });
                            }
                            else
                            {
                                var value = condTrim.Trim('=', '\'', '"');
                                stack.Push(obj =>
                                {
                                    if (obj == null) return false;
                                    if (field.Contains('.'))
                                    {
                                        var parts = field.Split('.', 2);
                                        var tbl = parts[0];
                                        var col = parts[1];
                                        var prop = obj.GetType().GetProperty(tbl);
                                        if (prop == null) return false;
                                        var row = prop.GetValue(obj) as DataRow;
                                        if (row == null) return false;
                                        if (!row.Table.Columns.Contains(col)) return false;
                                        return row.Field<string>(col) == value;
                                    }
                                    else
                                    {
                                        var p = obj.GetType().GetProperty(field);
                                        if (p == null) return false;
                                        return (p.GetValue(obj)?.ToString() ?? "") == value;
                                    }
                                });
                            }
                        }
                        else if (token is string op && (op == "AND" || op == "OR"))
                        {
                            var right = stack.Pop();
                            var left = stack.Pop();
                            if (op == "AND") stack.Push(obj => left(obj) && right(obj));
                            else stack.Push(obj => left(obj) || right(obj));
                        }
                    }
                    mergedPredicate = stack.Count > 0 ? stack.Pop() : (obj => true);
                }

                query = query.Where(x => mergedPredicate(x));
            }

            static int Precedence(string op) => op == "AND" ? 2 : op == "OR" ? 1 : 0;

            // DataTable生成
            DataTable mergedTable = new();
            mergedTable.Columns.Add("部門");
            mergedTable.Columns.Add("大分類");
            mergedTable.Columns.Add("中分類");
            mergedTable.Columns.Add("品種");
            mergedTable.Columns.Add("年");
            mergedTable.Columns.Add("月");
            mergedTable.Columns.Add("売上");
            mergedTable.Columns.Add("平均気温");
            mergedTable.Columns.Add("最高気温");
            mergedTable.Columns.Add("最低気温");
            mergedTable.Columns.Add("降水量合計");
            mergedTable.Columns.Add("日照時間");
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
    public class MySqlSalesDataMerger(string salesTable, string weatherTable, string unitsTable) : SalesDataMerger
    {

        /// <summary>
        /// マージ後列名 -> SQL エイリアス付きカラムのマッピング
        /// </summary>
        public override Dictionary<string, string> MergedColumnToSqlMapping { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["部門"] = "s.department",
            ["大分類"] = "s.primary_item",
            ["中分類"] = "s.secondary_item",
            ["品種"] = "s.variety",
            ["年"] = "s.year",
            ["月"] = "s.month",
            ["売上"] = "s.sales",
            ["平均気温"] = "w.average_temperature",
            ["最高気温"] = "w.maximum_temperature",
            ["最低気温"] = "w.lowest_temperature",
            ["降水量合計"] = "w.precipitation",
            ["日照時間"] = "w.sunshine_hours",
            ["単位"] = "u.unit"
        };

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
            // WHERE句生成（Filters の内容をマージ後列名または table.field で解釈して SQL に変換）
            var clauses = new List<string>();
            parameters = new List<MySqlParameter>();
            int paramIndex = 0;
            // マージ後列名 -> alias.column のマッピングは DataMerger 側のプロパティを利用
            var map = this.MergedColumnToSqlMapping ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var cond in Filters.OfType<FilterCondition>())
            {
                string left = null;
                if (cond.Field.Contains('.'))
                {
                    // table.field 形式 -> alias.field
                    var parts = cond.Field.Split('.', 2);
                    var tbl = parts[0];
                    var col = parts[1];
                    var alias = tbl.Length > 0 ? tbl.Substring(0, 1) : tbl; // sales -> s
                    left = $"{alias}.{col}";
                }
                else if (map.TryGetValue(cond.Field, out var mapped))
                {
                    left = mapped;
                }
                else
                {
                    // 未知のフィールドは無視
                    continue;
                }

                var paramName = "@p" + paramIndex;
                var condTrim = cond.Condition.Trim();
                if (condTrim.StartsWith("LIKE", StringComparison.OrdinalIgnoreCase))
                {
                    clauses.Add($"{(string.IsNullOrEmpty(cond.Logic) ? "" : cond.Logic + " ")}{left} LIKE {paramName}");
                    var pattern = condTrim.Substring(4).Trim().Trim('\'', '"');
                    parameters.Add(new MySqlParameter(paramName, pattern));
                }
                else
                {
                    clauses.Add($"{(string.IsNullOrEmpty(cond.Logic) ? "" : cond.Logic + " ")}{left} = {paramName}");
                    parameters.Add(new MySqlParameter(paramName, condTrim.Trim('=', '\'', '"')));
                }
                paramIndex++;
            }
            return clauses.Count > 0 ? ("WHERE " + string.Join(" ", clauses)) : "";
		}
	}
}
