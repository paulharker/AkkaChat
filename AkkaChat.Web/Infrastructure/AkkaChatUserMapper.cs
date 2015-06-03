using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaChat.Actors;
using Microsoft.AspNet.SignalR;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;

namespace AkkaChat.Infrastructure
{
    /// <summary>
    /// Used to retrieve information about the user from their identity
    /// </summary>
    public class AkkaChatUserMapper : IUserMapper, IUserIdProvider
    {
        private readonly IActorRef _sessionManager;
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(1.5);

        public AkkaChatUserMapper(IActorRef sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            return GetUserFromIdentifier(identifier);
        }

        private IUserIdentity GetUserFromIdentifier(Guid identifier)
        {
            /*
             * Going to treat session lookup errors as "could not authenticate" messages.
             * If you love your web developers, you'd add logging here ;)
             */
            try
            {
                var task = _sessionManager.Ask<object>(new UserSessionActor.CheckSession(identifier), DefaultTimeout);
                task.Wait(); //can't use async / await here due to Nancy API constraints
                var userIdentity = task.Result as UserSessionActor.ValidSession;
                return userIdentity == null ? null : userIdentity.Identity;
            }
            catch (AggregateException ex)
            {
                ex.Handle(e => true);
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Task<ClaimsPrincipal> GetClaimsPrincial(Guid identifier)
        {
            var iuser = GetUserFromIdentifier(identifier);
            return iuser == null ? null : Task.FromResult(new ClaimsPrincipal(new GenericIdentity(iuser.UserName)));
        }

        public string GetUserId(IRequest request)
        {
            return request.User.Identity.Name;
        }
    }
}