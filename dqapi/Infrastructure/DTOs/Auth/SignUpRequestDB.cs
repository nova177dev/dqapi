using dqapi.Domain.Entities.Static.Auth;

namespace dqapi.Infrastructure.DTOs.Auth
{
    public class SignUpRequestDB
    {
        public string TraceUuid { get; set; }
        public required UserSignUpDB Data { get; set; }

        public SignUpRequestDB()
        {
            TraceUuid ??= Guid.NewGuid().ToString();
        }
    }
}
