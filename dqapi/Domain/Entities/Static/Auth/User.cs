namespace dqapi.Domain.Entities.Static.Auth
{
    public class User
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public string Title { get; set; }
        public User()
        {
            Title ??= "";
        }
    }
}
