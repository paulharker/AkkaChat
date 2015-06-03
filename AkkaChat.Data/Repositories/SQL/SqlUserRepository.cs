using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AkkaChat.Data.Models;
using Dapper;

namespace AkkaChat.Data.Repositories.SQL
{
    /// <summary>
    /// SQL Server implementation of the <see cref="IUserRepository"/>
    /// </summary>
    public class SqlUserRepository : IUserRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly SqlQueries _queries;

        public SqlUserRepository(SqlQueries queries, IDbConnection dbConnection)
        {
            _queries = queries;
            _dbConnection = dbConnection;
        }

        public RepositoryResult CreateUser(User newUser)
        {
            try
            {
                var result = _dbConnection.Execute(_queries.CreateUserSql, newUser);
                return new RepositoryResult(ResultCode.Success);
            }
            catch (SqlException ex) { return new RepositoryResult(ResultCode.Failure, ex.Message);}
            
        }

        public async Task<RepositoryResult<bool>> UserNameIsAvailable(string userName)
        {
            try
            {
                var result = (await _dbConnection.QueryAsync<ExistingUser>(_queries.GetUserByNameByNameSql, new {DisplayName = userName})).ToList();
                return result.Count == 0 ? new RepositoryResult<bool>(ResultCode.Success, true) 
                    : new RepositoryResult<bool>(ResultCode.Success, false);
            }
            catch (SqlException ex) { return new RepositoryResult<bool>(ResultCode.Failure, false, ex.Message); }
        }

        public async Task<RepositoryResult<ExistingUser>> FetchUser(string userName)
        {
            try
            {
                var result = (await _dbConnection.QueryAsync<ExistingUser>(_queries.GetUserByNameByNameSql, new { DisplayName = userName })).ToList();
                return result.Count == 0 ? new RepositoryResult<ExistingUser>(ResultCode.Success, new MissingUser()) 
                    : new RepositoryResult<ExistingUser>(ResultCode.Success, result.First());
            }
            catch (SqlException ex) { return new RepositoryResult<ExistingUser>(ResultCode.Failure, new MissingUser(), ex.Message); }
        }

        public async Task<RepositoryResult<ExistingUser>> FetchUser(Guid cookieId)
        {
            try
            {
                var result = (await _dbConnection.QueryAsync<ExistingUser>(_queries.GetUserByCookieIdSql, new { CookieId = cookieId})).ToList();
                return result.Count == 0 ? new RepositoryResult<ExistingUser>(ResultCode.Success, new MissingUser())
                    : new RepositoryResult<ExistingUser>(ResultCode.Success, result.First());
            }
            catch (SqlException ex) { return new RepositoryResult<ExistingUser>(ResultCode.Failure, new MissingUser(), ex.Message); }
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