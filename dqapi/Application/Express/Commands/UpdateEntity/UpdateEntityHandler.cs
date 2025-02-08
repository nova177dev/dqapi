using dqapi.Application.Common;
using dqapi.Application.Express.Commands.CreateEntity;
using dqapi.Infrastructure.Data;
using dqapi.Infrastructure.DTOs.Express;
using MediatR;
using System.Text.Json;

namespace dqapi.Application.Express.Commands.CreteEntity
{
    public class UpdateEntityHandler : IRequestHandler<UpdateEntityCommand, ExpressResponse>
    {
        private readonly AppLogger _appLogger;
        private readonly AuthHelper _authHelper;
        private readonly AppDbDataContext _dbDataContext;
        private readonly JsonHelper _jsonHelper;
        public UpdateEntityHandler(AppLogger appLogger, AuthHelper authHelper, AppDbDataContext dbDataContext, JsonHelper jsonHelper)
        {
            _appLogger = appLogger;
            _authHelper = authHelper;
            _dbDataContext = dbDataContext;
            _jsonHelper = jsonHelper;
        }
        public Task<ExpressResponse> Handle(UpdateEntityCommand request, CancellationToken cancellationToken)
        {
            const string Schema = "u"; // crUd > Update
            request.RequestParams.AuthToken = _authHelper.GetAuthorizationToken();
            request.RequestParams.Uuid = request.EntityUuid;

            try
            {
                JsonElement dbResponse = _dbDataContext.requestDbForJson(Schema, request.EntityName, request.RequestParams);
                ExpressResponse response = _jsonHelper.DeserializeJson<ExpressResponse>(dbResponse) ?? throw new ArgumentNullException(nameof(dbResponse), "Response Validation Failed");

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex);

                return Task.FromResult(new ExpressResponse { TraceUuid = request.RequestParams.TraceUuid, ResponseCode = StatusCodes.Status400BadRequest, ResponseMessage = "Validation Failed" });
            }
        }
    }
}
