namespace RegressionAnalysis.Common;

/// <summary>
/// フィルター要素の抽象基底クラス
/// </summary>
public abstract record FilterElement;

/// <summary>
/// フィルター条件
/// </summary>
/// <param name="Logic">条件を結合する論理演算子（ANDまたはOR）</param>
/// <param name="Field">フィールド名</param>
/// <param name="Operator">フィルター演算子</param>
/// <param name="Value">フィルター値</param>
public record FilterCondition(string Logic, string Field, FilterOperator Operator, string Value) : FilterElement;

/// <summary>
/// フィルター演算子
/// </summary>
public enum FilterOperator
{
	Equals,		// 等しい
	Contains,	// 含む	
	StartsWith, // 先頭一致
	EndsWith    // 末尾一致
}

/// <summary>
/// フィルターグループの開始
/// </summary>
/// <param name="Logic">グループ内の条件を結合する論理演算子（ANDまたはOR）</param>
public record FilterGroupStart(string Logic = "") : FilterElement;

/// <summary>
/// フィルターグループの終了
/// </summary>
public record FilterGroupEnd() : FilterElement;