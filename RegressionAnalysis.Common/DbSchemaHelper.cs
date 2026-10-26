using MySqlConnector;
using static RegressionAnalysis.Common.DataTableHelpers;


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


		/// <summary>
		/// MySQLデータベースから接続データベースの全テーブル名とビュー名を取得するメソッド
		/// </summary>
		/// <param name="conn">MySqlConnectionオブジェクト</param>
		/// <param name="type">取得するテーブルの種類</param>
		/// <returns>テーブル名とビュー名のリスト</returns>
		public static List<(string Name, string Type)> GetAllTableNames(MySqlConnection conn, TableType type = TableType.TypeAll)
		{
			// テーブル名とビュー名のリストを格納するリストを作成
			var items = new List<(string Name, string Type)>();

			// SQLコマンドを作成して実行
			using var cmd = conn.CreateCommand();
			var sql = @"
                SELECT table_name, table_type
                FROM information_schema.tables
                WHERE table_schema = @db";
			// テーブルの種類に応じて条件を追加
			switch (type)
			{
				case TableType.TypeTable:
					sql += " AND table_type = 'BASE TABLE'";
					break;
				case TableType.TypeView:
					sql += " AND table_type = 'VIEW'";
					break;
				case TableType.TypeAll:
					sql += " AND table_type IN ('BASE TABLE', 'VIEW')";
					break;
			}
			sql += " ORDER BY table_name;";

			// SQLコマンドを設定
			cmd.CommandText = sql;
			cmd.Parameters.AddWithValue("@db", conn.Database);

			// SQLコマンドを実行して結果をリストに格納
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				items.Add((reader.GetString(0), reader.GetString(1)));
			}
			// リストを返す
			return items;
		}
	}
}

