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
		/// 結合データテーブル取得メソッド
		/// </summary>
		/// <returns>結合データテーブル</returns>
		public DataTable GetMergedDataTable()
		{
			try
			{
				return GetMergedDataTableCore();
			}
			catch (Exception ex)
			{
				LogError(ex);
				return new DataTable();
			}
		}

		/// <summary>
		/// Importer の GetTable() 相当の実装。Merger の最終合成結果を返す。
		/// </summary>
		protected override DataTable GetTableCore() => GetMergedDataTable();

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
		/// CSV用フィルタ述語生成
		/// </summary>
		public Func<DataRow, bool> BuildCsvFilterPredicate(string tableName)
		{
			// Shunting Yard Algorithm を使って中置記法のフィルタ条件を後置記法に変換
			var output = new List<object>();
			var ops = new Stack<string>();
			foreach (var filter in Filters)
			{
				if (filter is FilterCondition cond && cond.Field.StartsWith(tableName + "."))
				{
					output.Add(cond);
				}
				else if (filter is FilterGroupStart)
				{
					ops.Push("(");
				}
				else if (filter is FilterGroupEnd)
				{
					while (ops.Count > 0 && ops.Peek() != "(")
						output.Add(ops.Pop());
					if (ops.Count > 0) ops.Pop();
				}
				else if (filter is FilterCondition logicCond && (logicCond.Logic == "AND" || logicCond.Logic == "OR"))
				{
					while (ops.Count > 0 && Precedence(ops.Peek()) >= Precedence(logicCond.Logic))
						output.Add(ops.Pop());
					ops.Push(logicCond.Logic);
				}
			}
			while (ops.Count > 0) output.Add(ops.Pop());

			// 後置記法のフィルタ条件を評価する述語を生成
			var stack = new Stack<Func<DataRow, bool>>();
			foreach (var token in output)
			{
				// フィルタ条件の場合
				if (token is FilterCondition cond && cond.Field.StartsWith(tableName + "."))
				{
					var field = cond.Field.Substring(tableName.Length + 1);
					var value = cond.Value;
					stack.Push(row =>
					{
						if (!row.Table.Columns.Contains(field)) return false;
						var val = row.Field<string>(field);
						if (val == null) return false;
						return cond.Operator switch
						{
							FilterOperator.Contains => val.Contains(value),
							FilterOperator.StartsWith => val.StartsWith(value),
							FilterOperator.EndsWith => val.EndsWith(value),
							_ => val == value
						};
					});
				}
				// 論理演算子の場合
				else if (token is string op && (op == "AND" || op == "OR"))
				{
					var right = stack.Pop();
					var left = stack.Pop();
					if (op == "AND") stack.Push(row => left(row) && right(row));
					else stack.Push(row => left(row) || right(row));
				}
			}
			// 最終的な述語を返す（スタックが空の場合は常に true を返す述語）
			return stack.Count > 0 ? stack.Pop() : row => true;

			// フィルタ条件の優先度を返すヘルパー関数
			static int Precedence(string op) => op == "AND" ? 2 : op == "OR" ? 1 : 0;
		}

		/// <summary>
		/// 指定した目的変数・説明変数の有効な行のみを抽出したDataTableを返す
		/// </summary>
		public static DataTable? FilterValidRows(DataTable source, string responseName, List<string> predictorNames)
		{
			if (source == null) return null;
			// 目的変数と説明変数のいずれかが欠損している行を除外
			var validRows = source.AsEnumerable()
				.Where(row =>
					!row.IsNull(responseName) &&
					!string.IsNullOrWhiteSpace(row[responseName]?.ToString()) &&
					predictorNames.All(name =>
						!row.IsNull(name) &&
						!string.IsNullOrWhiteSpace(row[name]?.ToString())
					)
				).ToList();
			// 有効な行が存在する場合はそれをコピーして新しいDataTableを返す。存在しない場合は空のDataTableを返す。
			return validRows.Count > 0 ? validRows.CopyToDataTable() : source.Clone();
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

		/// <summary>
		/// SQLクエリを実行してDataTableを取得（テンプレートメソッド）
		/// </summary>
		protected virtual DataTable ExecuteMergedSqlQuery(
			string sql,
			List<DbParameter> parameters,
			Action<DbCommand>? setupCommand = null)
		{
			// データベース接続を取得して SQL クエリを実行し、結果を DataTable に変換して返す
			return ExecuteSqlQuery(sql, parameters, GetDbConnection, setupCommand);
		}

		/// <summary>
		/// LINQクエリにフィルタを適用してDataTableに変換（テンプレートメソッド）
		/// </summary>
		protected virtual DataTable ApplyFiltersAndConvertToDataTable<T>(IEnumerable<T> query)
		{
			// LINQクエリにフィルタを適用して DataTable に変換して返す
			var filteredQuery = ApplyFilters(query);
			return ConvertToDataTable(filteredQuery);
		}

		/// <summary>
		/// JOIN 仕様から SQL JOIN 句を自動生成
		/// </summary>
		protected string BuildSqlJoinClauses(JoinSpecification spec)
		{
			var clauses = new System.Text.StringBuilder();
			// JOIN 条件を順に処理して SQL JOIN 句を構築
			foreach (var cond in spec.Conditions)
			{
				_ = RequiredDBDialect.QuoteIdentifier(cond.LeftTable);
				_ = RequiredDBDialect.QuoteIdentifier(cond.RightTable);
				var leftAlias = cond.LeftTable.Length > 0 ? RequiredDBDialect.NormalizeAlias(cond.LeftTable[0].ToString()) : "l";
				var rightAlias = cond.RightTable.Length > 0 ? RequiredDBDialect.NormalizeAlias(cond.RightTable[0].ToString()) : "r";
				var joinKeys = string.Join(" AND ",
					cond.LeftKeys.Zip(cond.RightKeys,
						(lk, rk) => $"{RequiredDBDialect.QuoteIdentifier(leftAlias)}.{RequiredDBDialect.QuoteIdentifier(lk)} = {RequiredDBDialect.QuoteIdentifier(rightAlias)}.{RequiredDBDialect.QuoteIdentifier(rk)}"));
				clauses.AppendLine($"INNER JOIN {RequiredDBDialect.QuoteIdentifier(cond.RightTable)} {RequiredDBDialect.QuoteIdentifier(rightAlias)} ON {joinKeys}");
			}
			return clauses.ToString();
		}

		/// <summary>
		/// JOIN 仕様から LINQ JOIN を自動適用（CSV 側用）
		/// </summary>
		protected virtual IEnumerable<dynamic> ApplyJoinSpecification(Dictionary<string, DataTable> tableMap, JoinSpecification spec)
		{
			if (spec.Conditions.Count == 0)
			{
				yield break;
			}

			// JOIN 仕様の最初の条件を基準にして、左テーブルから順に結合を適用
			var firstCond = spec.Conditions[0];
			var leftTableName = firstCond.LeftTable.ToLower();
			if (!tableMap.ContainsKey(leftTableName))
			{
				yield break;
			}

			// 左テーブルを取得して、IEnumerable<dynamic> に変換
			var leftTable = tableMap[leftTableName];
			IEnumerable<dynamic> current = leftTable.AsEnumerable().Cast<dynamic>();

			// JOIN 条件を順に処理して LINQ JOIN を適用
			foreach (var cond in spec.Conditions)
			{
				var rightTableName = cond.RightTable.ToLower();
				if (!tableMap.ContainsKey(rightTableName)) continue;
				var rightTable = tableMap[rightTableName];
				var leftKeySelectors = cond.LeftKeys;
				var rightKeySelectors = cond.RightKeys;
				if (leftKeySelectors.Length == 1)
				{
					// 単一キーの場合と複合キーの場合で JOIN の方法を分ける
					var leftKey = leftKeySelectors[0];
					var rightKey = rightKeySelectors[0];
					current = current.Join(
						rightTable.AsEnumerable(),
						leftRow =>
						{
							try
							{
								if (leftRow is DataRow dr)
									return dr.Field<string>(leftKey) ?? "";
								return ((dynamic)leftRow).Left.Field<string>(leftKey) ?? "";
							}
							catch
							{
								return "";
							}
						},
						rightRow => rightRow.Field<string>(rightKey) ?? "",
						(left, right) => (dynamic)new { Left = left, Right = right }
					);
				}
				else
				{
					// 複合キーの場合
					current = current.Join(
						rightTable.AsEnumerable(),
						leftRow =>
						{
							try
							{
								DataRow dataRow;
								if (leftRow is DataRow dr)
									dataRow = dr;
								else
									dataRow = ((dynamic)leftRow).Left;
								var keyParts = leftKeySelectors.Select(k => dataRow.Field<string>(k) ?? "");
								return string.Join("|", keyParts);
							}
							catch
							{
								return "";
							}
						},
						rightRow =>
						{
							var keyParts = rightKeySelectors.Select(k => rightRow.Field<string>(k) ?? "");
							return string.Join("|", keyParts);
						},
						(left, right) => (dynamic)new { Left = left, Right = right }
					);
				}
			}

			foreach (var item in current)
			{
				yield return item;
			}
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

