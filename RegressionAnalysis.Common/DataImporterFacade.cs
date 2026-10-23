using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RegressionAnalysis.Common;

/// <summary>
/// DataImporterFacade クラスは、複数のデータソースを統合するためのファサードクラスです。
/// </summary>
/// <remarks>
/// このクラスは、IMergeStrategy インターフェースを使用して、複数の IImportSource からデータを取得し、統合された DataTable を提供します。
/// </remarks>
public sealed class DataImporterFacade : DataImporter
{
	/// <summary>
	/// マージ戦略
	/// </summary>
	private readonly IMergeStrategy _strategy;
	/// <summary>
	/// データソースの辞書
	/// </summary>
	private readonly IReadOnlyDictionary<string, IImportSource> _sources;

	/// <summary>
	/// コンストラクター
	/// </summary>
	/// <param name="strategy">マージ戦略</param>
	/// <param name="sources">データソースの辞書</param>
	public DataImporterFacade(IMergeStrategy strategy, IReadOnlyDictionary<string, IImportSource> sources)
		: base(strategy.GetType().Name, null)
	{
		_strategy = strategy;
		_sources = sources;
	}

	/// <summary>
	/// データソースからデータを取得し、マージ戦略に従って統合されたデータテーブルを返します。
	/// </summary>
	protected override DataTable GetTableCore()
	{
		var merged = _strategy.Merge(_sources);

		// Facade自身に設定されたフィルタ条件（AddFilterCondition）をマージ結果に適用する。
		foreach (var cond in Filters.OfType<FilterCondition>())
		{
			var predicate = DataMerger.BuildRowPredicate(cond);
			merged = DataMerger.ApplyRowFilter(merged, predicate);
		}

		return merged;
	}

	/// <summary>
	/// 統合されたデータテーブルの列名を取得します。
	/// </summary>
	/// <returns>統合されたデータテーブルの列名の配列</returns>
	public override string[] GetTableColumnNames()
	{
		return _strategy.GetMergedColumnNames();
	}
}