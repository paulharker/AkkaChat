using System.Data.SqlClient;

namespace AkkaChat.Data
{
    /// <summary>
    /// Defines all of our CRUD SQL queries
    /// </summary>
    public class SqlQueries
    {
        private readonly string _schemaName;
        private readonly string _usersTableName;
        private readonly string _roomsTableName;

        public SqlQueries(string schemaName, string usersTableName, string roomsTableName)
        {
            _schemaName = schemaName;
            _usersTableName = usersTableName;
            _roomsTableName = roomsTableName;
        }

        private string _createUserFormatted = null;

        public string CreateUserSql
        {
            get
            {
                if (!string.IsNullOrEmpty(_createUserFormatted)) return _createUserFormatted;

                var cb = new SqlCommandBuilder();
                _createUserFormatted = string.Format(CreateUserFormat, cb.QuoteIdentifier(_schemaName),
                    cb.QuoteIdentifier(_usersTableName));
                return _createUserFormatted;
            }
            
        }

        private static readonly string CreateUserFormat = @"
            INSERT INTO {0}.{1}
           ([DisplayName]
           ,[Email]
           ,[CookieId]
           ,[PasswordHash]
           ,[PasswordSalt]
           ,[DateJoined])
     VALUES
           (@DisplayName
           ,@Email
           ,@CookieId
           ,@PasswordHash
           ,@PasswordSalt
           ,@DateJoined)";

        #region GET User by Name

        private string _getUserByNameFormatted = null;

        public string GetUserByNameByNameSql
        {
            get
            {
                if (!string.IsNullOrEmpty(_getUserByNameFormatted)) return _getUserByNameFormatted;

                var cb = new SqlCommandBuilder();
                _getUserByNameFormatted = string.Format(GetUserFormat, cb.QuoteIdentifier(_schemaName),
                    cb.QuoteIdentifier(_usersTableName));
                return _getUserByNameFormatted;
            }
        }

        private static readonly string GetUserFormat = @"SELECT TOP 1 * FROM {0}.{1} WHERE lower(DisplayName) = lower(@DisplayName)";

        #endregion

        #region GET User by CookieID

        private string _getUserByCookieIdFormatted = null;

        public string GetUserByCookieIdSql
        {
            get
            {
                if (!string.IsNullOrEmpty(_getUserByCookieIdFormatted)) return _getUserByCookieIdFormatted;

                var cb = new SqlCommandBuilder();
                _getUserByCookieIdFormatted = string.Format(GetUserByCookieIdFormat, cb.QuoteIdentifier(_schemaName),
                    cb.QuoteIdentifier(_usersTableName));
                return _getUserByCookieIdFormatted;
            }
        }

        private static readonly string GetUserByCookieIdFormat = @"SELECT TOP 1 * FROM {0}.{1} WHERE CookieId = @CookieId";
        
        #endregion

        #region GET room

        private string _getRoomFormatted = null;
        public string GetRoomByNameSql
        {
            get
            {
                if (!string.IsNullOrEmpty(_getRoomFormatted)) return _getRoomFormatted;

                var cb = new SqlCommandBuilder();
                _getRoomFormatted = string.Format(GetRoomFormat, cb.QuoteIdentifier(_schemaName),
                    cb.QuoteIdentifier(_roomsTableName));
                return _getRoomFormatted;
            }
        }

        private static readonly string GetRoomFormat = @"SELECT TOP 1 * FROM {0}.{1} WHERE lower(DisplayName) = lower(@DisplayName)";

        #endregion


    }
}
