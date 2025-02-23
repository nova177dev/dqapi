using dqapi.Application.Common;
using dqapi.Application.Express.Commands.CreateEntity;
using dqapi.Infrastructure.Data;
using MediatR;


namespace dqapi.Application.Express.Commands.CreateEntityCompressed
{
    public class CreateEntityCompressedHandler : IRequestHandler<CreateEntityCompressedCommand, byte[]>
    {
        private readonly AppLogger _appLogger;
        private readonly AuthHelper _authHelper;
        private readonly AppDbDataContext _dbDataContext;
        private readonly JsonHelper _jsonHelper;
        public CreateEntityCompressedHandler(AppLogger appLogger, AuthHelper authHelper, AppDbDataContext dbDataContext, JsonHelper jsonHelper)
        {
            _appLogger = appLogger;
            _authHelper = authHelper;
            _dbDataContext = dbDataContext;
            _jsonHelper = jsonHelper;
        }
        public Task<byte[]> Handle(CreateEntityCompressedCommand request, CancellationToken cancellationToken)
        {
            const string Schema = "c"; // CRUD > Create
            request.RequestParams.AuthToken = _authHelper.GetAuthorizationToken();
            request.RequestParams.Compress = true;
            byte[] dbResponse;

            try
            {
                dbResponse = _dbDataContext.requestDb(Schema, request.EntityName, request.RequestParams);
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
