using dqapi.Domain.Entities.Express;
using System.Text.Json.Serialization;

namespace dqapi.Infrastructure.DTOs.Express
{
    public class ExpressResponse : ExpressEntity
    {
        public string? TraceUuid { get; set; }
        public int ResponseCode { get; set; }
        public string? ResponseMessage { get; set; }
        public string? Uuid { get; set; }
    }
}
