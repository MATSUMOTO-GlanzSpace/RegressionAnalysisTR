using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;
using static RegressionAnalysis.Common.DataTableHelpers;

namespace RegressionAnalysis.Common
{
	/// <summary>
	/// 線形帰分析を行った、係数・p値・決定係数を表す結果クラス
	/// 重回帰分析の場合にはVIFを含む
	/// </summary>
	public class LinearRegressionResult
	{
		/// <summary>
		/// カラム情報クラス
		/// </summary>
		public class ColumnInfo
		{
			// 数値カラムかどうか
			public bool IsNumeric { get; set; }

			// 書式文字列
			private string? _format;
			// 書式文字列プロパティ
			public string? Format
			{
				get => _format;
				set
				{
					_format = value;
					// 書式が設定されている場合、DataTypeはstringにする
					_dataType = IsNumeric && !string.IsNullOrEmpty(_format) ? typeof(string)
							  : IsNumeric ? typeof(double) : typeof(string);
				}
			}
			// データ型
			private Type _dataType;
			// データ型プロパティ
			public Type DataType
			{
				get => _dataType;
				set
				{
					_dataType = value;
					// DataTypeがstring以外ならFormatをnullに
					if (IsNumeric && _dataType != typeof(string)) _format = null;
				}
			}

			/// <summary>
			/// ColumnInfo クラスの新しいインスタンスを初期化します。
			/// </summary>
			/// <param name="isNumeric">数値カラムかどうか</param>
			/// <param name="format">数値カラムの場合の書式文字列（例: "E4"、"F2" など）</param>
			public ColumnInfo(bool isNumeric, string? format = null)
			{
				IsNumeric = isNumeric;
				Format = format;
				_dataType = (IsNumeric && !string.IsNullOrEmpty(format)) ? typeof(string)
						  : (IsNumeric ? typeof(double) : typeof(string));
			}
		}

		// カラム名をキーとする連想配列でカラム情報を保持
		public Dictionary<string, ColumnInfo> Columns { get; } = new()
		{
			{ "変数名", new ColumnInfo(false) },
			{ "回帰係数", new ColumnInfo(true, "E4") },
			{ "VIF", new ColumnInfo(true, "E4") },
			{ "片側p値", new ColumnInfo(true, "E4") },
			{ "両側p値", new ColumnInfo(true, "E4") }
		};

		// 変数ごとの統計情報を保持するDataTable
		public DataTable VariableStats { get; set; }
		// 決定係数
		public double RSquared { get; set; }
		// 補正決定係数
		public double AdjustedRSquared { get; set; }

		/// <summary>
		/// LinearRegressionResult クラスの新しいインスタンスを初期化します。
		/// </summary>
		public LinearRegressionResult()
		{
			VariableStats = new DataTable();
			foreach (var kv in Columns)
				VariableStats.Columns.Add(kv.Key, kv.Value.DataType);
		}

		/// <summary>
		/// 指定したカラム名に対応するカラム情報を取得します。
		/// </summary>
		/// <param name="columnName">カラム名</param>
		public ColumnInfo? GetColumnInfo(string columnName)
			=> Columns.TryGetValue(columnName, out var info) ? info : null;
	}

	/// <summary>
	/// 線形回帰分析を実行し、変数名・回帰係数・p値・VIF・決定係数を持つ結果インスタンスを返します。
	/// </summary>
	/// <param name="responseTable">目的変数（従属変数）のDataTable（1列のみ）</param>
	/// <param name="predictorTable">説明変数（独立変数）のDataTable（各列が変数）</param>
	/// <returns>回帰係数・VIF・p値・決定係数などを含む LinearRegressionResult インスタンス</returns>
	/// <remarks>
	/// このメソッドは、与えられたデータテーブルから線形回帰分析を行い、
	/// ・定数項および各説明変数の回帰係数
	/// ・各変数のVIF（定数項はNaN）
	/// ・各変数の片側/両側p値
	/// ・決定係数および補正決定係数
	/// を含む結果を返します。
	/// 欠損値や非数値データが含まれる場合は例外をスローします。
	/// </remarks>
	public class LinearRegressionAnalyzer
	{
		/// <summary>
		/// 数値カラム用書式属性
		/// </summary>
		public class NumericColumnFormat
		{
			// 回帰係数書式
			public string? RegressionCoefficient { get; set; } = "E4";
			// VIF書式
			public string? Vif { get; set; } = "E4";
			// 片側p値書式
			public string? PValueOneSided { get; set; } = "E4";
			// 両側p値書式
			public string? PValueTwoSided { get; set; } = "E4";
		}

		// 数値カラム用書式属性
		public static NumericColumnFormat NumericFormat { get; set; } = new();

		/// <summary>
		///	データテーブルの数値チェック
		/// </summary>
		/// <param name="table"></param>
		/// <exception cref="ArgumentException"></exception>
		private static void CheckDataTableNumeric(DataTable table)
		{
			// 各セルをチェック
			for (int col = 0; col < table.Columns.Count; col++)
			{
				// 各行をチェック
				for (int row = 0; row < table.Rows.Count; row++)
				{
					// 欠損値のチェック
					if (table.Rows[row].IsNull(col) || string.IsNullOrWhiteSpace(table.Rows[row][col]?.ToString()))
						throw new ArgumentException($"{table.TableName}の{row + 1}行{table.Columns[col].ColumnName}列に欠損値（nullまたは空文字）が含まれています。");
					var value = table.Rows[row][col]?.ToString();
					// 数値のチェック
					if (!double.TryParse(value, out _))
						throw new ArgumentException($"{table.TableName}の{row + 1}行{table.Columns[col].ColumnName}列の値 '{value}' は数値として読み込めません。");
				}
			}
		}

		/// <summary>
		/// 標準誤差の計算
		/// </summary>
		/// <param name="X">説明変数ジャグ配列</param>
		/// <param name="response">目的変数配列</param>
		/// <param name="ssRes">残差平方和</param>
		/// <param name="df">自由度</param>
		/// <returns>標準誤差配列</returns>
		private static double[] CalcStandardErrors(double[][] X, double[] response, double ssRes, int df)
		{
			// X行列の作成（定数項を含む）
			int n = response.Length;
			int k = X[0].Length;
			var Xarr = new double[n, k + 1];
			for (int i = 0; i < n; i++)
			{
				Xarr[i, 0] = 1.0;
				for (int j = 0; j < k; j++)
					Xarr[i, j + 1] = X[i][j];
			}
			// (X'X)^-1 の計算
			var Xmat = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(Xarr);
			var XXinv = Xmat.TransposeThisAndMultiply(Xmat).Inverse();
			// 標準誤差の計算
			double mse = ssRes / df;
			var se = new double[k + 1];
			for (int i = 0; i < k + 1; i++)
				se[i] = Math.Sqrt(mse * XXinv[i, i]);
			return se;
		}

		/// <summary>
		/// VIFの計算
		/// </summary>
		/// <param name="predictors">説明変数ジャグ配列</param>
		/// <returns>VIF配列</returns>
		private static List<double> CalcVIFs(double[][] predictors)
		{
			// 各説明変数を目的変数として回帰分析を行い、VIFを計算
			var vifList = new List<double>();
			int n = predictors.Length;
			int k = predictors[0].Length;
			for (int i = 0; i < k; i++)
			{
				// 目的変数を取得
				var target = predictors.Select(row => row[i]).ToArray();
				var targetTable = new DataTable();
				targetTable.Columns.Add("Y", typeof(double));
				for (int row = 0; row < n; row++)
					targetTable.Rows.Add(target[row]);
				// 他の説明変数を取得
				var otherPredictors = Enumerable.Range(0, k)
					.Where(idx => idx != i)
					.Select(idx => predictors.Select(row => row[idx]).ToArray())
					.ToArray();
				var otherPredictorsTable = new DataTable();
				// 他の説明変数カラムを追加
				for (int col = 0; col < otherPredictors.Length; col++)
					otherPredictorsTable.Columns.Add($"X{col}", typeof(double));
				// 他の説明変数データを追加
				for (int row = 0; row < n; row++)
				{
					var values = new object[otherPredictors.Length];
					// 各カラムの値を設定
					for (int col = 0; col < otherPredictors.Length; col++)
						values[col] = otherPredictors[col][row];
					// 行を追加
					otherPredictorsTable.Rows.Add(values);
				}
				// 回帰分析の実行
				var ols = CalcRegressionCoefficients(otherPredictorsTable, targetTable);
				// 予測値の計算
				var otherPredictorsJagged = ToJaggedArray(otherPredictorsTable);
				var yHatVif = CalcPredictedValues(otherPredictorsJagged, ols);
				// 決定係数の計算
				double r2Vif = CalcRSquared(target, yHatVif);
				double vif = 1.0 / (1.0 - r2Vif);
				// VIFの計算
				vifList.Add(vif);
			}
			return vifList;
		}

		/// <summary>
		/// 決定係数の計算
		/// </summary>
		/// <param name="response">目的変数配列</param>
		/// <param name="yHat">予測値配列</param>
		/// <returns>決定係数</returns>
		private static double CalcRSquared(double[] response, double[] yHat)
		{
			double ssRes = response.Zip(yHat, (y, yh) => Math.Pow(y - yh, 2)).Sum();
			double ssTot = response.Select(y => Math.Pow(y - response.Average(), 2)).Sum();
			return 1 - ssRes / ssTot;
		}

		/// <summary>
		/// 補正決定係数の計算
		/// </summary>
		/// <param name="r2">決定係数</param>
		/// <param name="n">データ数</param>
		/// <param name="k">変数数</param>
		/// <returns>補正決定係数</returns>
		private static double CalcAdjustedRSquared(double r2, int n, int k)
		{
			return 1 - (1 - r2) * (n - 1) / (n - k - 1);
		}

		/// <summary>
		/// 予測値の計算
		/// </summary>
		/// <param name="predictorsJagged">説明変数ジャグ配列</param>
		/// <param name="coefficients">回帰係数配列</param>
		/// <returns>予測値配列</returns>
		private static double[] CalcPredictedValues(double[][] predictorsJagged, double[] coefficients)
		{
			int n = predictorsJagged.Length;
			int k = predictorsJagged[0].Length;
			var yHat = new double[n];
			for (int i = 0; i < n; i++)
			{
				yHat[i] = coefficients[0];
				for (int j = 0; j < k; j++)
					yHat[i] += coefficients[j + 1] * predictorsJagged[i][j];
			}
			return yHat;
		}

		/// <summary>
		/// 残差平方和の計算
		/// </summary>
		/// <param name="response">目的変数配列</param>
		/// <param name="yHat">予測値配列</param>
		/// <returns>残差平方和</returns>
		private static double CalcResidualSumOfSquares(double[] response, double[] yHat)
		{
			return response.Zip(yHat, (y, yh) => Math.Pow(y - yh, 2)).Sum();
		}

		/// <summary>
		/// 片側p値の計算
		/// </summary>
		/// <param name="t">t値</param>
		/// <param name="degreesOfFreedom">自由度</param>
		/// <returns>片側p値</returns>
		private static double CalcOneSidedPValue(double t, int degreesOfFreedom)
		{
			var dist = new StudentT(0, 1, degreesOfFreedom);
			return 1 - dist.CumulativeDistribution(Math.Abs(t));
		}

		/// <summary>
		/// t値の計算
		/// </summary>
		/// <param name="coefficient">回帰係数</param>
		/// <param name="standardError">標準誤差</param>
		/// <returns>t値</returns>
		private static double CalcTValue(double coefficient, double standardError)
		{
			return coefficient / standardError;
		}

		/// <summary>
		/// 指定した説明変数DataTableと目的変数DataTableから回帰係数を計算する
		/// </summary>
		/// <param name="predictors">説明変数DataTable</param>
		/// <param name="response">目的変数DataTable（1列のみ）</param>
		/// <returns>回帰係数配列</returns>
		private static double[] CalcRegressionCoefficients(DataTable predictors, DataTable response)
		{
			var predictorsJagged = ToJaggedArray(predictors);
			var responseArray = response.AsEnumerable().Select(r => Convert.ToDouble(r[0])).ToArray();
			return MultipleRegression.QR(predictorsJagged, responseArray, intercept: true);
		}

		/// <summary>
		/// 線形回帰分析を実行し、変数名・回帰係数・p値・VIF・決定係数を持つ結果インスタンスを返します。
		/// </summary>
		/// <param name="responseTable">目的変数（従属変数）のDataTable（1列のみ）</param>
		/// <param name="predictorTable">説明変数（独立変数）のDataTable（各列が変数）</param>
		/// <returns>回帰係数・VIF・p値・決定係数などを含む LinearRegressionResult インスタンス</returns>
		/// <remarks>
		/// このメソッドは、与えられたデータテーブルから線形回帰分析を行い、
		/// ・定数項および各説明変数の回帰係数
		/// ・各変数のVIF（定数項はNaN）
		/// ・各変数の片側/両側p値
		/// ・決定係数および補正決定係数
		/// を含む結果を返します。
		/// 欠損値や非数値データが含まれる場合は例外をスローします。
		/// </remarks>
		public static LinearRegressionResult Analyze(DataTable responseTable, DataTable predictorTable)
		{
			// 入力データの数値チェック（欠損値・非数値があれば例外）
			CheckDataTableNumeric(responseTable);
			CheckDataTableNumeric(predictorTable);

			// データ数・変数数・自由度の取得
			int n = responseTable.Rows.Count;
			int k = predictorTable.Columns.Count;
			int df = n - k - 1;

			// 回帰係数の計算
			var coefficients = CalcRegressionCoefficients(predictorTable, responseTable);

			// 説明変数のジャグ配列化
			var predictorsJagged = ToJaggedArray(predictorTable);

			// 目的変数の配列取得
			var response = responseTable.AsEnumerable().Select(r => Convert.ToDouble(r[0])).ToArray();

			// 予測値の計算
			var yHat = CalcPredictedValues(predictorsJagged, coefficients);

			// 残差平方和の計算
			double ssRes = CalcResidualSumOfSquares(response, yHat);

			// 決定係数・補正決定係数の計算
			var r2 = CalcRSquared(response, yHat);
			var adjR2 = CalcAdjustedRSquared(r2, n, k);

			// 結果格納用インスタンスの生成
			var result = new LinearRegressionResult();

			// VIF（分散拡大係数）の計算
			var vifList = CalcVIFs(predictorsJagged);

			// 標準誤差の計算
			var se = CalcStandardErrors(predictorsJagged, response, ssRes, df);

			// 必須カラム情報の取得（なければ例外）
			var colInfo = new VariableStatColumnInfo
			{
				Coefficient = result.GetColumnInfo("回帰係数") ?? throw new InvalidOperationException("回帰係数カラム情報が見つかりません。"),
				Vif = result.GetColumnInfo("VIF") ?? throw new InvalidOperationException("VIFカラム情報が見つかりません。"),
				PValueOneSided = result.GetColumnInfo("片側p値") ?? throw new InvalidOperationException("片側p値カラム情報が見つかりません。"),
				PValueTwoSided = result.GetColumnInfo("両側p値") ?? throw new InvalidOperationException("両側p値カラム情報が見つかりません。")
			};

			// VariableStatリストを作成
			var stats = new List<VariableStat>
			{
				// 定数項（切片）
				new() {
					VariableName = "定数項",
					Coefficient = coefficients[0],
					Vif = double.NaN,	// 定数項のVIFはNaN
					PValueOneSided = CalcOneSidedPValue(CalcTValue(coefficients[0], se[0]), df),
					PValueTwoSided = 2 * CalcOneSidedPValue(CalcTValue(coefficients[0], se[0]), df)
				}
			};
			// 各説明変数
			for (int i = 0; i < predictorTable.Columns.Count; i++)
			{
				// インデックス調整（coefficientsとseは定数項分ずれている）
				int idx = i + 1;
				// t値・p値の計算
				double t = CalcTValue(coefficients[idx], se[idx]);
				double pOneSided = CalcOneSidedPValue(t, df);
				// VariableStatインスタンスを追加
				stats.Add(new VariableStat
				{
					VariableName = predictorTable.Columns[i].ColumnName,
					Coefficient = coefficients[idx],
					Vif = vifList[i],
					PValueOneSided = pOneSided,
					PValueTwoSided = 2 * pOneSided
				});
			}
			// 一括でDataTableに追加
			AddVariableStatsRows(result.VariableStats, stats, colInfo);

			// 決定係数・補正決定係数を結果に格納
			result.RSquared = r2;
			result.AdjustedRSquared = adjR2;

			// 結果を返す
			return result;
		}

		/// <summary>
		/// 変数ごとの統計情報を表すクラス
		/// </summary>
		/// <remarks>
		/// このクラスは、各変数の名前、回帰係数、VIF、片側p値、および両側p値を保持します。
		/// </remarks>
		public class VariableStat
		{
			// 変数名
			public string VariableName { get; set; } = string.Empty;
			// 回帰係数 
			public double Coefficient { get; set; }
			// VIF
			public double Vif { get; set; }
			// 片側p値
			public double PValueOneSided { get; set; }
			// 両側p値
			public double PValueTwoSided { get; set; }
		}

		/// <summary>
		/// 変数統計情報のカラム情報を表すクラス
		/// </summary>
		/// <remarks>
		/// このクラスは、変数統計情報の各カラムに対応する LinearRegressionResult.ColumnInfo インスタンスを保持します。
		/// </remarks>
		public class VariableStatColumnInfo
		{
			// 回帰係数カラム情報
			public LinearRegressionResult.ColumnInfo Coefficient { get; set; } = null!;
			// VIFカラム情報
			public LinearRegressionResult.ColumnInfo Vif { get; set; } = null!;
			// 片側p値カラム情報
			public LinearRegressionResult.ColumnInfo PValueOneSided { get; set; } = null!;
			// 両側p値カラム情報
			public LinearRegressionResult.ColumnInfo PValueTwoSided { get; set; } = null!;
		}

		/// <summary>
		/// 変数統計情報のDataTableに行を追加します。
		/// </summary>
		/// <param name="variableStats">行が追加される変数統計情報</param>
		/// <param name="stats">変数ごとの統計情報リスト</param>
		/// <param name="colInfo">変数統計情報のカラム情報</param>
		private static void AddVariableStatsRows(
			DataTable variableStats,
			IEnumerable<VariableStat> stats,
			VariableStatColumnInfo colInfo)
		{
			// 各変数統計情報をDataTableに追加
			foreach (var stat in stats)
			{
				// 新しい行を作成
				var row = variableStats.NewRow();
				// 各カラムに値を設定
				row["変数名"] = stat.VariableName;
				row["回帰係数"] = colInfo.Coefficient?.Format != null ? stat.Coefficient.ToString(colInfo.Coefficient.Format) : stat.Coefficient;
				row["VIF"] = colInfo.Vif?.Format != null ? stat.Vif.ToString(colInfo.Vif.Format) : stat.Vif;
				row["片側p値"] = colInfo.PValueOneSided?.Format != null ? stat.PValueOneSided.ToString(colInfo.PValueOneSided.Format) : stat.PValueOneSided;
				row["両側p値"] = colInfo.PValueTwoSided?.Format != null ? stat.PValueTwoSided.ToString(colInfo.PValueTwoSided.Format) : stat.PValueTwoSided;
				// 行をDataTableに追加
				variableStats.Rows.Add(row);
			}
		}
	}
}
