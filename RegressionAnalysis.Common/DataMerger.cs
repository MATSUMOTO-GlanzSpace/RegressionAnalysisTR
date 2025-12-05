using System;
using System.Collections.Generic;
using System.Data;
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
		/// 派生クラスで実装する結合ロジック
		/// </summary>
		/// <returns>結合データテーブル</returns>
		protected abstract DataTable GetMergedDataTableCore();

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
					var value = cond.Condition.Trim('=', '\'', '"');
					stack.Push(row =>
					{
						if (!row.Table.Columns.Contains(field)) return false;
						return row.Field<string>(field) == value;
					});
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
