using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace RegressionAnalysis.Common;

/// <summary>
/// マージ処理の共通基底クラス。
/// DataImporter から分離し、実データ取得は source で行う。
/// 結合後のフィルタは IMergeStrategy 側で扱う。
/// </summary>
public abstract class DataMerger : IMergeStrategy
{
	/// <summary>
	/// マージ後のフィルタ条件。
	/// </summary>
	public List<FilterElement> MergeFilters { get; } = [];

	/// <summary>
	/// マージ後の結果フィルタ。
	/// </summary>
	public List<IResultFilter> ResultFilters { get; } = [];

	/// <summary>
	/// マージ後のフィルタ条件を追加する。
	/// </summary>
	public void AddMergeFilter(string logic, string field, FilterOperator filterOperator, string value)
		=> MergeFilters.Add(new FilterCondition(logic, field, filterOperator, value));

	/// <summary>
	/// マージ後の結果フィルタを追加する。
	/// </summary>
	public void AddResultFilter(IResultFilter filter)
		=> ResultFilters.Add(filter);

	/// <summary>
	/// 指定されたデータソースをマージする。
	/// </summary>
	/// <param name="sources">マージするデータソースの辞書</param>
	/// <returns>マージ後のデータテーブル</returns>
	public virtual DataTable Merge(IReadOnlyDictionary<string, IImportSource> sources)
	{
		// 引数チェック
		if (sources == null)
			throw new ArgumentNullException(nameof(sources));
		ValidateRequiredSources(sources);
		// マージ処理の実行
		var merged = MergeCore(sources);

		foreach (var filter in MergeFilters)
		{
			if (filter is FilterCondition cond)
			{
				// マージ後のフィルタ条件を構築
				var rowFilter = BuildRowPredicate(cond);
				// マージ後のフィルタ条件を適用
				merged = ApplyRowFilter(merged, rowFilter);
			}
		}

		foreach (var filter in ResultFilters)
		{
			// マージ後の結果フィルタを適用
			merged = filter.Apply(merged);
		}
		// マージ後の結果を返す
		return merged;
	}

	/// <summary>
	/// 実際のマージ処理を実装する。
	/// </summary>
	/// <param name="sources">マージするデータソースの辞書</param>
	/// <returns>マージ後のデータテーブル</returns>
	protected abstract DataTable MergeCore(IReadOnlyDictionary<string, IImportSource> sources);

	/// <summary>
	/// マージ後の列名を取得する。
	/// </summary>
	/// <returns>マージ後の列名の配列</returns>
	public virtual string[] GetMergedColumnNames() => Array.Empty<string>();

	/// <summary>
	/// マージ後の列名を取得する（ソース別）。
	/// </summary>
	/// <returns>マージ後の列名の配列</returns>
	public virtual string[] GetMergedColumnNamesBySource()
		=> GetMergedColumnNames();

	/// <summary>
	/// 必要なデータソースが揃っているかを検証する。
	/// </summary>
	/// <param name="sources">マージするデータソースの辞書</param>
	protected virtual void ValidateRequiredSources(IReadOnlyDictionary<string, IImportSource> sources)
	{
	}

	/// <summary>
	/// 指定されたキーのデータソースを取得する。
	/// </summary>
	/// <param name="sources">マージするデータソースの辞書</param>
	/// <param name="key">取得するデータソースのキー</param>
	/// <typeparam name="T">データソースの型</typeparam>
	/// <returns>指定されたキーのデータソース</returns>
	protected static T RequireSource<T>(IReadOnlyDictionary<string, IImportSource> sources, string key)
		where T : class, IImportSource
	{
		if (!sources.TryGetValue(key, out var source))
		{
			throw new InvalidOperationException($"結合するためのデータソースが不足しています。({key})");
		}
		// データソースの型をチェック
		return source as T ?? throw new InvalidOperationException($"キー '{key}' のデータソースは {typeof(T).Name} ではありません。");
	}

	/// <summary>
	/// 指定された条件に基づいて DataTable の行をフィルタリングする。
	/// </summary>
	/// <param name="table">フィルタリングするデータテーブル</param>
	/// <param name="predicate">行をフィルタリングする条件を表す述語</param>
	/// <returns>フィルタリング後のデータテーブル</returns>
	internal static DataTable ApplyRowFilter(DataTable table, Func<DataRow, bool> predicate)
	{
		// 引数チェック
		var filteredRows = table.Rows.Cast<DataRow>().Where(predicate).ToArray();
		// 新しい DataTable を作成し、フィルタリングされた行をインポート
		var clone = table.Clone();

		foreach (var row in filteredRows)
		{
			// 行をインポート
			clone.ImportRow(row);
		}
		// フィルタリング後の DataTable を返す
		return clone;
	}

	/// <summary>
	/// 指定された条件に基づいて DataRow の述語を構築する。
	/// </summary>
	/// <param name="cond">フィルタ条件</param>
	/// <returns>DataRow を評価する述語</returns>
	internal static Func<DataRow, bool> BuildRowPredicate(FilterCondition cond)
	{
		return row =>
		{
			if (row == null) return false;
			// 指定されたフィールドの値を取得
			var value = row[cond.Field]?.ToString();
			if (value == null) return false;
			// フィルタ条件に基づいて値を評価
			return cond.Operator switch
			{
				FilterOperator.Equals => value == cond.Value,
				FilterOperator.Contains => value.Contains(cond.Value),
				FilterOperator.StartsWith => value.StartsWith(cond.Value),
				FilterOperator.EndsWith => value.EndsWith(cond.Value),
				_ => value == cond.Value
			};
		};
	}
}

/// <summary>
/// マージ後の結果フィルタを表すインターフェイス。
/// </summary>
public sealed class DataTableResultFilter : IResultFilter
{
	/// <summary>
	/// マージ後の結果フィルタ条件。
	/// </summary>
	private readonly FilterCondition _condition;

	/// <summary>
	/// マージ後の結果フィルタを初期化する。
	/// </summary>
	/// <param name="condition"></param>
	public DataTableResultFilter(FilterCondition condition)
	{
		_condition = condition;
	}

	/// <summary>
	/// 指定された DataTable に対してフィルタ条件を適用する。
	/// </summary>
	public DataTable Apply(DataTable table)
	{
		return DataMerger.ApplyRowFilter(table, DataMerger.BuildRowPredicate(_condition));
	}
}

/// <summary>
/// 結合条件を表すレコード。
/// </summary>
public record JoinCondition(
	string LeftTable,   // 左側のテーブル名
	string[] LeftKeys,  // 左側の結合キー
	string RightTable,	// 右側のテーブル名
	string[] RightKeys); // 右側の結合キー

/// <summary>
/// 結合仕様を表すクラス。
/// </summary>
public class JoinSpecification
{
	/// <summary>
	/// 結合条件のリスト。
	/// </summary>
	public List<JoinCondition> Conditions { get; } = [];

	/// <summary>
	/// 指定された結合条件を追加する。
	/// </summary>
	/// <param name="leftTable">左側のテーブル名</param>
	/// <param name="leftKeys">左側の結合キー</param>
	/// <param name="rightTable">右側のテーブル名</param>
	/// <param name="rightKeys">右側の結合キー</param>
	/// <returns>結合仕様のインスタンス</returns>
	public JoinSpecification Join(string leftTable, string[] leftKeys, string rightTable, string[] rightKeys)
	{
		Conditions.Add(new JoinCondition(leftTable, leftKeys, rightTable, rightKeys));
		return this;
	}
}