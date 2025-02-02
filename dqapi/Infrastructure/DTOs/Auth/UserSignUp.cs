using dqapi.Domain.Entities.Static.Auth;

namespace dqapi.Infrastructure.DTOs.Auth
{
    public class UserSignUp : User
    {
        public required string InvitationToken { get; set; }
        public required string Password { get; set; }
    }
}
