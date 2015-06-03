namespace AkkaChat.Hubs
{
    public class UserViewModel
    {
        public UserViewModel(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; set; }

    }
}