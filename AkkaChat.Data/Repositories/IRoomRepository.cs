using System;
using System.Threading.Tasks;
using AkkaChat.Data.Models;

namespace AkkaChat.Data.Repositories
{
    /// <summary>
    /// Repository for looking up supported chatrooms
    /// </summary>
    public interface IRoomRepository : IDisposable
    {
        /// <summary>
        /// Determine if an existing chatroom exists
        /// </summary>
        Task<RepositoryResult<bool>> RoomExists(string displayName);

        /// <summary>
        /// Fetch an existing chatroom from the database
        /// </summary>
        Task<RepositoryResult<ExistingRoom>> FetchRoom(string displayName);
    }
}
