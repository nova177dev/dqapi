using dqapi.Infrastructure.DTOs.Express;
using MediatR;

namespace dqapi.Application.Express.Commands.CreateEntityCompressed
{
    public class CreateEntityCompressedCommand : IRequest<byte[]>
    {
        public string EntityName { get; set; }
        public ExpressRequest RequestParams { get; }
        public CreateEntityCompressedCommand(string entityName, ExpressRequest request)
        {
            EntityName = entityName;
            RequestParams = request;
        }
    }
}
