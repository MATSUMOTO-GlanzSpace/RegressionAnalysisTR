using RegressionAnalysis.Common;
using static RegressionAnalysis.Common.DataTableHelpers;
using System.Data;
using System.Data.Common;
using MySqlConnector;

namespace SalesAnalysisSource;

/// <summary>
/// Sales データマージャー基底class
/// </summary>
public abstract class SalesDataMerger : DataMerger
{
	public SalesDataMerger()
	{
	}

	/// <summary>
	/// マージ後の統一列名 -> SQL エイリアス付きカラムのマッピング
	/// </summary>
	public virtual Dictionary<string, string> MergedColumnToSqlMapping { get; } = [];

	/// <summary>
	/// JOIN 仕様を定義（MySQL側で実装）
	/// CSV側は LINQ to DataSet で直接 JOIN を記述するため実装しない
	/// </summary>
	public abstract JoinSpecification GetJoinSpecification();

	/// <summary>
	/// CSV ファイルから Sales/Weather/Units の結合データ取得
	/// </summary>
	public class CsvSalesDataMerger : SalesDataMerger
	{
		private readonly string _csvSalesPath;
		private readonly string _csvWeatherPath;
		private readonly string _csvUnitsPath;

		public CsvSalesDataMerger(string csvSalesPath, string csvWeatherPath, string csvUnitsPath)
		{
			_csvSalesPath = csvSalesPath;
			_csvWeatherPath = csvWeatherPath;
			_csvUnitsPath = csvUnitsPath;
		}

		/// <summary>
		/// CSV データ用 JOIN 仕様（デッドコード）
		/// 
		/// 仕様定義のため残している。
		/// 実装：GetMergedDataTableCore() 内で LINQ to DataSet により実現
		/// （複数キーJOINの複雑な結果処理を回避し、可読性を維持）
		/// </summary>
		public override JoinSpecification GetJoinSpecification()
		{
			return new JoinSpecification()
				.Join("sales", new[] { "年", "月" }, "weather", new[] { "年", "月" })
				.Join("sales", new[] { "品種" }, "units", new[] { "品種" });
		}

		protected override DataTable GetMergedDataTableCore()
		{
			// CSV ファイルを DataTable に読み込み（共通 Helper 利用）
			var salesTable = ReadCsv(_csvSalesPath);
			var weatherTable = ReadCsv(_csvWeatherPath);
			var unitsTable = ReadCsv(_csvUnitsPath);

			// LINQ to DataSet で JOIN を実装
			// 複数キーJOIN対応：year と month で weather テーブルと JOIN、品種で units テーブルと JOIN
			//
			var query = from s in salesTable.AsEnumerable()
					join w in weatherTable.AsEnumerable() 
						on new { year = s.Field<string>("年"), month = s.Field<string>("月") }
						equals new { year = w.Field<string>("年"), month = w.Field<string>("月") }
					join u in unitsTable.AsEnumerable() 
						on s.Field<string>("品種") equals u.Field<string>("品種")
					select new
					{
						部門 = s.Field<string>("部門"),
						大分類 = s.Field<string>("大分類"),
						中分類 = s.Field<string>("中分類"),
						品種 = s.Field<string>("品種"),
						年 = s.Field<string>("年"),
						月 = s.Field<string>("月"),
						売上 = s.Field<string>("売上"),
						平均気温 = w.Field<string>("平均気温(℃)"),
						最高気温 = w.Field<string>("最高気温(℃)"),
						最低気温 = w.Field<string>("最低気温(℃)"),
						降水量合計 = w.Field<string>("降水量の合計(mm)"),
						日照時間 = w.Field<string>("日照時間(時間)"),
						単位 = u.Field<string>("単位")
					};

			// DataMerger の共通フィルタ適用メソッドを利用
			var filteredQuery = ApplyFilters(query);

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

			// 結合データを DataTable に追加（SQL結果と同様にフラットなデータが取得される）
			foreach (var row in filteredQuery)
			{
				try
				{
					mergedTable.Rows.Add(
						row.部門, row.大分類, row.中分類, row.品種, row.年, row.月, row.売上,
						row.平均気温, row.最高気温, row.最低気温, row.降水量合計, row.日照時間, row.単位);
				}
				catch (Exception ex)
				{
					LogError(new Exception($"行データ変換エラー: {ex.Message}", ex));
					continue;
				}
			}

			return mergedTable;
		}
	}

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
		/// MySQL データ用 JOIN 仕様の定義
		/// </summary>
		public override JoinSpecification GetJoinSpecification()
		{
			return new JoinSpecification()
				.Join("sales", new[] { "year", "month" }, "weather", new[] { "year", "month" })
				.Join("sales", new[] { "variety" }, "units", new[] { "variety" });
		}

		/// <summary>
		/// MySQL データベース接続を取得
		/// </summary>
		protected override DbConnection GetDbConnection()
		{
			return MySqlConnectionFactory.CreateOpenConnection();
		}

		/// <summary>
		/// MySQL テーブル結合ロジック実装
		/// GetJoinSpecification() から SQL JOIN 句を自動生成
		/// </summary>
		/// <returns>結合データテーブル</returns>
		protected override DataTable GetMergedDataTableCore()
		{
			// GetJoinSpecification() から JOIN 条件を取得して SQL JOIN 句を自動生成
			var joinSpec = GetJoinSpecification();
			var joinClauses = BuildSqlJoinClauses(joinSpec);

			// DataMerger の共通 SQL フィルタ生成を利用（generic版）
			var whereSql = BuildSqlWhereClause<MySqlParameter>(
				"sales", 
				(fieldName, value) => new MySqlParameter($"@p{Guid.NewGuid():N}", value),
				out var parameters);

			string sql = $@"
				SELECT s.department AS 部門, s.primary_item AS 大分類, s.secondary_item AS 中分類, s.variety AS 品種, s.year AS 年, s.month AS 月, s.sales AS 売上,
					   w.average_temperature AS 平均気温, w.maximum_temperature AS 最高気温, w.lowest_temperature AS 最低気温,
					   w.precipitation AS 降水量合計, w.sunshine_hours AS 日照時間, u.unit AS 単位
				FROM {salesTable} s
				{joinClauses}
				{whereSql}
				";

			// 共通 SQL 実行メソッドへ委譲（generic版）
			return ExecuteMergedSqlQuery<MySqlParameter>(sql, parameters);
		}


	}
}
