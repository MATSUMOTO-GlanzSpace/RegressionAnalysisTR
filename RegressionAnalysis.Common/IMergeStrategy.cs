using System.Collections.Generic;
using System.Data;

namespace RegressionAnalysis.Common;

/// <summary>
/// マージ処理の共通インターフェース。
/// </summary>
public interface IMergeStrategy
{
	/// <summary>
	/// マージ処理を実行する。
	/// </summary>
	DataTable Merge(IReadOnlyDictionary<string, IImportSource> sources);
	
	/// <summary>
	/// マージ後の列名を取得する。
	/// </summary>
	string[] GetMergedColumnNames();
}