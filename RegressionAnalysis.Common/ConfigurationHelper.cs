using Microsoft.Extensions.Configuration;

namespace RegressionAnalysis.Common
{
	/// <summary>
	/// 構成ヘルパークラス
	/// </summary>
	public static class ConfigurationHelper
	{
		/// <summary>
        /// 接続文字列取得メソッド
        /// </summary>
        /// <param name="key">接続文字列キー（デフォルト: MyDbConnection）</param>
        /// <returns>接続文字列</returns>
		public static string GetConnectionString(string key = "MyDbConnection")
		{
			// 実行環境名取得（例: Development, Production, Staging）
			var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
			var configuration = CreateConfiguration(environment);

			// 接続文字列取得
			var connStr = configuration.GetConnectionString(key);
			if (string.IsNullOrEmpty(connStr))
				throw new InvalidOperationException($"接続文字列キー '{key}' が見つかりません");
			return connStr;
		}

		/// <summary>
        /// 構成作成メソッド
        /// </summary>
        /// <param name="environment">環境名</param>
        /// <returns>IConfigurationRootオブジェクト</returns>
		public static IConfigurationRoot CreateConfiguration(string environment)
		{

			// 構成ビルダーでappsettings.{Environment}.jsonを読み込む
			// appsettings.jsonと環境別設定をマージ
			// {Environment}　Development 開発時; Production 本番環境; Staging ステージング環境
			var builder = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables();

			var configuration = builder.Build();
			return configuration;
		}
	}
}