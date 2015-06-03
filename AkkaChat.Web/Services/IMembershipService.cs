using System.Threading.Tasks;
using AkkaChat.Messages;

namespace AkkaChat.Services
{
    /// <summary>
    /// Membership provider
    /// </summary>
    public interface IMembershipService
    {
        Task<UserIdentity> AddUser(string userName, string email, string password);

        Task<bool> IsUserNameAvailable(string userName);

        bool TryAuthenticateUser(string userName, string password, out AuthenticationSuccess user);

        Task<UserIdentity> FetchUser(string userName);
    }
}
