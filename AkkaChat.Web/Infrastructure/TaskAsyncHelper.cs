using System.Threading.Tasks;

namespace AkkaChat.Infrastructure
{
    internal static class TaskAsyncHelper
    {
        private static readonly Task NoneTask = Task.FromResult<object>(null);
        public static Task None
        {
            get { return NoneTask; }
        }
    }
}