namespace AkkaChat.Data.Repositories
{
    /// <summary>
    /// Status result returned by a repository
    /// </summary>
    public class RepositoryResult
    {
        public RepositoryResult(ResultCode status, string message = null)
        {
            Message = message;
            Status = status;
        }

        public ResultCode Status { get; private set; }

        public string Message { get; private set; }
    }

    /// <summary>
    /// Typed repository result
    /// </summary>
    /// <typeparam name="T">The type of objects we're returning back to our domain</typeparam>
    public class RepositoryResult<T> : RepositoryResult
    {
        public RepositoryResult(ResultCode status, T payload, string message = null) : base(status, message)
        {
            Payload = payload;
        }

        public T Payload { get; private set; }
    }
}