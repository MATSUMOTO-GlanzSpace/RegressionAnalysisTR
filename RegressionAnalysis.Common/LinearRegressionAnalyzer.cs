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
		public class ColumnInfo
		{
			public string Name { get; set; }
			public bool IsNumeric { get; set; }

			private string? _format;
			public string? Format
			{
				get => _format;
				set
				{
					_format = value;
					// IsNumericの場合、FormatがあればDataType=string、なければdouble
					_dataType = IsNumeric && !string.IsNullOrEmpty(_format) ? typeof(string)
							  : IsNumeric ? typeof(double) : typeof(string);
				}
			}

			private Type _dataType;
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

			public ColumnInfo(string name, bool isNumeric, string? format = null)
			{
				Name = name;
				IsNumeric = isNumeric;
				Format = format;
				_dataType = (IsNumeric && !string.IsNullOrEmpty(format)) ? typeof(string)
						  : (IsNumeric ? typeof(double) : typeof(string));
			}
		}

		public List<ColumnInfo> Columns { get; } = new()
		{
			new ColumnInfo("変数名", false),
			new ColumnInfo("回帰係数", true, "E4"),
			new ColumnInfo("VIF", true, "E4"),
			new ColumnInfo("片側p値", true, "E4"),
			new ColumnInfo("両側p値", true, "E4")
		};

		public DataTable VariableStats { get; set; }
		public double RSquared { get; set; }
		public double AdjustedRSquared { get; set; }

		public LinearRegressionResult()
		{
			VariableStats = new DataTable();
			foreach (var col in Columns)
				VariableStats.Columns.Add(col.Name, col.DataType);
		}

		public ColumnInfo? GetColumnInfo(string columnName)
		{
			return Columns.FirstOrDefault(c => c.Name == columnName);
		}
	}

	/// <summary>
	/// 線形回帰分析を実行し、変数名・回帰係数・p値・VIF・決定係数を持つ結果インスタンスを返します。
	/// </summary>
	/// <remarks>
	/// このクラスは、回帰分析を用いて従属変数（目的変数）と複数の独立変数（説明変数）の関係を分析する機能を提供します。
	/// 分析には、回帰係数の算出、統計的有意性（p値）、多重共線性診断（VIF）、および適合度指標（決定係数や補正決定係数）の計算が含まれます。
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
			for (int col = 0; col < table.Columns.Count; col++)
			{
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
		/// <param name="predictors"></param>
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
				for (int col = 0; col < otherPredictors.Length; col++)
					otherPredictorsTable.Columns.Add($"X{col}", typeof(double));
				for (int row = 0; row < n; row++)
				{
					var values = new object[otherPredictors.Length];
					for (int col = 0; col < otherPredictors.Length; col++)
						values[col] = otherPredictors[col][row];
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
		/// <param name="response"></param>
		/// <param name="yHat"></param>
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
		/// <param name="response"></param>
		/// <param name="yHat"></param>
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
		/// 線形回帰分析を実行し、変数名・回帰係数・p値・決定係数を持つ結果インスタンスを返す
		/// 重回帰分析の場合にはVIFを含む
		/// </summary>
		/// <param name="responseTable">目的変数のDataTable（1列のみ）</param>
		/// <param name="predictorTable">説明変数のDataTable（各列が変数）</param>
		/// <returns>LinearRegressionResult</returns>
		public static LinearRegressionResult Analyze(DataTable responseTable, DataTable predictorTable)
		{
			// データの数値チェック
			CheckDataTableNumeric(responseTable);
			CheckDataTableNumeric(predictorTable);

			// データ数と変数数の取得
			int n = responseTable.Rows.Count;
			int k = predictorTable.Columns.Count;
			int df = n - k - 1;
			var coefficients = CalcRegressionCoefficients(predictorTable, responseTable);
			var predictorsJagged = ToJaggedArray(predictorTable);
			var response = responseTable.AsEnumerable().Select(r => Convert.ToDouble(r[0])).ToArray();
			var yHat = CalcPredictedValues(predictorsJagged, coefficients);
			double ssRes = CalcResidualSumOfSquares(response, yHat);
			var r2 = CalcRSquared(response, yHat);
			var adjR2 = CalcAdjustedRSquared(r2, n, k);

			var result = new LinearRegressionResult();
			var variableStats = result.VariableStats;
			var cols = result.Columns;

			var vifList = CalcVIFs(predictorsJagged);
			var se = CalcStandardErrors(predictorsJagged, response, ssRes, df);

			// 定数項のp値計算
			double t0 = CalcTValue(coefficients[0], se[0]);
			double pOneSided0 = CalcOneSidedPValue(t0, df);
			double pTwoSided0 = 2 * pOneSided0;

			// 例: LinearRegressionResult.Analyzeメソッド内での定数項の追加
			var row = variableStats.NewRow();
			row["変数名"] = "定数項";
			row["回帰係数"] = cols[1].Format != null ? coefficients[0].ToString(cols[1].Format) : coefficients[0];
			row["VIF"] = cols[2].Format != null ? double.NaN.ToString(cols[2].Format) : double.NaN;
			row["片側p値"] = cols[3].Format != null ? pOneSided0.ToString(cols[3].Format) : pOneSided0;
			row["両側p値"] = cols[4].Format != null ? pTwoSided0.ToString(cols[4].Format) : pTwoSided0;
			variableStats.Rows.Add(row);

			// 各説明変数も同様に
			for (int i = 0; i < k; i++)
			{
				double t = CalcTValue(coefficients[i + 1], se[i + 1]);
				double pOneSided = CalcOneSidedPValue(t, df);
				double pTwoSided = 2 * pOneSided;
				var vrow = variableStats.NewRow();
				vrow["変数名"] = predictorTable.Columns[i].ColumnName;
				vrow["回帰係数"] = cols[1].Format != null ? coefficients[i + 1].ToString(cols[1].Format) : coefficients[i + 1];
				vrow["VIF"] = cols[2].Format != null ? vifList[i].ToString(cols[2].Format) : vifList[i];
				vrow["片側p値"] = cols[3].Format != null ? pOneSided.ToString(cols[3].Format) : pOneSided;
				vrow["両側p値"] = cols[4].Format != null ? pTwoSided.ToString(cols[4].Format) : pTwoSided;
				variableStats.Rows.Add(vrow);
			}

			result.RSquared = r2;
			result.AdjustedRSquared = adjR2;
			return result;
		}
	}
}
