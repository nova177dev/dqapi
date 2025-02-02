using dqapi.Domain.Entities.Static.Auth;

namespace dqapi.Infrastructure.DTOs.Auth
{
    public class SignOutResponse
    {
        public required string TraceUuid { get; set; }
        public required int ResponseCode { get; set; }
        public required string ResponseMessage { get; set; }
    }
}
