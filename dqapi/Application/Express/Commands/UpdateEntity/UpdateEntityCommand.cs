using dqapi.Infrastructure.DTOs.Express;
using MediatR;

namespace dqapi.Application.Express.Commands.CreateEntity
{
    public class UpdateEntityCommand : IRequest<ExpressResponse>
    {
        public string EntityName { get; set; }
        public string EntityUuid { get; set; }
        public ExpressRequest RequestParams { get; }
        public UpdateEntityCommand(string entityName, string entityUuid, ExpressRequest request)
        {
            EntityName = entityName;
            EntityUuid = entityUuid;
            RequestParams = request;
        }
    }
}
