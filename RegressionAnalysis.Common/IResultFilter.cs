using System.Data;

namespace RegressionAnalysis.Common;

/// <summary>
/// 結果フィルタのインターフェース
/// </summary>
public interface IResultFilter
{
	/// <summary>
	/// 結果フィルタを適用する。
	/// </summary>
	/// <param name="table">入力データテーブル</param>
	/// <returns>フィルタ適用後のデータテーブル</returns>
	DataTable Apply(DataTable table);
}