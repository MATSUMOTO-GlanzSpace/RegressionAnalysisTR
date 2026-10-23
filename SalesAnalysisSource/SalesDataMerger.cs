using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RegressionAnalysis.Common;

namespace SalesAnalysisSource;

/// <summary>
/// 売上データマージ処理の共通基底クラス
/// </summary>
public abstract class SalesDataMerger : DataMerger
{
	/// <summary>
	/// マージ後の列名を取得する
	/// </summary>
	public override string[] GetMergedColumnNames() =>
	[
		"部門", "大分類", "中分類", "品種", "年", "月", "売上",
		"平均気温", "最高気温", "最低気温", "降水量合計", "日照時間", "単位"
	];

	/// <summary>
	/// CSVデータのマージ 
	/// </summary>
	public class CsvSalesDataMerger : SalesDataMerger
	{
		/// <summary>
		/// CSVソースをマージして1つのDataTableを作成する
		/// </summary>
		protected override DataTable MergeCore(IReadOnlyDictionary<string, IImportSource> sources)
		{
			// CSVソースを取得する
			var salesSource = RequireSource<IImportSource>(sources, "sales");
			var weatherSource = RequireSource<IImportSource>(sources, "weather");
			var unitsSource = RequireSource<IImportSource>(sources, "units");
			// DataTableを取得する
			var salesTable = salesSource.GetTable();
			var weatherTable = weatherSource.GetTable();
			var unitsTable = unitsSource.GetTable();
			// LINQを使用して、売上データ、気象データ、単位データを結合する
			var query =
				from s in salesTable.AsEnumerable()
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

			var mergedTable = new DataTable();
			foreach (var col in GetMergedColumnNames())
			{
				// マージ後の列名を追加する
				mergedTable.Columns.Add(col);
			}

			foreach (var row in query)
			{
				// マージ後の行を追加する
				mergedTable.Rows.Add(
					row.部門, row.大分類, row.中分類, row.品種, row.年, row.月, row.売上,
					row.平均気温, row.最高気温, row.最低気温, row.降水量合計, row.日照時間, row.単位
				);
			}
			// マージ後のDataTableを返す
			return mergedTable;
		}
	}

	/// <summary>
	/// DBデータのマージ
	/// </summary>
	public class DbSalesDataMerger : SalesDataMerger
	{
		/// <summary>
		/// DBソースをマージして1つのDataTableを作成する
		/// </summary>
		/// <param name="sources">ソースの辞書</param>
		/// <returns>マージ後のDataTable</returns>
		protected override DataTable MergeCore(IReadOnlyDictionary<string, IImportSource> sources)
		{
			// DBソースを取得する
			var salesSource = RequireSource<IDbImportSource>(sources, "sales");
			var weatherSource = RequireSource<IDbImportSource>(sources, "weather");
			var unitsSource = RequireSource<IDbImportSource>(sources, "units");
			// DataTableを取得する
			var salesTable = salesSource.GetTable();
			var weatherTable = weatherSource.GetTable();
			var unitsTable = unitsSource.GetTable();
			// LINQを使用して、売上データ、気象データ、単位データを結合する
			var query =
				from s in salesTable.AsEnumerable()
				join w in weatherTable.AsEnumerable()
					on new { year = s.Field<int>("year"), month = s.Field<int>("month") }
					equals new { year = w.Field<int>("year"), month = w.Field<int>("month") }
				join u in unitsTable.AsEnumerable()
					on s.Field<string>("variety") equals u.Field<string>("variety")
				select new
				{
					部門 = s.Field<string>("department"),
					大分類 = s.Field<string>("primary_item"),
					中分類 = s.Field<string>("secondary_item"),
					品種 = s.Field<string>("variety"),
					年 = s.Field<int>("year").ToString(),
					月 = s.Field<int>("month").ToString(),
					売上 = s.Field<decimal>("sales").ToString(),
					平均気温 = w.Field<decimal>("average_temperature").ToString(),
					最高気温 = w.Field<decimal>("maximum_temperature").ToString(),
					最低気温 = w.Field<decimal>("lowest_temperature").ToString(),
					降水量合計 = w.Field<decimal>("precipitation").ToString(),
					日照時間 = w.Field<decimal>("sunshine_hours").ToString(),
					単位 = u.Field<string>("unit")
				};

			// マージ後のDataTableを作成する
			var mergedTable = new DataTable();
			foreach (var col in GetMergedColumnNames())
			{
				// マージ後の列名を追加する
				mergedTable.Columns.Add(col);
			}

			foreach (var row in query)
			{
				// マージ後の行を追加する
				mergedTable.Rows.Add(
					row.部門, row.大分類, row.中分類, row.品種, row.年, row.月, row.売上,
					row.平均気温, row.最高気温, row.最低気温, row.降水量合計, row.日照時間, row.単位
				);
			}
			// マージ後のDataTableを返す
			return mergedTable;
		}
	}
}