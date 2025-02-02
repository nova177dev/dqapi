using dqapi.Domain.Entities.Static.Auth;

namespace dqapi.Infrastructure.DTOs.Auth
{
    public class UserSignUpDB : User
    {
        public required string InvitationToken { get; set; }
        public string? PasswordHash { get; set; }
        public string? PasswordSalt { get; set; }
    }
}
