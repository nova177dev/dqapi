using dqapi.Domain.Entities.Static.Auth;
using Swashbuckle.AspNetCore.Annotations;

namespace dqapi.Infrastructure.DTOs.Auth
{
    public class SignOutRequest
    {
        [SwaggerIgnore]
        public string TraceUuid { get; set; }
        public Session? Data { get; set; }
        public SignOutRequest()
        {
            TraceUuid ??= Guid.NewGuid().ToString();
        }
    }
}
