using MySqlConnector;


namespace RegressionAnalysis.Common
{
	public static class DbSchemaHelper
	{
		/// <summary>
		/// 指定した接続からテーブル名のホワイトリストを取得（MySQL用）
		/// </summary>
		public static HashSet<string> GetTableWhitelist_MySql(MySqlConnection conn)
		{
			var whitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			using var cmd = new MySqlCommand("SHOW TABLES", conn);
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				whitelist.Add(reader.GetString(0));
			}
			return whitelist;
		}

		// 将来的に他DBMS用のメソッドも追加可能
		// public static HashSet<string> GetTableWhitelist_SqlServer(SqlConnection conn, string dbName) { ... }
		// public static HashSet<string> GetTableWhitelist_PostgreSql(NpgsqlConnection conn, string dbName) { ... }
	}
}

