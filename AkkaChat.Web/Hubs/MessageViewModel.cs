using AkkaChat.Messages;

namespace AkkaChat.Hubs
{
    public class MessageViewModel
    {
        public MessageViewModel(ChatMessage message)
        {
            Id = message.Id;
            Content = message.Message;
            WhenStr = message.When.ToShortTimeString();
            WhenTicks = message.When.Ticks; //for sorting purposes
            IsSystemMessage = message is SystemMessage;
            if (!IsSystemMessage)
            {
                User = new UserViewModel(message.UserName);
            }
        }

        public bool IsSystemMessage { get; set; }

        public string Id { get; set; }

        public string Content { get; set; }

        public string WhenStr { get; set; }

        public long WhenTicks { get; set; }

        public UserViewModel User { get; set; }
    }
}