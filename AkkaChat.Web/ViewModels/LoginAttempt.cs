namespace AkkaChat.ViewModels
{
    /// <summary>
    /// DTO for attempting to login
    /// </summary>
    public class LoginAttempt
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}