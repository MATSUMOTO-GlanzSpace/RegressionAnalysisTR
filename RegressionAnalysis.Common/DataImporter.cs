using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace RegressionAnalysis.Common;

/// <summary>
/// データインポーターの抽象クラス。CSVファイルやデータベースなど、さまざまなデータソースからデータをインポートするための基底クラスです。
/// </summary>
public abstract class DataImporter : IImportSource
{
	/// <summary>
	/// データベース方言
	/// </summary>
	private DBDialect? DBDialect { get; }

	/// <summary>
	/// インポーターの名前
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// 必須のデータベース方言
	/// </summary>
	protected DBDialect RequiredDBDialect =>
		DBDialect ?? throw new InvalidOperationException("DB方言が設定されていません。");

	/// <summary>
	/// コンストラクター
	/// </summary>
	/// <param name="name">インポーターの名前。指定がない場合はクラス名を使用。</param>
	/// <param name="dbDialect">データベース方言。指定がない場合は null を使用。</param>
	protected DataImporter(string? name = null, DBDialect? dbDialect = null)
	{
		// インポーターの名前を設定。指定がない場合はクラス名を使用。
		Name = name ?? GetType().Name;
		// データベース方言を設定。
		DBDialect = dbDialect;
	}

	/// <summary>
	/// フィルター条件のリスト
	/// </summary>
	public List<FilterElement> Filters { get; } = [];

	/// <summary>
	/// データテーブルを取得します。
	/// </summary>
	/// <returns>データテーブルを返します。</returns>
	public virtual DataTable GetTable()
	{
		try
		{
			// データテーブルを取得するコアメソッドを呼び出す。
			return GetTableCore();
		}
		catch (Exception ex)
		{
			// エラーをログに記録する。
			LogError(ex);
			// エラーが発生した場合は空のデータテーブルを返す。
			return new DataTable();
		}
	}

	/// <summary>
	/// データテーブルを取得するコアメソッド
	/// </summary>
	/// <returns>データテーブルを返します。</returns>
	protected abstract DataTable GetTableCore();

	/// <summary>
	/// データテーブルの列名を取得します。
	/// </summary>
	/// <returns>列名を含む配列を返します。</returns>
	public virtual string[] GetColumnNames() => GetTableColumnNames();

	/// <summary>
	/// データテーブルの列名を取得するコアメソッド
	/// </summary>
	/// <returns>列名を含む配列を返します。</returns>
	public virtual string[] GetTableColumnNames() => Array.Empty<string>();

	/// <summary>
	/// データテーブルの列名とSQL列名のマッピングを取得します。
	/// </summary>
	/// <returns>列名とSQL列名のマッピングを含む辞書を返します。</returns>
	public virtual Dictionary<string, string> TableColumnToSqlMapping { get; } = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// フィルター条件を追加します。
	/// </summary>
	/// <param name="logic">論理演算子（AND または OR）</param>
	/// <param name="field">フィールド名</param>
	/// <param name="filterOperator">フィルター演算子</param>
	/// <param name="value">フィルター値</param>
	public void AddFilterCondition(string logic, string field, FilterOperator filterOperator, string value)
		=> Filters.Add(new FilterCondition(logic, field, filterOperator, value));

	/// <summary>
	/// フィルター条件を適用します。
	/// </summary>
	/// <param name="query">フィルターを適用するクエリ</param>
	/// <returns>フィルターが適用されたクエリを返します。</returns>
	protected virtual IEnumerable<T> ApplyFilters<T>(IEnumerable<T> query)
	{
		if (Filters.Count > 0)
		{
			// フィルター条件を構築するための述語を作成します。
			var predicate = BuildLinqFilterPredicate();
			if (predicate != null)
			{
				// フィルター条件を適用して、null でない要素を返します。
				query = query.Where(x => x != null && predicate(x)).ToList();
			}
		}
		// フィルターがない場合は、元のクエリをそのまま返します。
		return query;
	}

	/// <summary>
	/// LINQ フィルター述語を構築します。
	/// </summary>
	/// <returns>フィルター述語を返します。</returns>
	public virtual Func<object, bool> BuildLinqFilterPredicate()
	{
		// フィルター条件を逆ポーランド記法（RPN）に変換するための出力リストと演算子スタックを作成します。
		var output = new List<object>();
		// 演算子の優先順位を管理するためのスタックを作成します。
		var ops = new Stack<string>();

		// フィルター条件を順に処理します。
		foreach (var filter in Filters)
		{
			if (filter is FilterCondition cond)
			{
				// 論理演算子が AND または OR の場合、演算子の優先順位に基づいて出力リストに追加します。
				if (cond.Logic == "AND" || cond.Logic == "OR")
				{
					// 演算子スタックのトップにある演算子の優先順位が現在の演算子の優先順位以上である限り、出力リストに追加します。
					while (ops.Count > 0 && Precedence(ops.Peek()) >= Precedence(cond.Logic))
						output.Add(ops.Pop());
					// 現在の演算子を演算子スタックに追加します。
					ops.Push(cond.Logic);
				}
				else
				{
					// フィルター条件を出力リストに追加します。
					output.Add(cond);
				}
			}
			else if (filter is FilterGroupStart)
			{
				// グループの開始を示す "(" を演算子スタックに追加します。
				ops.Push("(");
			}
			else if (filter is FilterGroupEnd)
			{
				// グループの終了を示す ")" が見つかるまで、演算子スタックから出力リストに追加します。
				while (ops.Count > 0 && ops.Peek() != "(") output.Add(ops.Pop());
				// "(" を演算子スタックから削除します。
				if (ops.Count > 0) ops.Pop();
			}
		}

		// 残りの演算子を出力リストに追加します。
		while (ops.Count > 0) output.Add(ops.Pop());

		// 逆ポーランド記法（RPN）を使用して、フィルター条件を評価するための述語を構築します。
		var stack = new Stack<Func<object, bool>>();

		foreach (var token in output)
		{
			if (token is FilterCondition condToken)
			{
				// フィールド名と値を取得します。
				var field = condToken.Field;
				var value = condToken.Value;

				// フィルター条件に基づいて、オブジェクトを評価する述語をスタックに追加します。
				stack.Push(obj =>
				{
					if (obj == null) return false;
					string? targetValue;

					if (field.Contains('.'))
					{
						// フィールド名が "table.column" の形式の場合、テーブル名と列名に分割します。
						var parts = field.Split('.', 2);
						var tbl = parts[0];
						var col = parts[1];
						// テーブル名に対応するプロパティを取得します。
						var prop = obj.GetType().GetProperty(tbl);
						if (prop == null) return false;
						// プロパティの値を DataRow として取得します。
						var row = prop.GetValue(obj) as DataRow;
						if (row == null || !row.Table.Columns.Contains(col)) return false;
						// DataRow から列の値を取得します。
						targetValue = row.Field<string>(col);
					}
					else if (obj is IDictionary<string, string?> row)
					{
						// IDictionary<string, string?> の場合、フィールド名に対応する値を取得します。
						if (!row.TryGetValue(field, out targetValue)) return false;
					}
					else
					{
						// その他のオブジェクトの場合、プロパティ名に対応する値を取得します。
						var p = obj.GetType().GetProperty(field);
						if (p == null) return false;
						// プロパティの値を文字列として取得します。
						targetValue = p.GetValue(obj)?.ToString();
					}

					if (string.IsNullOrEmpty(targetValue)) return false;

					return condToken.Operator switch
					{
						// フィルター演算子に応じて、文字列の比較を行います。
						FilterOperator.Contains => targetValue.Contains(value),
						FilterOperator.StartsWith => targetValue.StartsWith(value),
						FilterOperator.EndsWith => targetValue.EndsWith(value),
						_ => targetValue == value
					};
				});
			}
			else if (token is string op && (op == "AND" || op == "OR"))
			{
				// スタックから右オペランドと左オペランドをポップします。
				var right = stack.Pop();
				var left = stack.Pop();
				// AND または OR の演算子に応じて、述語を結合してスタックにプッシュします。
				if (op == "AND") stack.Push(obj => left(obj) && right(obj));
				// OR の場合
				else stack.Push(obj => left(obj) || right(obj));
			}
		}
		// 最終的な述語をスタックからポップして返します。スタックが空の場合は、常に true を返す述語を返します。
		return stack.Count > 0 ? stack.Pop() : (obj => true);
		// フィルター演算子の優先順位を返すヘルパーメソッド
		static int Precedence(string op) => op == "AND" ? 2 : op == "OR" ? 1 : 0;
	}

	/// <summary>
	/// SQL WHERE 句を構築します。
	/// </summary>
	/// <param name="tableName">テーブル名</param>
	/// <param name="parameters">出力パラメータのリスト</param>
	/// <returns>構築された SQL WHERE 句</returns>
	public virtual string BuildSqlWhereClause(string tableName, out List<DbParameter> parameters)
	{
		// WHERE 句の条件を格納するリストを作成します。
		var clauses = new List<string>();
		parameters = [];
		int paramIndex = 0;
		// 列名と SQL 列名のマッピングを取得します。指定がない場合は空の辞書を使用します。
		var map = TableColumnToSqlMapping ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		foreach (var cond in Filters.OfType<FilterCondition>())
		{
			string? left = null;

			// フィールド名にドットが含まれている場合、テーブル名と列名に分割します。
			if (cond.Field.Contains('.'))
			{
				// フィールド名をテーブル名と列名に分割します。
				var parts = cond.Field.Split('.', 2);
				var tbl = parts[0];
				var col = parts[1];
				// テーブル名と列名を引用符で囲みます。
				_ = RequiredDBDialect.QuoteIdentifier(tbl);
				// テーブル名のエイリアスを正規化します。
				var alias = tbl.Length > 0 ? RequiredDBDialect.NormalizeAlias(tbl.Substring(0, 1)) : tbl;
				left = $"{RequiredDBDialect.QuoteIdentifier(alias)}.{RequiredDBDialect.QuoteIdentifier(col)}";
			}
			else if (map.TryGetValue(cond.Field, out var mapped))
			{
				// マッピングが存在する場合、マッピングされた SQL 列名を使用します。
				left = mapped;
			}
			else
			{
				// マッピングが存在しない場合、テーブル名と列名を引用符で囲んで使用します。
				continue;
			}

			if (string.IsNullOrEmpty(left)) continue;

			// パラメータ名を生成します。
			var paramName = "@p" + paramIndex;
			var value = cond.Value;

			switch (cond.Operator)
			{
				// フィルター演算子に応じて、SQL WHERE 句の条件を構築します。
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
			// パラメータインデックスをインクリメントします。
			paramIndex++;
		}
		// WHERE 句が存在する場合は "WHERE " を付加して返します。存在しない場合は空文字を返します。
		return clauses.Count > 0 ? "WHERE " + string.Join(" ", clauses) : "";
	}

	/// <summary>
	///　指定されたデータテーブルから、目的変数と説明変数の列に有効な値を持つ行のみを抽出します。	
	/// </summary>
	/// <param name="source">元のデータテーブル</param>
	/// <param name="responseName">目的変数の列名</param>
	/// <param name="predictorNames">説明変数の列名リスト</param>
	/// <returns>有効な行のみを含むデータテーブル、無効な行がない場合は元のデータテーブルのクローン</returns>
	public static DataTable? GetValidRows(DataTable source, string responseName, List<string> predictorNames)
	{
		if (source == null) return null;

		// 目的変数と説明変数の列が存在し、値が null または空白でない行のみを抽出します。
		var validRows = source.AsEnumerable()
			.Where(row =>
				!row.IsNull(responseName) &&
				!string.IsNullOrWhiteSpace(row[responseName]?.ToString()) &&
				predictorNames.All(name =>
					!row.IsNull(name) &&
					!string.IsNullOrWhiteSpace(row[name]?.ToString())
				)
			).ToList();
		// 有効な行が存在する場合は、それらの行を含む新しいデータテーブルを、存在しない場合は、元のデータテーブルの構造を持つ空のデータテーブルを返します。
		return validRows.Count > 0 ? validRows.CopyToDataTable() : source.Clone();
	}

	/// <summary>
	/// 指定された SQL クエリを実行し、結果をデータテーブルとして返します。
	/// </summary>
	/// <param name="sql">実行する SQL クエリ</param>
	/// <param name="parameters">SQL パラメータのリスト</param>
	/// <param name="connectionFactory">データベース接続を作成するファクトリ関数</param>
	/// <param name="setupCommand">コマンドの追加設定を行うオプションのアクション</param>
	/// <returns>クエリ結果を含むデータテーブル</returns>
	protected virtual DataTable ExecuteSqlQuery(
		string sql,
		List<DbParameter> parameters,
		Func<DbConnection> connectionFactory,
		Action<DbCommand>? setupCommand = null)
	{
		// データベース接続を作成し、SQL コマンドを実行して結果をデータテーブルとして返します。
		using var conn = connectionFactory();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		// SQL パラメータをコマンドに追加します。
		foreach (var p in parameters)
			cmd.Parameters.Add(p);

		// 追加のコマンド設定が指定されている場合は、setupCommand を呼び出します。
		setupCommand?.Invoke(cmd);
		// データプロバイダーファクトリを取得し、データアダプターを作成します。
		var factory = DbProviderFactories.GetFactory(conn);
		var adapter = factory?.CreateDataAdapter()
			?? throw new InvalidOperationException("DataAdapter の作成に失敗しました。");

		try
		{
			// データアダプターの SelectCommand に SQL コマンドを設定します。
			adapter.SelectCommand = cmd;
			// データテーブルを作成し、データアダプターを使用して SQL クエリの結果をデータテーブルに読み込みます。
			var table = new DataTable();
			adapter.Fill(table);
			// データテーブルを返します。
			return table;
		}
		finally
		{
			// データアダプターを破棄します。
			adapter.Dispose();
		}
	}

	/// <summary>
	/// エラーをログに記録します。
	/// </summary>
	/// <param name="ex">記録する例外</param>
	protected virtual void LogError(Exception ex)
	{
		Console.WriteLine($"[DataImporter Error] {ex.Message}");
	}
}

/// <summary>
/// CSVファイルからデータをインポートするためのクラスです。
/// </summary>
public class CSVImporter : DataImporter
{
	/// <summary>
	/// CSVファイルのパス
	/// </summary>
	private readonly string _csvPath;

	/// <summary>
	/// コンストラクター
	/// </summary>
	/// <param name="csvPath">CSVファイルのパス</param>
	/// <param name="name">インポーターの名前（省略可能）</param>
	public CSVImporter(string csvPath, string? name = null) : base(name ?? csvPath, null)
	{
		_csvPath = csvPath;
	}

	/// <summary>
	/// CSVファイルの列名を取得します。
	/// </summary>
	public override string[] GetTableColumnNames()
	{
		// CSVファイルを読み込み、列名を取得します。
		var table = CsvHelpers.ReadCsv(_csvPath);
		return table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
	}

	/// <summary>
	/// CSVファイルのデータを取得します。
	/// </summary>
	/// <returns>CSVファイルのデータを含むデータテーブル</returns>
	protected override DataTable GetTableCore()
	{
		// CSVファイルを読み込みます。
		var table = CsvHelpers.ReadCsv(_csvPath);
		if (Filters.Count == 0)
		{
			return table;
		}
		// フィルター条件を適用するために、データテーブルの行を辞書に変換します。
		var projected = table.AsEnumerable().Select(row =>
		{
			// 各行を列名と値の辞書に変換します。
			var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
			foreach (DataColumn column in table.Columns)
			{
				// 列名をキー、列の値を文字列として辞書に追加します。
				dict[column.ColumnName] = row[column]?.ToString();
			}
			return dict;
		});
		// フィルター条件を適用して、有効な行のみを抽出します。
		var filtered = ApplyFilters(projected).ToList();
		var result = table.Clone();

		// フィルター条件に基づいて有効な行を結果のデータテーブルに追加します。
		foreach (var item in filtered)
		{
			var newRow = result.NewRow();
			// 各列に対して、辞書から値を取得し、データテーブルの行に設定します。
			foreach (DataColumn column in result.Columns)
			{
				// 列名に対応する値を取得し、DBNull.Value に変換して設定します。
				newRow[column.ColumnName] = item.TryGetValue(column.ColumnName, out var value) && value != null
					? value
					: DBNull.Value;
			}
			// 新しい行を結果のデータテーブルに追加します。
			result.Rows.Add(newRow);
		}
		// フィルター条件に基づいて有効な行のみを含むデータテーブルを返します。
		return result;
	}
}

/// <summary>
/// データベースからデータをインポートするためのクラスです。
/// </summary>
public class DBImporter : DataImporter, IDbImportSource
{
	/// <summary>
	/// データベースのテーブル名
	/// </summary>
	private readonly string _tableName;
	/// <summary>
	/// データベース接続を作成するためのファクトリ関数
	/// </summary>
	private readonly Func<DbConnection> _connectionFactory;

	/// <summary>
	/// コンストラクター
	/// </summary>
	/// <param name="tableName">データベースのテーブル名</param>
	/// <param name="dialect">データベース方言（省略可能）</param>
	/// <param name="connectionFactory">データベース接続を作成するためのファクトリ関数（省略可能）</param>
	/// <param name="name">インポーターの名前（省略可能）</param>
	public DBImporter(
		string tableName,
		DBDialect? dialect = null,
		Func<DbConnection>? connectionFactory = null,
		string? name = null) : base(name ?? tableName, dialect ?? new MySqlDialect())
	{
		// データベースのテーブル名を設定します。
		_tableName = tableName;
		// データベース接続を作成するためのファクトリ関数を設定します。指定がない場合は MySqlConnectionFactory を使用します。
		_connectionFactory = connectionFactory ?? MySqlConnectionFactory.CreateOpenConnection;
	}

	/// <summary>
	/// データベース方言を取得します。
	/// </summary>
	public DBDialect Dialect => RequiredDBDialect;

	/// <summary>
	/// データベースのテーブルの列名を取得します。
	/// </summary>
	public override string[] GetTableColumnNames()
	{
		// データベースのテーブルの列名を取得するために、SQL クエリを実行します。
		string sql = $"SELECT * FROM {RequiredDBDialect.QuoteIdentifier(_tableName)} t LIMIT 0";
		var table = ExecuteSqlQuery(sql, [], _connectionFactory);
		// データテーブルの列名を配列として返します。
		return table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
	}

	/// <summary>
	/// データベースのテーブルの列名と対応する SQL 式のマッピングを取得します。
	/// </summary>
	/// <returns>列名と対応する SQL 式のマッピングを含む辞書</returns>
	public override Dictionary<string, string> TableColumnToSqlMapping
	{
		get
		{
			// データベースのテーブルの列名を取得します。
			var names = GetTableColumnNames();
			// 列名と対応する SQL 式のマッピングを格納する辞書を作成します。
			var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (var name in names)
			{
				// 列名と対応する SQL 式をマッピングに追加します。ここでは、テーブルエイリアス "t" を使用して列名を参照します。
				map[name] = $"t.{name}";
			}
			// マッピングを返します。
			return map;
		}
	}

	/// <summary>
	/// データベースのテーブルのデータを取得します。
	/// </summary>
	/// <returns>データベースのテーブルのデータを含むデータテーブル</returns>
	protected override DataTable GetTableCore()
	{
		// SQL WHERE 句を構築し、パラメータを取得します。
		var whereSql = BuildSqlWhereClause("t", out var parameters);
		string sql = $"SELECT * FROM {RequiredDBDialect.QuoteIdentifier(_tableName)} t {whereSql}";
		// SQL クエリを実行して、結果をデータテーブルとして返します。
		return ExecuteSqlQuery(sql, parameters, _connectionFactory);
	}
}