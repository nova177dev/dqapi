using dqapi.Domain.Entities.Static.Auth;

namespace dqapi.Infrastructure.DTOs.Auth
{
    public class SignUpResponse
    {
        public required string TraceUuid { get; set; }
        public required int ResponseCode { get; set; }
        public required string ResponseMessage { get; set; }
        public string? Uuid { get; set; }
        public User? Data { get; set; }
    }
}
