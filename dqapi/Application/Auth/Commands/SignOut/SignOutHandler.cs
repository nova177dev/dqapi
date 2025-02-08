using dqapi.Application.Common;
using dqapi.Infrastructure.Data;
using dqapi.Infrastructure.DTOs.Auth;
using MediatR;
using System.Text.Json;

namespace dqapi.Application.Auth.Commands.SignUp
{
    public class SignOutHandler : IRequestHandler<SignOutCommand, SignOutResponse>
    {
        private readonly AppLogger _appLogger;
        private readonly AppDbDataContext _dbDataContext;
        private readonly JsonHelper _jsonHelper;

        public SignOutHandler(AppLogger appLogger, AppDbDataContext dbDataContext, JsonHelper jsonHelper)
        {
            _appLogger = appLogger;
            _dbDataContext = dbDataContext;
            _jsonHelper = jsonHelper;
        }

        public Task<SignOutResponse> Handle(SignOutCommand request, CancellationToken cancellationToken)
        {
            const string Schema = "c"; // Crud > Create
            const string EntityName = "signOut";

            try
            {
                JsonElement dbResponse = _dbDataContext.requestDbForJson(Schema, EntityName, request.RequestParams);
                SignOutResponse response = _jsonHelper.DeserializeJson<SignOutResponse>(dbResponse) ?? throw new ArgumentNullException(nameof(dbResponse), "Response Validation Failed");

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex);
                return Task.FromResult(CreateErrorResponse(request.RequestParams.TraceUuid, "Validation Failed"));
            }
        }

        private SignOutResponse CreateErrorResponse(string traceUuid, string message)
        {
            return new SignOutResponse
            {
                TraceUuid = traceUuid,
                ResponseCode = StatusCodes.Status400BadRequest,
                ResponseMessage = message
            };
        }
    }
}
