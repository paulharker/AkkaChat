namespace AkkaChat.Data.Repositories
{
    /// <summary>
    /// Status codes returned by a repository
    /// </summary>
    public enum ResultCode
    {
        Success = 0,
        Failure = 1,
        Timeout = 4
    }
}