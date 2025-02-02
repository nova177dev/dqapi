namespace dqapi.Domain.Entities.Static.Auth
{
    public class Session
    {
        public required string UserUuid { get; set; }
        public required string AuthToken { get; set; }
    }
}
