using dqapi.Domain.Entities.Static.Auth;
using Microsoft.IdentityModel.Tokens;

namespace dqapi.Infrastructure.DTOs.Auth
{
    public class SignInRequestDB
    {
        public required string Login { get; set; }
    }
}
