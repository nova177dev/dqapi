using Swashbuckle.AspNetCore.Annotations;

namespace dqapi.Infrastructure.DTOs.Auth
{
    public class SignUpRequest
    {
        [SwaggerIgnore]
        public string TraceUuid { get; set; }
        public required UserSignUp Data { get; set; }

        public SignUpRequest()
        {
            TraceUuid ??= Guid.NewGuid().ToString();
        }
    }
}
