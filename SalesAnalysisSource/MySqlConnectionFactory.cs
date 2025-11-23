using MySqlConnector;

namespace SalesAnalysisSource
{
    /// <summary>
    /// MySQLコネクション生成・管理用ファクトリクラス
    /// </summary>
    public static class MySqlConnectionFactory
    {
		/// <summary>
		/// コネクション文字列
		/// </summary>
		private static string? _connectionString;

		/// <summary>
		/// コネクション文字列をセット
		/// <param name="connectionString">コネクション文字列</param>
		/// </summary>
		public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

		/// <summary>
		/// オープン済みMySqlConnectionを生成
		/// <return>オープン済みMySqlConnectionインスタンス</return>
		/// </summary>
		public static MySqlConnection CreateOpenConnection()
        {
			// コネクション文字列が設定されていない場合は例外をスロー
			if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Connection string is not set.");
            var conn = new MySqlConnection(_connectionString);
			// コネクションをオープン
			conn.Open();
            return conn;
        }
    }
}
