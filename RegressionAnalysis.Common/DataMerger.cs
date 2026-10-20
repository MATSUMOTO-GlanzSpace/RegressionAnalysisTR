using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegressionAnalysis.Common
{
	/// <summary>
	/// データ結合基底クラス
	/// </summary>
	public abstract class DataMerger : DataImporter
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="dbDialect">DB方言</param>
		protected DataMerger(DBDialect? dbDialect = null) : base(dbDialect)
		{
		}

		/// <summary>
		/// Importer の GetTable() 相当の実装。Merger の最終合成結果を返す。
		/// </summary>
		protected override DataTable GetTableCore() => GetMergedDataTableCore();

		/// <summary>
		/// Importer の GetTableColumnNames() 相当の実装。Merger の合成後の列名一覧を返す。
		/// </summary>
		public override string[] GetTableColumnNames() => GetMergedDataTableColumnNames();

		/// <summary>
		/// 派生クラスで実装する結合ロジック
		/// </summary>
		/// <returns>結合データテーブル</returns>
		protected abstract DataTable GetMergedDataTableCore();

		/// <summary>
		/// データベース接続を取得（派生クラスで実装）
		/// DB プッシュダウン型マージャーは実装、CSV型は例外をスロー
		/// </summary>
		/// <returns>DbConnection インスタンス</returns>
		protected virtual DbConnection GetDbConnection()
		{
			throw new NotSupportedException("このマージャーはデータベース接続をサポートしていません。");
		}

		/// <summary>
		/// クエリ結果を DataTable に変換（リフレクションで匿名型を統一列名にマッピング）
		/// GetMergedDataTableColumnNames() が定義されている前提
		/// </summary>
		/// <typeparam name="T">クエリ結果の型（通常は匿名型）</typeparam>
		/// <param name="data">変換対象のデータ</param>
		/// <returns>変換後の DataTable</returns>
		protected virtual DataTable ConvertToDataTable<T>(IEnumerable<T> data)
		{
			// 結合後の列名を取得
			DataTable mergedTable = new();
			foreach (var colName in GetMergedDataTableColumnNames())
			{
				mergedTable.Columns.Add(colName);
			}

			// データを DataTable に変換
			foreach (var row in data)
			{
				if (row == null) continue;
				var colNames = GetMergedDataTableColumnNames();
				var type = row.GetType();
				var values = colNames.Select(c =>
				{
					var prop = type.GetProperty(c);
					if (prop == null) return DBNull.Value;
					var value = prop.GetValue(row);
					return value ?? DBNull.Value;
				}).ToArray();
				mergedTable.Rows.Add(values);
			}
			return mergedTable;
		}

		/// <summary>
		/// マージ後のDataTableの列名一覧を返す（派生クラスでオーバーライド）
		/// </summary>
		public virtual string[] GetMergedDataTableColumnNames() => Array.Empty<string>();

		/// <summary>
		/// マージ後の列名 -> DB 側の alias.column へのマッピング
		/// </summary>
		public virtual Dictionary<string, string> MergedColumnToSqlMapping { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// SQL WHERE 句を構築する（派生クラスでオーバーライド）
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public override string BuildSqlWhereClause(string tableName, out List<DbParameter> parameters)
		{
			var clauses = new List<string>();
			parameters = [];
			int paramIndex = 0;
			// MergedColumnToSqlMapping が null の場合は空の辞書を使用
			var map = MergedColumnToSqlMapping ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			// フィルタ条件を処理
			foreach (var cond in Filters.OfType<FilterCondition>())
			{
				// フィルタ条件のフィールドが tableName で始まる場合のみ処理
				string? left = null;
				if (cond.Field.Contains('.'))
				{
					var parts = cond.Field.Split('.', 2);
					var tbl = parts[0];
					var col = parts[1];
					_ = RequiredDBDialect.QuoteIdentifier(tbl);
					var alias = tbl.Length > 0 ? RequiredDBDialect.NormalizeAlias(tbl.Substring(0, 1)) : tbl;
					left = $"{RequiredDBDialect.QuoteIdentifier(alias)}.{RequiredDBDialect.QuoteIdentifier(col)}";
				}
				// MergedColumnToSqlMapping にマッピングが存在する場合はそれを使用
				else if (map.TryGetValue(cond.Field, out var mapped))
				{
					left = mapped;
				}
				else
				{
					continue;
				}

				if (string.IsNullOrEmpty(left)) continue;
				// パラメータ名を生成
				var paramName = "@p" + paramIndex;
				var value = cond.Value;
				// フィルタ条件の演算子に応じて SQL 句を構築
				switch (cond.Operator)
				{
					case FilterOperator.Contains:
						clauses.Add($"{(string.IsNullOrEmpty(cond.Logic) ? "" : cond.Logic + " ")}{left} LIKE {paramName}");
						parameters.Add(RequiredDBDialect.CreateParameter(paramName, $"%{value}%"));
						break;
					case FilterOperator.StartsWith:
						clauses.Add($"{(string.IsNullOrEmpty(cond.Logic) ? "" : cond.Logic + " ")}{left} LIKE {paramName}");
						parameters.Add(RequiredDBDialect.CreateParameter(paramName, $"{value}%"));
						break;
					case FilterOperator.EndsWith:
						clauses.Add($"{(string.IsNullOrEmpty(cond.Logic) ? "" : cond.Logic + " ")}{left} LIKE {paramName}");
						parameters.Add(RequiredDBDialect.CreateParameter(paramName, $"%{value}"));
						break;
					default:
						clauses.Add($"{(string.IsNullOrEmpty(cond.Logic) ? "" : cond.Logic + " ")}{left} = {paramName}");
						parameters.Add(RequiredDBDialect.CreateParameter(paramName, value));
						break;
				}
				paramIndex++;
			}
			// WHERE 句を返す（条件がない場合は空文字列を返す）
			return clauses.Count > 0 ? ("WHERE " + string.Join(" ", clauses)) : "";
		}

	}

	/// <summary>
	/// フィルタ要素基底レコード
	/// </summary>
	public abstract record FilterElement;

	/// <summary>
	/// フィルタ条件レコード
	/// </summary>
	public record FilterCondition(string Logic, string Field, FilterOperator Operator, string Value) : FilterElement;

	/// <summary>
	/// フィルタ演算子
	/// </summary>
	public enum FilterOperator
	{
		Equals,     // 完全一致
		Contains,   // 部分一致
		StartsWith, // 前方一致
		EndsWith    // 後方一致
	}

	/// <summary>
	/// フィルタグループ開始レコード"("
	/// </summary>
	public record FilterGroupStart(string Logic = "") : FilterElement;

	/// <summary>
	/// フィルタグループ終了レコード")"
	/// </summary>
	public record FilterGroupEnd() : FilterElement;
}

/// <summary>
/// JOIN 条件（複数テーブルの結合キーを定義）
/// </summary>
public record JoinCondition(
	string LeftTable,   // 左テーブル名
	string[] LeftKeys,  // 左テーブルの結合キー列名
	string RightTable,  // 右テーブル名
	string[] RightKeys);// 右テーブルの結合キー列名

/// <summary>
/// JOIN 仕様（複数の JOIN 条件を保持）
/// </summary>
public class JoinSpecification
{
	// JOIN 条件のリスト
	public List<JoinCondition> Conditions { get; } = [];

	/// <summary>
	/// 結合条件を追加するメソッド
	/// </summary>
	/// <param name="leftTable"></param>
	/// <param name="leftKeys"></param>
	/// <param name="rightTable"></param>
	/// <param name="rightKeys"></param>
	/// <returns></returns>
	public JoinSpecification Join(string leftTable, string[] leftKeys, string rightTable, string[] rightKeys)
	{
		Conditions.Add(new JoinCondition(leftTable, leftKeys, rightTable, rightKeys));
		return this;
	}
}

