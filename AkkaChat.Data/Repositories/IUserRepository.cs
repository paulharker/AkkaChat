using System;
using System.Threading.Tasks;
using AkkaChat.Data.Models;

namespace AkkaChat.Data.Repositories
{
    /// <summary>
    /// Repository for looking up <see cref="User"/> entities
    /// </summary>
    public interface IUserRepository : IDisposable
    {
        /// <summary>
        /// Create a new <see cref="User"/>
        /// </summary>
        RepositoryResult CreateUser(User newUser);

        /// <summary>
        /// Determine if an existing username is available.
        /// </summary>
        Task<RepositoryResult<bool>> UserNameIsAvailable(string userName);

        /// <summary>
        /// Fetch an existing user from the database
        /// </summary>
        Task<RepositoryResult<ExistingUser>> FetchUser(string userName);

        /// <summary>
        /// Fetch an existing user from the database
        /// </summary>
        Task<RepositoryResult<ExistingUser>> FetchUser(Guid cookieId);
    }
}
