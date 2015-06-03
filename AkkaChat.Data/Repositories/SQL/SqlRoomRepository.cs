using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AkkaChat.Data.Models;
using Dapper;

namespace AkkaChat.Data.Repositories.SQL
{
    /// <summary>
    /// Dapper-powered SQL repository for chat rooms
    /// </summary>
    public class SqlRoomRepository : IRoomRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly SqlQueries _queries;

        public SqlRoomRepository(SqlQueries queries, IDbConnection dbConnection)
        {
            _queries = queries;
            _dbConnection = dbConnection;
        }

        public async Task<RepositoryResult<bool>> RoomExists(string displayName)
        {
            try
            {
                var result = (await _dbConnection.QueryAsync<ExistingRoom>(_queries.GetRoomByNameSql, new { DisplayName = displayName })).ToList();
                return result.Count == 0 ? new RepositoryResult<bool>(ResultCode.Success, true)
                    : new RepositoryResult<bool>(ResultCode.Success, false);
            }
            catch (SqlException ex) { return new RepositoryResult<bool>(ResultCode.Failure, false, ex.Message); }
        }

        public async Task<RepositoryResult<ExistingRoom>> FetchRoom(string displayName)
        {
            try
            {
                var result =
                    (await
                        _dbConnection.QueryAsync<ExistingRoom>(_queries.GetRoomByNameSql,
                            new {DisplayName = displayName})).ToList();
                return result.Count == 0
                    ? new RepositoryResult<ExistingRoom>(ResultCode.Success, new MissingRoom())
                    : new RepositoryResult<ExistingRoom>(ResultCode.Success, result.First());
            }
            catch (SqlException ex)
            {
                return new RepositoryResult<ExistingRoom>(ResultCode.Failure, new MissingRoom(), ex.Message);
            }
        }

        public void Dispose()
        {
            try
            {
                _dbConnection.Close();
                _dbConnection.Dispose();
            }
            catch { } //supress errors
        }
    }
}