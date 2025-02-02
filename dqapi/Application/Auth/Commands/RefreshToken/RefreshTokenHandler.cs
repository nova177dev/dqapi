using dqapi.Application.Common;
using dqapi.Infrastructure.Data;
using dqapi.Infrastructure.DTOs.Auth;
using MediatR;

namespace dqapi.Application.Auth.Commands.SignUp
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, SessionResponse>
    {
        private readonly AppLogger _appLogger;
        private readonly AuthHelper _authHelper;
        private readonly AppDbDataContext _dbDataContext;
        private readonly JsonHelper _jsonHelper;

        public RefreshTokenHandler(AppLogger appLogger, AuthHelper authHelper, AppDbDataContext dbDataContext, JsonHelper jsonHelper)
        {
            _appLogger = appLogger;
            _authHelper = authHelper;
            _dbDataContext = dbDataContext;
            _jsonHelper = jsonHelper;
        }

        public Task<SessionResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            const string Schema = "c"; // Crud > Create
            const string EntityName = "session";

            try
            {
                string userUuid = _authHelper.GetUserUuid();

                var sessionResponse = _dbDataContext.requestDbForJson(Schema, EntityName, request.RequestParams);
                SessionResponse sessionData = _jsonHelper.DeserializeJson<SessionResponse>(sessionResponse) ?? throw new ArgumentNullException(nameof(sessionResponse));

                return Task.FromResult(sessionData);
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex);
                return Task.FromResult(CreateErrorResponse(request.RequestParams.TraceUuid, "Validation Failed"));
            }
        }

        private SessionResponse CreateErrorResponse(string traceUuid, string message)
        {
            return new SessionResponse
            {
                TraceUuid = traceUuid,
                ResponseCode = StatusCodes.Status400BadRequest,
                ResponseMessage = message
            };
        }
    }
}
