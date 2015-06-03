using System;
using System.Data.SqlClient;

namespace AkkaChat.Data
{
	/// <summary>
	/// Static class for bootstrapping SQL Server schema
	/// </summary>
	public static class SqlServerInitializer
	{
	   static string SqlUserFormat = @"
			IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{2}' AND TABLE_NAME = '{3}')
			BEGIN
				CREATE TABLE {0}.{1} (
					[UserId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
					[DisplayName] [nvarchar](50) NOT NULL UNIQUE,
                    [CookieId] [uniqueidentifier] NOT NULL UNIQUE,
					[Email] [nvarchar](128) NOT NULL UNIQUE,
					[Gravatar] [nvarchar](50) NULL,
					[PasswordHash] [nvarchar](50) NOT NULL,
					[PasswordSalt] [nvarchar](50) NOT NULL,
					[DateJoined] [datetime] NULL
				);
			END
		";

		static string SqlRoomsFormat = @"
			IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{2}' AND TABLE_NAME = '{3}')
			BEGIN
				CREATE TABLE {0}.{1} (
					[RoomId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
					[DisplayName] [nvarchar](50) NOT NULL UNIQUE
				);
			INSERT INTO {0}.{1} ([DisplayName]) VALUES ('Default');
			END
		";

		/// <summary>
		/// Creates all of the tables needed to manage AkkaChat users
		/// </summary>
		/// <param name="connectionString">The connection string we want to use</param>
		/// <param name="schemaName">The name of the scehma (can be null)</param>
		/// <param name="tableName">Th name of the table (cannot be null)</param>
		public static void CreateSqlServerUserTables(string connectionString, string schemaName, string tableName)
		{
			var sql = InitUsersSql(tableName, schemaName);
			ExecuteSql(connectionString, sql);
		}

		/// <summary>
		/// Creates all of the tables needed to manage AkkaChat chat rooms
		/// </summary>
		/// <param name="connectionString">The connection string we want to use</param>
		/// <param name="schemaName">The name of the scehma (can be null)</param>
		/// <param name="tableName">Th name of the table (cannot be null)</param>
		public static void CreateSqlServerChatRoomTables(string connectionString, string schemaName, string tableName)
		{
			var sql = InitRoomsSql(tableName, schemaName);
			ExecuteSql(connectionString, sql);
			
		}

		internal static string InitUsersSql(string tableName, string schemaName = null)
		{
			if(string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");
			schemaName = schemaName ?? "dbo";

			var cb = new SqlCommandBuilder();
			return string.Format(SqlUserFormat, cb.QuoteIdentifier(schemaName), cb.QuoteIdentifier(tableName),
				cb.UnquoteIdentifier(schemaName), cb.UnquoteIdentifier(tableName));
		}

		internal static string InitRoomsSql(string tableName, string schemaName = null)
		{
			if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");
			schemaName = schemaName ?? "dbo";

			var cb = new SqlCommandBuilder();
			return string.Format(SqlRoomsFormat, cb.QuoteIdentifier(schemaName), cb.QuoteIdentifier(tableName),
				cb.UnquoteIdentifier(schemaName), cb.UnquoteIdentifier(tableName));
		}

		internal static void ExecuteSql(string connectionString, string sql)
		{
			using (var conn = new SqlConnection(connectionString))
			using (var command = conn.CreateCommand())
			{
				conn.Open();

				command.CommandText = sql;
				command.ExecuteNonQuery();
			}
		}
	}
}
