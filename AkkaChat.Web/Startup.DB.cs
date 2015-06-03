using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using AkkaChat.Data;

namespace AkkaChat
{
    public partial class Startup
    {
        private static void RunDbSetup()
        {
            /*
             * Initialize SQL
             */
            var connectionString = ConfigurationManager.ConnectionStrings["akkaChat"];
            var schemaName = ConfigurationManager.AppSettings["SqlSchemaName"];
            SqlServerInitializer.CreateSqlServerChatRoomTables(connectionString.ConnectionString, schemaName, ConfigurationManager.AppSettings["ChatTableName"]);
            SqlServerInitializer.CreateSqlServerUserTables(connectionString.ConnectionString, schemaName, ConfigurationManager.AppSettings["UsersTableName"]);
        }
    }
}