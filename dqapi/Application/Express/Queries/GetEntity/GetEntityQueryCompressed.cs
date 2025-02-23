using MediatR;

namespace dqapi.Application.Express.Queries.GetEntityCompressed
{
    public class GetEntityCompressedQuery : IRequest<byte[]>
    {
        public string EntityName { get; }
        public string EntityUuid { get; }

        public GetEntityCompressedQuery(string entityName, string entityUuid)
        {
            EntityName = entityName;
            EntityUuid = entityUuid;
        }
    }
}

