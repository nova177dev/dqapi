using dqapi.Domain.Entities.Static.Auth;
using Swashbuckle.AspNetCore.Annotations;

namespace dqapi.Infrastructure.DTOs.Auth
{
    public class SessionRequest
    {
        [SwaggerIgnore]
        public string TraceUuid { get; set; }
        public Session? Data { get; set; }
        public SessionRequest()
        {
            TraceUuid ??= Guid.NewGuid().ToString();
        }
    }
}
