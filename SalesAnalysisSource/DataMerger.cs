using MySqlConnector;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

namespace SalesAnalysisSource
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

		/**
         * フィルタ条件リスト
         */
		public List<FilterElement> Filters { get; } = [];

		/**
         * フィルタ条件追加メソッド
         * @param logic 論理演算子（AND/OR）
         * @param field フィールド名（テーブル名.フィールド名形式）
         * @param condition 条件式（例: "='value'"）
         */
		public void AddFilterCondition(string logic, string field, string condition)
            => Filters.Add(new FilterCondition(logic, field, condition));

		/**
         * フィルタグループ開始追加メソッド
         * @param logic 論理演算子（AND/OR）
         */
		public void AddGroupStart(string logic = "")
            => Filters.Add(new FilterGroupStart(logic));

		/**
         * フィルタグループ終了追加メソッド
         */
		public void AddGroupEnd()
            => Filters.Add(new FilterGroupEnd());

		/**
         * CSV用フィルタ述語生成
         * @param tableName テーブル名
         * @return フィルタ述語
         * @note Shunting Yard AlgorithmでRPN変換し評価
         * @see https://en.wikipedia.org/wiki/Shunting_yard_algorithm
         */
		public Func<DataRow, bool> BuildCsvFilterPredicate(string tableName)
        {
			// Shunting Yard AlgorithmでRPN変換
			// RPN: Reverse Polish Notation（逆ポーランド記法）
			// 出力キューと演算子スタック
			var output = new List<object>();
            var ops = new Stack<string>();
            foreach (var filter in Filters)
            {
                if (filter is FilterCondition cond && cond.Field.StartsWith(tableName + "."))
                {
					// フィルタ条件を出力キューに追加
					output.Add(cond);
                }
                else if (filter is FilterGroupStart)
                {
					// グループ開始を演算子スタックに追加
					ops.Push("(");
                }
                else if (filter is FilterGroupEnd)
                {
					// グループ終了まで演算子を出力キューに移動
					while (ops.Count > 0 && ops.Peek() != "(")
                        output.Add(ops.Pop());
                    if (ops.Count > 0) ops.Pop(); // remove "("
                }
                else if (filter is FilterCondition logicCond && (logicCond.Logic == "AND" || logicCond.Logic == "OR"))
                {
					// 論理演算子の優先度に基づき演算子スタックから出力キューに移動
					while (ops.Count > 0 && Precedence(ops.Peek()) >= Precedence(logicCond.Logic))
                        output.Add(ops.Pop());
                    ops.Push(logicCond.Logic);
                }
            }
			// 残りの演算子を出力キューに移動
			while (ops.Count > 0) output.Add(ops.Pop());

            // RPN評価
            var stack = new Stack<Func<DataRow, bool>>();
            foreach (var token in output)
            {
                if (token is FilterCondition cond && cond.Field.StartsWith(tableName + "."))
                {
					// フィルタ条件を述語に変換
					var field = cond.Field.Substring(tableName.Length + 1);
                    var value = cond.Condition.Trim('=', '\'', '"');
                    stack.Push(row =>
                    {
                        if (!row.Table.Columns.Contains(field)) return false;
                        return row.Field<string>(field) == value;
                    });
                }
                else if (token is string op && (op == "AND" || op == "OR"))
                {
					// 論理演算子適用
					var right = stack.Pop();
                    var left = stack.Pop();
                    if (op == "AND") stack.Push(row => left(row) && right(row));
                    else stack.Push(row => left(row) || right(row));
                }
            }
            return stack.Count > 0 ? stack.Pop() : row => true;

			// 演算子の優先度
			static int Precedence(string op) => op == "AND" ? 2 : op == "OR" ? 1 : 0;
        }

		/**
         * SQL用WHERE句生成（tableName指定、パラメータ化）
         * @param tableName テーブル名
         * @out parameters パラメータリスト出力
         * @return WHERE句文字列
         */
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
                clauses.Add($"{(string.IsNullOrEmpty(cond.Logic) ? "" : cond.Logic + " ")}{tableName.Substring(0,1)}.{field} = {paramName}");
                parameters.Add(new MySqlParameter(paramName, cond.Condition.Trim('=', '\'', '"')));
                paramIndex++;
            }
			return clauses.Count > 0 ? ("WHERE " + string.Join(" ", clauses)) : "";
        }
    }

	/**
     * フィルタ要素基底レコード
     */
	public abstract record FilterElement;

	/**
     * フィルタ条件レコード
     * @param Logic 論理演算子（AND/OR）
     * @param Field フィールド名（テーブル名.フィールド名形式）
     * @param Condition 条件式（例: "='value'"）
     * @return フィルタ条件レコード
     */
	public record FilterCondition(string Logic, string Field, string Condition) : FilterElement;

	/**
     * フィルタグループ開始レコード"("
     * @param Logic 論理演算子（AND/OR）
     * @return フィルタグループ開始レコード
     */
	public record FilterGroupStart(string Logic = "") : FilterElement;

	/**
     * フィルタグループ終了レコード")"
     * @return フィルタグループ終了レコード
     */
	public record FilterGroupEnd() : FilterElement;

	/**
     * 1. CSVファイル結合クラス
     * @param salesCsvPath 売上CSVファイルパス
     * @param weatherCsvPath 天気CSVファイルパス
     * @param unitsCsvPath 単位CSVファイルパス
     */
	public class CsvSalesDataMerger(string salesCsvPath, string weatherCsvPath, string unitsCsvPath) : DataMerger
    {

		/**
         * CSVファイル結合ロジック実装
         * @return 結合データテーブル
         */
		protected override DataTable GetMergedDataTableCore()
        {
            // CSV読み込み
            DataTable salesTable, weatherTable, unitsTable;
            try
            {
                salesTable = DataTableUtils.ReadCsv(salesCsvPath);
                weatherTable = DataTableUtils.ReadCsv(weatherCsvPath);
                unitsTable = DataTableUtils.ReadCsv(unitsCsvPath);
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

	/**
     * 2. MySQLテーブル結合クラス
     * @param connectionString MySQL接続文字列
     * @param salesTable 売上テーブル名
     * @param weatherTable 天気テーブル名
     * @param unitsTable 単位テーブル名
     */
	public class MySqlSalesDataMerger(string connectionString, string salesTable, string weatherTable, string unitsTable) : DataMerger
    {

		/**
         * MySQLテーブル結合ロジック実装
         * @return 結合データテーブル
         */
		protected override DataTable GetMergedDataTableCore()
        {
			using var conn = new MySqlConnection(connectionString);
            conn.Open();
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
    }
}
