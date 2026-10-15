using System;
using System.Data.Common;
using System.Text.RegularExpressions;
using MySqlConnector;

namespace RegressionAnalysis.Common;

/// <summary>
/// DB 方言ごとの識別子処理を定義する抽象基底クラス
/// </summary>
public abstract class DBDialect
{
	/// <summary>
	/// 安全な識別子を検証する正規表現
	/// </summary>
	private static readonly Regex SafeIdentifierRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

	/// <summary>
	/// 識別子の開始クオート文字列
	/// </summary>
	protected abstract string IdentifierQuoteStart { get; }

	/// <summary>
	/// 識別子の終了クオート文字列
	/// </summary>
	protected abstract string IdentifierQuoteEnd { get; }

	/// <summary>
	/// 識別子を検証してクオートする
	/// </summary>
	public string QuoteIdentifier(string identifier)
	{
		if (string.IsNullOrWhiteSpace(identifier) || !SafeIdentifierRegex.IsMatch(identifier))
			throw new InvalidOperationException($"不正な識別子です: {identifier}");

		return $"{IdentifierQuoteStart}{identifier}{IdentifierQuoteEnd}";
	}

	/// <summary>
	/// DB パラメータを生成
	/// </summary>
	/// <param name="name">パラメータ名</param>
	/// <param name="value">パラメータ値</param>
	/// <returns>生成された DB パラメータ</returns>
	public abstract DbParameter CreateParameter(string name, object value);

	/// <summary>
	/// テーブルエイリアスの正規化
	/// </summary>
	public virtual string NormalizeAlias(string alias) => alias.ToLowerInvariant(); // カルチャ環境に依存しない正規化
}

/// <summary>
/// MySQL 向け SQL 方言
/// </summary>
public sealed class MySqlDialect : DBDialect
{
	/// <summary>
	/// 識別子の開始クオート文字列
	/// </summary>
	protected override string IdentifierQuoteStart => "`";
	/// <summary>
	/// 識別子の終了クオート文字列
	/// </summary>
	protected override string IdentifierQuoteEnd => "`";
	/// <summary>
	/// MySQL パラメータを生成
	/// </summary>
	/// <param name="name">パラメータ名</param>
	/// <param name="value">パラメータ値</param>
	/// <returns>生成された MySqlParameter</returns>
	public override DbParameter CreateParameter(string name, object value) => new MySqlParameter(name, value);
}
