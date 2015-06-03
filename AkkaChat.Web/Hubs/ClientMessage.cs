namespace AkkaChat.Hubs
{
    public class ClientMessage
    {
        public string Id { get; set; }

        public string Content { get; set; }

        public string RoomName { get; set; }

        public string UserName { get; set; }
    }

    public static class ClientMessageExtensions
    {
        public static bool IsValid(this ClientMessage message)
        {
            return message != null &&
                   !string.IsNullOrEmpty(message.Content)
                   && !string.IsNullOrWhiteSpace(message.Content)
                   && !string.IsNullOrEmpty(message.RoomName)
                   && !string.IsNullOrWhiteSpace(message.RoomName)
                   && !string.IsNullOrEmpty(message.UserName)
                   && !string.IsNullOrWhiteSpace(message.UserName);
        }
    }
}