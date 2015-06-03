namespace AkkaChat.Data.Models
{
    public class Room
    {
        public string DisplayName { get; set; }
    }

    public class ExistingRoom : Room
    {
        public int Id { get; set; }

        protected bool Equals(ExistingRoom other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ExistingRoom) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }

    public class MissingRoom : ExistingRoom { }
}
