using dqapi.Domain.Entities.Static.Auth;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace dqapi.Infrastructure.DTOs.Auth
{
    public class SignInRequest
    {
        [SwaggerIgnore]
        public string TraceUuid { get; set; }
        public required Credentials Data { get; set; }
        public SignInRequest()
        {
            TraceUuid ??= Guid.NewGuid().ToString();
        }
    }
}
