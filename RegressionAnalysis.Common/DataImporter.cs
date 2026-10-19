using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace RegressionAnalysis.Common;

/// <summary>
/// 1 CSV / 1 DB テーブルをフィルタ付きで DataTable として参照する基底クラス
/// </summary>
public abstract class DataImporter
{
	/// <summary>
	/// DB方言
	/// </summary>
	private DBDialect? DBDialect { get; }

	/// <summary>
	/// DB方言（必須）
	/// </summary>
	protected DBDialect RequiredDBDialect =>
		DBDialect ?? throw new InvalidOperationException("DB方言が設定されていません。");

	/// <summary>
	/// コンストラクタ
	/// </summary>
	protected DataImporter(DBDialect? dbDialect = null)
	{
		DBDialect = dbDialect;
	}

	/// <summary>
	/// フィルタ条件リスト
	/// </summary>
	public List<FilterElement> Filters { get; } = [];

	/// <summary>
	/// 取り込みデータテーブルを取得
	/// </summary>
	public DataTable GetTable()
	{
		try
		{
			return GetTableCore();
		}
		catch (Exception ex)
		{
			LogError(ex);
			return new DataTable();
		}
	}

	/// <summary>
	/// 派生クラスで実装する取得ロジック
	/// </summary>
	protected abstract DataTable GetTableCore();

	/// <summary>
	/// 元データ側の列名一覧（CSV列名 / テーブル列名）
	/// </summary>
	public virtual string[] GetTableColumnNames() => Array.Empty<string>();

	/// <summary>
	/// 列名 -> SQL alias.column マッピング
	/// </summary>
	public virtual Dictionary<string, string> TableColumnToSqlMapping { get; } = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// フィルタ条件追加メソッド（互換用）
	/// </summary>
	public void AddFilterCondition(string logic, string field, string condition)
	{
		var parsed = ParseLegacyCondition(condition);
		Filters.Add(new FilterCondition(logic, field, parsed.Operator, parsed.Value));
	}

	/// <summary>
	/// フィルタ条件追加メソッド（推奨）
	/// </summary>
	public void AddFilterCondition(string logic, string field, FilterOperator filterOperator, string value)
		=> Filters.Add(new FilterCondition(logic, field, filterOperator, value));

	/// <summary>
	/// フィルタグループ開始追加メソッド
	/// </summary>
	public void AddGroupStart(string logic = "")
		=> Filters.Add(new FilterGroupStart(logic));

	/// <summary>
	/// フィルタグループ終了追加メソッド
	/// </summary>
	public void AddGroupEnd()
		=> Filters.Add(new FilterGroupEnd());

	/// <summary>
	/// IEnumerable にフィルタを適用（DataTable 変換なし）
	/// </summary>
	protected virtual IEnumerable<T> ApplyFilters<T>(IEnumerable<T> query)
	{
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
	/// LINQ クエリ用フィルタ述語生成
	/// </summary>
	/// <returns>フィルタ述語</returns>
	public virtual Func<object, bool> BuildLinqFilterPredicate()
	{
		// フィルタ条件を逆ポーランド記法に変換
		var output = new List<object>();
		var ops = new Stack<string>();
		foreach (var filter in Filters)
		{
			if (filter is FilterCondition cond)
			{
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

		// 逆ポーランド記法を評価して述語を構築
		var stack = new Stack<Func<object, bool>>();
		foreach (var token in output)
		{
			if (token is FilterCondition condToken)
			{
				// フィルタ条件を評価する述語を作成
				var field = condToken.Field;
				var value = condToken.Value;
				stack.Push(obj =>
				{
					if (obj == null) return false;
					string? targetValue;
					if (field.Contains('.'))
					{
						// フィールド名が "Table.Column" の形式の場合、DataRow から値を取得
						var parts = field.Split('.', 2);
						var tbl = parts[0];
						var col = parts[1];
						var prop = obj.GetType().GetProperty(tbl);
						if (prop == null) return false;
						var row = prop.GetValue(obj) as DataRow;
						if (row == null || !row.Table.Columns.Contains(col)) return false;
						targetValue = row.Field<string>(col);
					}
					else if (obj is IDictionary<string, string?> row)
					{
						// オブジェクトが IDictionary<string, string?> の場合、値を取得
						if (!row.TryGetValue(field, out targetValue)) return false;
					}
					else
					{
						// オブジェクトのプロパティから値を取得
						var p = obj.GetType().GetProperty(field);
						if (p == null) return false;
						targetValue = p.GetValue(obj)?.ToString();
					}
					if (string.IsNullOrEmpty(targetValue)) return false;
					// フィルタ条件に応じて比較
					return condToken.Operator switch
					{
						FilterOperator.Contains => targetValue.Contains(value),
						FilterOperator.StartsWith => targetValue.StartsWith(value),
						FilterOperator.EndsWith => targetValue.EndsWith(value),
						_ => targetValue == value
					};
				});
			}
			else if (token is string op && (op == "AND" || op == "OR"))
			{
				// 2つの述語を組み合わせる
				var right = stack.Pop();
				var left = stack.Pop();
				if (op == "AND") stack.Push(obj => left(obj) && right(obj));
				else stack.Push(obj => left(obj) || right(obj));
			}
		}
		// 最終的な述語を返す
		return stack.Count > 0 ? stack.Pop() : (obj => true);

		// 演算子の優先順位を返すヘルパーメソッド
		static int Precedence(string op) => op == "AND" ? 2 : op == "OR" ? 1 : 0;
	}

	/// <summary>
	/// SQL WHERE句生成メソッド
	/// </summary>
	public virtual string BuildSqlWhereClause(string tableName, out List<DbParameter> parameters)
	{
		var clauses = new List<string>();
		parameters = [];
		int paramIndex = 0;
		var map = TableColumnToSqlMapping ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		// フィルタ条件をSQL WHERE句に変換  
		foreach (var cond in Filters.OfType<FilterCondition>())
		{
			// フィールド名をSQL列名に変換
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
		// WHERE句を生成
		return clauses.Count > 0 ? ("WHERE " + string.Join(" ", clauses)) : "";
	}

	/// <summary>
	/// SQLクエリを実行してDataTableを取得
	/// </summary>
	/// <param name="sql">実行するSQLクエリ</param>
	/// <param name="parameters">SQLパラメータのリスト</param>
	/// <param name="connectionFactory">DbConnectionを生成するファクトリメソッド</param>
	/// <param name="setupCommand">DbCommandの追加設定を行うアクション</param>
	/// <returns>取得したDataTable</returns>
	protected virtual DataTable ExecuteSqlQuery(
		string sql,
		List<DbParameter> parameters,
		Func<DbConnection> connectionFactory,
		Action<DbCommand>? setupCommand = null)
	{
		using var conn = connectionFactory();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;
		foreach (var p in parameters)
			cmd.Parameters.Add(p);

		setupCommand?.Invoke(cmd);
		var factory = DbProviderFactories.GetFactory(conn);
		var adapter = factory?.CreateDataAdapter()
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
	/// エラーログ出力メソッド
	/// </summary>
	protected virtual void LogError(Exception ex)
	{
		Console.WriteLine($"[DataImporter Error] {ex.Message}");
	}

	/// <summary>
	/// 旧仕様の条件式をパースして FilterOperator と値に変換する
	/// </summary>
	/// <param name="condition"></param>
	/// <returns></returns>
	private static (FilterOperator Operator, string Value) ParseLegacyCondition(string condition)
	{
		var condTrim = condition.Trim();
		if (condTrim.StartsWith("LIKE", StringComparison.OrdinalIgnoreCase))
		{
			// LIKE 条件をパースして FilterOperator と値に変換
			var pattern = condTrim.Substring(4).Trim().Trim('\'', '"');
			var startsWithWildcard = pattern.StartsWith("%");
			var endsWithWildcard = pattern.EndsWith("%");
			var core = pattern.Trim('%');

			// LIKE 条件のパターンに応じて FilterOperator を決定
			if (startsWithWildcard && endsWithWildcard) return (FilterOperator.Contains, core);
			if (startsWithWildcard) return (FilterOperator.EndsWith, core);
			if (endsWithWildcard) return (FilterOperator.StartsWith, core);
			return (FilterOperator.Equals, core);
		}
		// デフォルトは Equals 条件として扱う
		return (FilterOperator.Equals, condTrim.Trim('=', '\'', '"'));
	}
}

/// <summary>
/// CSV 1ファイル取得用Importer
/// </summary>
public class CSVImporter(string csvPath) : DataImporter
{
	/// <summary>
	/// テーブル列名一覧を取得
	/// </summary>
	/// <returns></returns>
	public override string[] GetTableColumnNames()
	{
		var table = CsvHelpers.ReadCsv(csvPath);
		return table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
	}

	/// <summary>
	/// テーブル列名 -> SQL alias.column マッピングを取得
	/// </summary>
	/// <returns></returns>
	protected override DataTable GetTableCore()
	{
		var table = CsvHelpers.ReadCsv(csvPath);
		if (Filters.Count == 0)
		{
			return table;
		}

		// DataTable を IEnumerable<Dictionary<string, string?>> に変換してフィルタを適用
		var projected = table.AsEnumerable().Select(row =>
		{
			var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
			foreach (DataColumn column in table.Columns)
			{
				dict[column.ColumnName] = row[column]?.ToString();
			}
			return dict;
		});

		var filtered = ApplyFilters(projected).ToList();
		var result = table.Clone();
		// フィルタ結果を DataTable に反映
		foreach (var item in filtered)
		{
			// 新しい行を作成して値を設定
			var newRow = result.NewRow();
			foreach (DataColumn column in result.Columns)
			{
				newRow[column.ColumnName] = item.TryGetValue(column.ColumnName, out var value) && value != null
					? value
					: DBNull.Value;
			}
			result.Rows.Add(newRow);
		}

		return result;
	}
}

/// <summary>
/// DB 1テーブル取得用Importer
/// </summary>
public class DBImporter(
	string tableName,
	DBDialect? dialect = null,
	Func<DbConnection>? connectionFactory = null) : DataImporter(dialect ?? new MySqlDialect())
{
	/// <summary>
	/// 接続ファクトリ（デフォルトは MySQL 用）
	/// </summary>
	private readonly Func<DbConnection> _connectionFactory = connectionFactory ?? MySqlConnectionFactory.CreateOpenConnection;

	/// <summary>
	/// テーブル列名一覧を取得
	/// </summary>
	/// <returns></returns>
	public override string[] GetTableColumnNames()
	{
		string sql = $"SELECT * FROM {RequiredDBDialect.QuoteIdentifier(tableName)} t LIMIT 0";
		var table = ExecuteSqlQuery(sql, [], _connectionFactory);
		return table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
	}

	/// <summary>
	/// テーブル列名 -> SQL alias.column マッピングを取得
	/// </summary>
	public override Dictionary<string, string> TableColumnToSqlMapping
	{
		get
		{
			// 列名一覧を取得してマッピングを作成
			var names = GetTableColumnNames();
			var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (var name in names)
			{
				// 列名を SQL のエイリアス付きでマッピング
				map[name] = $"t.{name}";
			}
			return map;
		}
	}

	/// <summary>
	/// DBテーブルからデータを取得して DataTable として返す
	/// </summary>
	/// <returns></returns>
	protected override DataTable GetTableCore()
	{
		var whereSql = BuildSqlWhereClause("t", out var parameters);
		string sql = $"SELECT * FROM {RequiredDBDialect.QuoteIdentifier(tableName)} t {whereSql}";
		return ExecuteSqlQuery(sql, parameters, _connectionFactory);
	}
}
