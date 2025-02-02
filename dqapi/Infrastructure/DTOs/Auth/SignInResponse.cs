using dqapi.Domain.Entities.Static.Auth;

namespace dqapi.Infrastructure.DTOs.Auth
{
    public class SignInResponse
    {
        public required string TraceUuid { get; set; }
        public required int ResponseCode { get; set; }
        public required string ResponseMessage { get; set; }
        public Session? Data { get; set; }
    }
}
