using System.Collections.Generic;
using System.Data;

namespace RegressionAnalysis.Common;

/// <summary>
/// データ取得元のインターフェース。
/// データ取得元は、CSVファイル、Excelファイル、データベースなどが考えられる。
/// </summary>
public interface IImportSource
{
	/// <summary>
	/// データ取得元の名前を取得します。
	/// </summary>
	string Name { get; }

	/// <summary>
	/// データ取得元からデータテーブルを取得します。
	/// </summary>
	/// <returns>データテーブル</returns>
	DataTable GetTable();
	
	/// <summary>
	/// データ取得元の列名を取得します。
	/// </summary>
	/// <returns>列名の配列</returns>
	string[] GetColumnNames();
	
	/// <summary>
	/// フィルター条件のリストを取得します。
	/// </summary>
	List<FilterElement> Filters { get; }
	
	/// <summary>
	/// フィルター条件を追加します。
	/// </summary>
	/// <param name="logic">論理演算子</param>
	/// <param name="field">フィールド名</param>
	/// <param name="filterOperator">フィルター演算子</param>
	/// <param name="value">値</param>
	void AddFilterCondition(string logic, string field, FilterOperator filterOperator, string value);
}

/// <summary>
/// データベースからのデータ取得元のインターフェース。
/// </summary>
public interface IDbImportSource : IImportSource
{
	/// <summary>
	/// データベースの方言を取得します。
	/// </summary>
	DBDialect Dialect { get; }
}
