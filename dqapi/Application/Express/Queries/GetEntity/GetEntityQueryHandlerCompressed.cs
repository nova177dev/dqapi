using dqapi.Application.Common;
using dqapi.Infrastructure.Data;
using dqapi.Infrastructure.DTOs.Express;
using MediatR;
using System.Text.Json;

namespace dqapi.Application.Express.Queries
{
    public class GetEntityCompressedQueryHandler : IRequestHandler<GetEntityCompressedQuery, byte[]>
    {
        private readonly AppLogger _appLogger;
        private readonly AuthHelper _authHelper;
        private readonly AppDbDataContext _dbDataContext;
        private readonly JsonHelper _jsonHelper;


        public GetEntityCompressedQueryHandler(AppLogger appLogger, AuthHelper authHelper, AppDbDataContext dbDataContext, JsonHelper jsonHelper)
        {
            _appLogger = appLogger;
            _authHelper = authHelper;
            _dbDataContext = dbDataContext;
            _jsonHelper = jsonHelper;
        }

        public Task<byte[]> Handle(GetEntityCompressedQuery request, CancellationToken cancellationToken)
        {
            const string Schema = "r"; // CRUD > Read

            ExpressRequest requestParams = new()
            {
                AuthToken = _authHelper.GetAuthorizationToken(),
                Uuid = request.EntityUuid,
                Compress = true
            };

            try
            {
                byte[] dbResponse = _dbDataContext.requestDb(Schema, request.EntityName, requestParams);
                return Task.FromResult(dbResponse);
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex);
                return Task.FromResult(Array.Empty<byte>());
            }
        }
    }
}
