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
	public abstract class DataMerger
	{
		/// <summary>
		/// 結合データテーブル取得メソッド
		/// </summary>
		/// <returns>結合データテーブル</returns>
		public DataTable GetMergedDataTable()
		{
			try
			{
				// 派生クラスの実装を呼び出し
				return GetMergedDataTableCore();
			}
			catch (Exception ex)
			{
				// エラーログ出力
				LogError(ex);
				// 空のDataTableを返す
				return new DataTable();
					}
			}

			/// <summary>
			/// IEnumerable にフィルタを適用（DataTable 変換なし）
			/// CSV 結合時など、フィルタ後の DataTable 生成が派生側で異なる場合に使用
			/// </summary>
			/// <typeparam name="T">クエリ結果の型</typeparam>
			/// <param name="query">フィルタ対象のクエリ結果</param>
			/// <returns>フィルタ適用後のクエリ</returns>
			protected virtual IEnumerable<T> ApplyFilters<T>(IEnumerable<T> query)
			{
				// フィルタを適用
				if (Filters.Count > 0)
				{
					var predicate = BuildLinqFilterPredicate();
					if (predicate != null)
					{
						query = query.Where(x => x != null && predicate(x)).ToList();
					}
				}

				return query;
			}

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
			// データテーブルに列を追加（統一列名を使用）
			DataTable mergedTable = new();
			foreach (var colName in GetMergedDataTableColumnNames())
			{
				mergedTable.Columns.Add(colName);
			}

			// 結合データを DataTable に追加
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
		/// エラーログ出力メソッド
		/// </summary>
		/// <param name="ex">例外オブジェクト</param>
		protected virtual void LogError(Exception ex)
		{
			// 標準出力にエラーログ
			Console.WriteLine($"[DataMerger Error] {ex.Message}");
		}

		/// <summary>
		/// フィルタ条件リスト
		/// </summary>
		public List<FilterElement> Filters { get; } = [];

		/// <summary>
		/// フィルタ条件追加メソッド
		/// </summary>
		/// <param name="logic">論理演算子（AND/OR）</param>
		/// <param name="field">フィールド名（テーブル名.フィールド名形式）</param>
		/// <param name="condition">条件式（例: "='value'"）</param>
		public void AddFilterCondition(string logic, string field, string condition)
			=> Filters.Add(new FilterCondition(logic, field, condition));

		/// <summary>
		/// フィルタグループ開始追加メソッド
		/// </summary>
		/// <param name="logic">論理演算子（AND/OR）</param>
		public void AddGroupStart(string logic = "")
			=> Filters.Add(new FilterGroupStart(logic));

		/// <summary>
		/// フィルタグループ終了追加メソッド
		/// </summary>
		public void AddGroupEnd()
			=> Filters.Add(new FilterGroupEnd());

		/// <summary>
		/// CSV用フィルタ述語生成
		/// </summary>
		/// <param name="tableName">テーブル名</param>
		/// <returns>フィルタ述語</returns>
		/// <remarks>Shunting Yard AlgorithmでRPN変換し評価</remarks>
		/// <see href="https://en.wikipedia.org/wiki/Shunting_yard_algorithm" />
		public Func<DataRow, bool> BuildCsvFilterPredicate(string tableName)
		{
			// Shunting Yard AlgorithmでRPN変換
			// RPN: Reverse Polish Notation（逆ポーランド記法）
			// 出力キューと演算子スタック
			var output = new List<object>();
			var ops = new Stack<string>();
			foreach (var filter in Filters)
			{
				if (filter is FilterCondition cond && cond.Field.StartsWith(tableName + "."))
				{
					// フィルタ条件を出力キューに追加
					output.Add(cond);
				}
				else if (filter is FilterGroupStart)
				{
					// グループ開始を演算子スタックに追加
					ops.Push("(");
				}
				else if (filter is FilterGroupEnd)
				{
					// グループ終了まで演算子を出力キューに移動
					while (ops.Count > 0 && ops.Peek() != "(")
						output.Add(ops.Pop());
					if (ops.Count > 0) ops.Pop(); // remove "("
				}
				else if (filter is FilterCondition logicCond && (logicCond.Logic == "AND" || logicCond.Logic == "OR"))
				{
					// 論理演算子の優先度に基づき演算子スタックから出力キューに移動
					while (ops.Count > 0 && Precedence(ops.Peek()) >= Precedence(logicCond.Logic))
						output.Add(ops.Pop());
					ops.Push(logicCond.Logic);
				}
			}
			// 残りの演算子を出力キューに移動
			while (ops.Count > 0) output.Add(ops.Pop());

			// RPN評価
			var stack = new Stack<Func<DataRow, bool>>();
			foreach (var token in output)
			{
				if (token is FilterCondition cond && cond.Field.StartsWith(tableName + "."))
				{
					// フィルタ条件を述語に変換
					var field = cond.Field.Substring(tableName.Length + 1);
					var condTrim = cond.Condition.Trim();
					// LIKE 演算子に対応
					if (condTrim.StartsWith("LIKE", StringComparison.OrdinalIgnoreCase))
					{
						var pattern = condTrim.Substring(4).Trim().Trim('\'', '"');
						var startsWithWildcard = pattern.StartsWith("%");
						var endsWithWildcard = pattern.EndsWith("%");
						var core = pattern.Trim('%');
						stack.Push(row =>
						{
							if (!row.Table.Columns.Contains(field)) return false;
							var val = row.Field<string>(field);
							if (val == null) return false;
							if (startsWithWildcard && endsWithWildcard) return val.Contains(core);
							if (startsWithWildcard) return val.EndsWith(core);
							if (endsWithWildcard) return val.StartsWith(core);
							return val == core;
						});
					}
					else
					{
						var value = condTrim.Trim('=', '\'', '"');
						stack.Push(row =>
						{
							if (!row.Table.Columns.Contains(field)) return false;
							return row.Field<string>(field) == value;
						});
					}
				}
				else if (token is string op && (op == "AND" || op == "OR"))
				{
					// 論理演算子適用
					var right = stack.Pop();
					var left = stack.Pop();
					if (op == "AND") stack.Push(row => left(row) && right(row));
					else stack.Push(row => left(row) || right(row));
				}
			}
			return stack.Count > 0 ? stack.Pop() : row => true;

			// 演算子の優先度
			static int Precedence(string op) => op == "AND" ? 2 : op == "OR" ? 1 : 0;
		}

		/// <summary>
		/// 指定した目的変数・説明変数の有効な行のみを抽出したDataTableを返す
		/// </summary>
		/// <param name="source">元のDataTable</param>
		/// <param name="responseName">目的変数名</param>
		/// <param name="predictorNames">説明変数名リスト</param>
		/// <returns>有効な行のみのDataTable</returns>
		public static DataTable? FilterValidRows(DataTable source, string responseName, List<string> predictorNames)
		{
			if (source == null) return null;
			var validRows = source.AsEnumerable()
				.Where(row =>
					!row.IsNull(responseName) &&
					!string.IsNullOrWhiteSpace(row[responseName]?.ToString()) &&
					predictorNames.All(name =>
						!row.IsNull(name) &&
						!string.IsNullOrWhiteSpace(row[name]?.ToString())
					)
				).ToList();
			// 有効な行のみのDataTableを返す
			// 行がない場合はデータが空の（カラム情報は設定された）DataTableを返す
			return validRows.Count > 0 ? validRows.CopyToDataTable() : source.Clone();
		}

		/// <summary>
		/// マージ後のDataTableの列名一覧を返す（派生クラスでオーバーライド）
		/// </summary>
		/// <returns>列名配列</returns>
		public virtual string[] GetMergedDataTableColumnNames() => Array.Empty<string>();

		/// <summary>
		/// マージ後の列名 -> DB 側の alias.column へのマッピング
		/// DB プッシュダウンを行うマージャーはここをオーバーライドしてマッピングを提供する
		/// </summary>
		public virtual Dictionary<string, string> MergedColumnToSqlMapping { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// SQL WHERE句生成メソッド（ジェネリック版）
		/// </summary>
		/// <typeparam name="TParameter">パラメータ型（DbParameter 派生）</typeparam>
		/// <param name="tableName">テーブル別名</param>
		/// <param name="parameterFactory">パラメータ生成ファクトリ関数</param>
		/// <param name="parameters">生成されたパラメータリスト</param>
		/// <returns>WHERE句文字列（例："WHERE s.department = @p0 AND ..."）</returns>
		public virtual string BuildSqlWhereClause<TParameter>(
			string tableName,
			Func<string, object, TParameter> parameterFactory,
			out List<TParameter> parameters)
			where TParameter : DbParameter
		{
			var clauses = new List<string>();
			parameters = new List<TParameter>();
			int paramIndex = 0;
			var map = MergedColumnToSqlMapping ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (var cond in Filters.OfType<FilterCondition>())
			{
				string? left = null;
				if (cond.Field.Contains('.'))
				{
					// table.field 形式 -> alias.field
					var parts = cond.Field.Split('.', 2);
					var tbl = parts[0];
					var col = parts[1];
					var alias = tbl.Length > 0 ? tbl.Substring(0, 1) : tbl;
					left = $"{alias}.{col}";
				}
				else if (map.TryGetValue(cond.Field, out var mapped))
				{
					left = mapped;
				}
				else
				{
					// 未知のフィールドは無視
					continue;
				}

				// left が null でないことを確認
				if (string.IsNullOrEmpty(left)) continue;

				var paramName = "@p" + paramIndex;
				var condTrim = cond.Condition.Trim();
				if (condTrim.StartsWith("LIKE", StringComparison.OrdinalIgnoreCase))
				{
					clauses.Add($"{(string.IsNullOrEmpty(cond.Logic) ? "" : cond.Logic + " ")}{left} LIKE {paramName}");
					var pattern = condTrim.Substring(4).Trim().Trim('\'', '"');
					parameters.Add(parameterFactory(paramName, pattern));
				}
				else
				{
					clauses.Add($"{(string.IsNullOrEmpty(cond.Logic) ? "" : cond.Logic + " ")}{left} = {paramName}");
					parameters.Add(parameterFactory(paramName, condTrim.Trim('=', '\'', '"')));
				}
				paramIndex++;
			}
			return clauses.Count > 0 ? ("WHERE " + string.Join(" ", clauses)) : "";
		}

		/// <summary>
		/// LINQ クエリ用フィルタ述語生成（匿名オブジェクト対応）
		/// </summary>
		/// <remarks>
		/// Shunting Yard Algorithm で RPN 変換し、匿名オブジェクトに対する述語を生成します。
		/// 複数のテーブルから結合された匿名オブジェクト（sales, weather, unit など のプロパティを持つ）に対応します。
		/// </remarks>
		public virtual Func<object, bool> BuildLinqFilterPredicate()
		{
			// Shunting Yard Algorithm で RPN 変換
			var output = new List<object>();
			var ops = new Stack<string>();
			foreach (var filter in Filters)
			{
				if (filter is FilterCondition cond)
				{
					// 論理演算子処理
					if (cond.Logic == "AND" || cond.Logic == "OR")
					{
						while (ops.Count > 0 && Precedence(ops.Peek()) >= Precedence(cond.Logic))
							output.Add(ops.Pop());
						ops.Push(cond.Logic);
					}
					else
					{
						output.Add(cond);
					}
				}
				else if (filter is FilterGroupStart)
				{
					ops.Push("(");
				}
				else if (filter is FilterGroupEnd)
				{
					while (ops.Count > 0 && ops.Peek() != "(") output.Add(ops.Pop());
					if (ops.Count > 0) ops.Pop();
				}
			}
			while (ops.Count > 0) output.Add(ops.Pop());

			// RPN 評価で述語を作成
			var stack = new Stack<Func<object, bool>>();
			foreach (var token in output)
			{
				if (token is FilterCondition condToken)
				{
					var field = condToken.Field;
					var condTrim = condToken.Condition.Trim();
					if (condTrim.StartsWith("LIKE", StringComparison.OrdinalIgnoreCase))
					{
						var pattern = condTrim.Substring(4).Trim().Trim('\'', '"');
						var startsWithWildcard = pattern.StartsWith("%");
						var endsWithWildcard = pattern.EndsWith("%");
						var core = pattern.Trim('%');
						stack.Push(obj =>
						{
							if (obj == null) return false;
							// フィールド参照
							if (field.Contains('.'))
							{
								var parts = field.Split('.', 2);
								var tbl = parts[0];
								var col = parts[1];
								// sales/weather/unit の DataRow プロパティを経由
								var prop = obj.GetType().GetProperty(tbl);
								if (prop == null) return false;
								var row = prop.GetValue(obj) as DataRow;
								if (row == null) return false;
								var val = row.Table.Columns.Contains(col) ? row.Field<string>(col) : null;
								if (string.IsNullOrEmpty(val)) return false;
								if (startsWithWildcard && endsWithWildcard) return val.Contains(core);
								if (startsWithWildcard) return val.EndsWith(core);
								if (endsWithWildcard) return val.StartsWith(core);
								return val == core;
							}
							else
							{
								var p = obj.GetType().GetProperty(field);
								if (p == null) return false;
								var v = p.GetValue(obj)?.ToString();
								if (v == null) return false;
								if (startsWithWildcard && endsWithWildcard) return v.Contains(core);
								if (startsWithWildcard) return v.EndsWith(core);
								if (endsWithWildcard) return v.StartsWith(core);
								return v == core;
							}
						});
					}
					else
					{
						var value = condTrim.Trim('=', '\'', '"');
						stack.Push(obj =>
						{
							if (obj == null) return false;
							if (field.Contains('.'))
							{
								var parts = field.Split('.', 2);
								var tbl = parts[0];
								var col = parts[1];
								var prop = obj.GetType().GetProperty(tbl);
								if (prop == null) return false;
								var row = prop.GetValue(obj) as DataRow;
								if (row == null) return false;
								if (!row.Table.Columns.Contains(col)) return false;
								var rowValue = row.Field<string>(col);
								if (string.IsNullOrEmpty(rowValue)) return false;
								return rowValue == value;
							}
							else
							{
								var p = obj.GetType().GetProperty(field);
								if (p == null) return false;
								var pValue = p.GetValue(obj)?.ToString();
								if (string.IsNullOrEmpty(pValue)) return false;
								return pValue == value;
							}
						});
					}
				}
				else if (token is string op && (op == "AND" || op == "OR"))
				{
					var right = stack.Pop();
					var left = stack.Pop();
					if (op == "AND") stack.Push(obj => left(obj) && right(obj));
					else stack.Push(obj => left(obj) || right(obj));
				}
			}
			return stack.Count > 0 ? stack.Pop() : (obj => true);

			// 演算子の優先度
			static int Precedence(string op) => op == "AND" ? 2 : op == "OR" ? 1 : 0;
		}

		/// <summary>
		/// SQLクエリを実行してDataTableを取得（テンプレートメソッド）
		/// </summary>
		/// <typeparam name="TParameter">DbParameter 派生型</typeparam>
		/// <param name="sql">実行するSQL文</param>
		/// <param name="parameters">パラメータリスト</param>
		/// <param name="setupCommand">追加のコマンド設定（オプション）</param>
		/// <returns>クエリ実行結果の DataTable</returns>
		protected virtual DataTable ExecuteMergedSqlQuery<TParameter>(
			string sql,
			List<TParameter> parameters,
			Action<DbCommand>? setupCommand = null)
			where TParameter : DbParameter
		{
			using var conn = GetDbConnection();
			using var cmd = conn.CreateCommand();
			cmd.CommandText = sql;
			foreach (var p in parameters)
				cmd.Parameters.Add(p);

					setupCommand?.Invoke(cmd);

					var factory = DbProviderFactories.GetFactory(conn);
					var adapter = factory.CreateDataAdapter() 
						?? throw new InvalidOperationException("DataAdapter の作成に失敗しました。");

					try
					{
						adapter.SelectCommand = cmd;
						var table = new DataTable();
						adapter.Fill(table);
						return table;
					}
					finally
					{
						adapter.Dispose();
					}
		}

		/// <summary>
		/// LINQクエリにフィルタを適用してDataTableに変換（テンプレートメソッド）
		/// </summary>
		/// <typeparam name="T">クエリ結果の型</typeparam>
		/// <param name="query">対象のクエリ結果</param>
		/// <returns>変換後の DataTable</returns>
			protected virtual DataTable ApplyFiltersAndConvertToDataTable<T>(IEnumerable<T> query)
			{
				// フィルタを適用（ApplyFilters へ委譲）
				var filteredQuery = ApplyFilters(query);

				// DataTable に変換
				return ConvertToDataTable(filteredQuery);
			}

				/// <summary>
				/// JOIN 仕様から SQL JOIN 句を自動生成
				/// 複数キー対応、複数 JOIN 対応
				/// </summary>
				protected string BuildSqlJoinClauses(JoinSpecification spec)
				{
					var clauses = new System.Text.StringBuilder();
					foreach (var cond in spec.Conditions)
					{
						// テーブルエイリアスのマッピング
						var leftAlias = cond.LeftTable.Length > 0 ? cond.LeftTable[0].ToString().ToLower() : "l";
						var rightAlias = cond.RightTable.Length > 0 ? cond.RightTable[0].ToString().ToLower() : "r";

						// キーの結合条件を生成（複数キー対応）
						var joinKeys = string.Join(" AND ", 
							cond.LeftKeys.Zip(cond.RightKeys, 
								(lk, rk) => $"{leftAlias}.{lk} = {rightAlias}.{rk}"));

						clauses.AppendLine($"INNER JOIN {{{cond.RightTable}}} {rightAlias} ON {joinKeys}");
					}
					return clauses.ToString();
				}

				/// <summary>
				/// JOIN 仕様から LINQ JOIN を自動適用（CSV 側用）
				/// 複数 DataTable を JoinSpecification に基づいて逐次 JOIN
				/// </summary>
				/// <param name="tableMap">テーブル名 -> DataTable のマップ</param>
				/// <param name="spec">JOIN 仕様</param>
				/// <returns>JOIN 結果の IEnumerable（各要素は { Left = ..., Right = ... } 形式）</returns>
				protected virtual IEnumerable<dynamic> ApplyJoinSpecification(
					Dictionary<string, DataTable> tableMap, 
					JoinSpecification spec)
				{
					if (spec.Conditions.Count == 0)
					{
						// JOIN が指定されない場合は yield break
						yield break;
					}

					// 最初の JOIN 条件から左テーブルを取得
					var firstCond = spec.Conditions[0];
					var leftTableName = firstCond.LeftTable.ToLower();

					if (!tableMap.ContainsKey(leftTableName))
					{
						yield break;
					}

					var leftTable = tableMap[leftTableName];
					IEnumerable<dynamic> current = leftTable.AsEnumerable().Cast<dynamic>();

					// 各 JOIN 条件を逐次適用
					foreach (var cond in spec.Conditions)
					{
						var rightTableName = cond.RightTable.ToLower();
						if (!tableMap.ContainsKey(rightTableName))
						{
							continue;
						}

						var rightTable = tableMap[rightTableName];
						var leftKeySelectors = cond.LeftKeys;
						var rightKeySelectors = cond.RightKeys;

						if (leftKeySelectors.Length == 1)
						{
							// 単一キー JOIN
							var leftKey = leftKeySelectors[0];
							var rightKey = rightKeySelectors[0];

							current = current.Join(
								rightTable.AsEnumerable(),
								leftRow =>
								{
									try
									{
										// leftRow が DataRow の場合と dynamic 匿名型の場合に対応
										if (leftRow is DataRow dr)
											return dr.Field<string>(leftKey)?? "";
										else
											return ((dynamic)leftRow).Left.Field<string>(leftKey) ?? "";
									}
									catch
									{
										return "";
									}
								},
								rightRow => rightRow.Field<string>(rightKey) ?? "",
								(left, right) => (dynamic)new { Left = left, Right = right });
						}
						else
						{
							// 複数キー JOIN（文字列結合）
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
								(left, right) => (dynamic)new { Left = left, Right = right });
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
	/// <param name="Logic">論理演算子（AND/OR）</param>
	/// <param name="Field">フィールド名（テーブル名.フィールド名形式）</param>
	/// <param name="Condition">条件式（例: "='value'"）</param>
	public record FilterCondition(string Logic, string Field, string Condition) : FilterElement;

	/// <summary>
	/// フィルタグループ開始レコード"("
	/// </summary>
	/// <param name="Logic">論理演算子（AND/OR）</param>
	public record FilterGroupStart(string Logic = "") : FilterElement;

	/// <summary>
	/// フィルタグループ終了レコード")"
	/// </summary>
	public record FilterGroupEnd() : FilterElement;
}

/// <summary>
/// JOIN 条件（複数テーブルの結合キーを定義）
/// CSV、MySQL、その他データソース共通で使用可能
/// </summary>
public record JoinCondition(
	string LeftTable,
	string[] LeftKeys,
	string RightTable,
	string[] RightKeys);

/// <summary>
/// JOIN 仕様（複数の JOIN 条件を保持）
/// JSON、YAML などのメタデータから自動生成も可能
/// </summary>
public class JoinSpecification
{
	public List<JoinCondition> Conditions { get; } = [];

	/// <summary>
	/// JOIN 条件をビルダーパターンで追加
	/// </summary>
	public JoinSpecification Join(
		string leftTable, string[] leftKeys,
		string rightTable, string[] rightKeys)
	{
		Conditions.Add(new JoinCondition(leftTable, leftKeys, rightTable, rightKeys));
		return this;
	}
}

