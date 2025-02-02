using dqapi.Infrastructure.DTOs.Express;
using MediatR;

namespace dqapi.Application.Express.Commands.CreateEntity
{
    public class CreateEntityCommand : IRequest<ExpressResponse>
    {
        public string EntityName { get; set; }
        public ExpressRequest RequestParams { get; }
        public CreateEntityCommand(string entityName, ExpressRequest request)
        {
            EntityName = entityName;
            RequestParams = request;
        }
    }
}
