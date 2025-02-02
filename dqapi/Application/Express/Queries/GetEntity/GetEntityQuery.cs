using dqapi.Infrastructure.DTOs.Express;
using MediatR;

namespace dqapi.Application.Express.Queries.GetEntity
{
    public class GetEntityQuery : IRequest<ExpressResponse>
    {
        public string EntityName { get; }
        public string EntityUuid { get; }

        public GetEntityQuery(string entityName, string entityUuid)
        {
            EntityName = entityName;
            EntityUuid = entityUuid;
        }
    }
}

