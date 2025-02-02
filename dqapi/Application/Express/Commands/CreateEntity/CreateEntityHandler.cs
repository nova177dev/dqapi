using dqapi.Application.Common;
using dqapi.Application.Express.Commands.CreateEntity;
using dqapi.Infrastructure.Data;
using dqapi.Infrastructure.DTOs.Express;
using MediatR;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Text.Json;

namespace dqapi.Application.Express.Commands.SignUp
{
    public class CreateEntityHandler : IRequestHandler<CreateEntityCommand, ExpressResponse>
    {
        private readonly AppLogger _appLogger;
        private readonly AuthHelper _authHelper;
        private readonly AppDbDataContext _dbDataContext;
        private readonly JsonHelper _jsonHelper;
        public CreateEntityHandler(AppLogger appLogger, AuthHelper authHelper, AppDbDataContext dbDataContext, JsonHelper jsonHelper)
        {
            _appLogger = appLogger;
            _authHelper = authHelper;
            _dbDataContext = dbDataContext;
            _jsonHelper = jsonHelper;
        }
        public Task<ExpressResponse> Handle(CreateEntityCommand request, CancellationToken cancellationToken)
        {
            const string Schema = "c"; // CRUD > Create
            request.RequestParams.AuthToken = _authHelper.GetAuthorizationToken();

            try
            {
                JsonElement dbResponse = _dbDataContext.requestDbForJson(Schema, request.EntityName, request.RequestParams);
                ExpressResponse response = _jsonHelper.DeserializeJson<ExpressResponse>(dbResponse) ?? throw new ArgumentNullException("Response Validation Failed");

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
