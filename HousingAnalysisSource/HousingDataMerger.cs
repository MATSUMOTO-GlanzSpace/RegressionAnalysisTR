using RegressionAnalysis.Common;
using static RegressionAnalysis.Common.DataTableHelpers;
using System.Data;
using System.Data.Common;

namespace HousingAnalysisSource;

public abstract class HousingDataMerger : DataMerger
{
	/// <summary>
	/// マージ後の列名一覧
	/// </summary>
	//public override string[] GetMergedDataTableColumnNames() =>
	//[
	//	"経度", "緯度", "住宅築年数の中央値", "総部屋数", "総寝室数", "人口", "世帯数",
	//"世帯収入の中央値", "外洋まで1時間以内", "海岸線の近く", "湾沿い", "島しょ部", "住宅価格の中央値"
	//];

	/// <summary>
	/// マージ後の統一列名 -> SQL エイリアス付きカラムのマッピング
	/// </summary>
	//public override Dictionary<string, string> MergedColumnToSqlMapping { get; } = [];

	/// <summary>
	/// CSV ファイルから Housing の結合データ取得
	/// </summary>
	public class CsvHousingDataMerger(string csvHousingPath) : HousingDataMerger
	{
		protected override DataTable GetMergedDataTableCore()
		{
			// CSV ファイルを DataTable に読み込み（共通 Helper 利用）
			var housingTable = ReadCsv(csvHousingPath);

			// LINQ to DataSet実装
			var query = from s in housingTable.AsEnumerable()
						select new
						{
							経度 = s.Field<string>("longitude"),
							緯度 = s.Field<string>("latitude"),
							住宅築年数の中央値 = s.Field<string>("housing_median_age"),
							総部屋数 = s.Field<string>("total_rooms"),
							総寝室数 = s.Field<string>("total_bedrooms"),
							人口 = s.Field<string>("population"),
							世帯数 = s.Field<string>("households"),
							世帯収入の中央値 = s.Field<string>("median_household_income"),
							外洋まで1時間以内 = s.Field<string>("OCEAN"),
							海岸線の近く = s.Field<string>("NEAR_OCEAN"),
							湾沿い = s.Field<string>("NEAR_BAY"),
							島しょ部 = s.Field<string>("ISLAND"),
							住宅価格の中央値 = s.Field<string>("median_house_value")
						};

			// DataMerger の共通フィルタ適用メソッドを利用
			var filteredQuery = ApplyFilters(query);

			// DataTable生成
			DataTable mergedTable = new();
			mergedTable.Columns.Add("経度");
			mergedTable.Columns.Add("緯度");
			mergedTable.Columns.Add("住宅築年数の中央値");
			mergedTable.Columns.Add("総部屋数");
			mergedTable.Columns.Add("総寝室数");
			mergedTable.Columns.Add("人口");
			mergedTable.Columns.Add("世帯数");
			mergedTable.Columns.Add("世帯収入の中央値");
			mergedTable.Columns.Add("外洋まで1時間以内");
			mergedTable.Columns.Add("海岸線の近く");
			mergedTable.Columns.Add("湾沿い");
			mergedTable.Columns.Add("島しょ部");
			mergedTable.Columns.Add("住宅価格の中央値");

			// 結合データを DataTable に追加（SQL結果と同様にフラットなデータが取得される）
			foreach (var row in filteredQuery)
			{
				try
				{
					mergedTable.Rows.Add(
						row.経度, row.緯度, row.住宅築年数の中央値, row.総部屋数, row.総寝室数, row.人口, row.世帯数,
						row.世帯収入の中央値, row.外洋まで1時間以内, row.海岸線の近く, row.湾沿い, row.島しょ部, row.住宅価格の中央値);
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
}

