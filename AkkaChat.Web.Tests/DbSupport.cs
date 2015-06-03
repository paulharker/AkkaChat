using System;
using System.Configuration;
using System.Data.SqlClient;
using AkkaChat.Data;

namespace AkkaChat.Web.Tests
{
    /// <summary>
    /// Used for cleaning up our test database
    /// </summary>
    public static class DbSupport
    {
        public static readonly string ConnectionString;

        public static readonly SqlQueries Queries;

        static DbSupport()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["akkaChat"].ConnectionString;
            Queries = new SqlQueries("dbo", "Users", "Rooms");
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);
        }

        public static void Initialize()
        {
            SqlServerInitializer.CreateSqlServerChatRoomTables(ConnectionString, null, "Rooms");
            SqlServerInitializer.CreateSqlServerUserTables(ConnectionString, null, "Users");
        }

        public static void Clean()
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand())
            {
                conn.Open();
                cmd.CommandText = @"
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Users') BEGIN DELETE FROM dbo.Users END;
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Rooms') BEGIN DELETE FROM dbo.Rooms END";
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
