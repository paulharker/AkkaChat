namespace AkkaChat.ViewModels
{
    /// <summary>
    /// DTO for creating a new user
    /// </summary>
    public class NewUser
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}