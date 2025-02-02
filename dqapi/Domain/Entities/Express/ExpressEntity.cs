using System.Text.Json;

namespace dqapi.Domain.Entities.Express
{
    public class ExpressEntity
    {
        public JsonElement? Config { get; set; }
        public JsonElement? Data { get; set; }
    }
}
