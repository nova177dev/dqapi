using dqapi.Domain.Entities.Express;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace dqapi.Infrastructure.DTOs.Express
{
    public class ExpressRequest : ExpressEntity
    {
        [SwaggerIgnore]
        public string TraceUuid { get; set; }
        [SwaggerIgnore]
        public string AuthToken { get; set; }
        [SwaggerIgnore]
        public bool Compress { get; set; }
        public string Uuid { get; set; }

        public ExpressRequest()
        {
            TraceUuid ??= Guid.NewGuid().ToString();
            AuthToken ??= string.Empty;
            Uuid ??= string.Empty;
        }
    }
}
