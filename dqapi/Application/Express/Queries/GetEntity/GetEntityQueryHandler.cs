using dqapi.Application.Common;
using dqapi.Infrastructure.Data;
using dqapi.Infrastructure.DTOs.Express;
using MediatR;
using System.Text.Json;

namespace dqapi.Application.Express.Queries.GetEntity
{
    public class DeleteEntityQueryHandler : IRequestHandler<GetEntityQuery, ExpressResponse>
    {
        private readonly AppLogger _appLogger;
        private readonly AuthHelper _authHelper;
        private readonly AppDbDataContext _dbDataContext;
        private readonly JsonHelper _jsonHelper;


        public DeleteEntityQueryHandler(AppLogger appLogger, AuthHelper authHelper, AppDbDataContext dbDataContext, JsonHelper jsonHelper)
        {
            _appLogger = appLogger;
            _authHelper = authHelper;
            _dbDataContext = dbDataContext;
            _jsonHelper = jsonHelper;
        }

        public Task<ExpressResponse> Handle(GetEntityQuery request, CancellationToken cancellationToken)
        {
            const string Schema = "r"; // CRUD > Read

            ExpressRequest requestParams = new()
            {
                AuthToken = _authHelper.GetAuthorizationToken(),
                Uuid = request.EntityUuid
            };

            try
            {
                JsonElement dbResponse = _dbDataContext.requestDbForJson(Schema, request.EntityName, requestParams);
                ExpressResponse response = _jsonHelper.DeserializeJson<ExpressResponse>(dbResponse) ?? throw new ArgumentNullException(nameof(dbResponse), "Response Validation Failed");

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex);

                return Task.FromResult(new ExpressResponse { TraceUuid = requestParams.TraceUuid, ResponseCode = StatusCodes.Status400BadRequest, ResponseMessage = "Validation Failed"});
            }

        }
    }
}
