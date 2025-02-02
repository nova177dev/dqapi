using dqapi.Infrastructure.DTOs.Express;
using MediatR;

namespace dqapi.Application.Express.Commands.DeleteEntity
{
    public class DeleteEntityCommand : IRequest<ExpressResponse>
    {
        public string EntityName { get; }
        public string EntityUuid { get; }

        public DeleteEntityCommand(string entityName, string entityUuid)
        {
            EntityName = entityName;
            EntityUuid = entityUuid;
        }
    }
}

