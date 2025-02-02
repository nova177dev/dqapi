using dqapi.Application.Common;
using dqapi.Domain.Entities.Static.Auth;
using dqapi.Infrastructure.Data;
using dqapi.Infrastructure.DTOs.Auth;
using MediatR;

namespace dqapi.Application.Auth.Commands.SignUp
{
    public class SignInHandler : IRequestHandler<SignInCommand, SignInResponse>
    {
        private readonly AppLogger _appLogger;
        private readonly AuthHelper _authHelper;
        private readonly AppDbDataContext _dbDataContext;
        private readonly JsonHelper _jsonHelper;

        public SignInHandler(AppLogger appLogger, AuthHelper authHelper, AppDbDataContext dbDataContext, JsonHelper jsonHelper)
        {
            _appLogger = appLogger;
            _authHelper = authHelper;
            _dbDataContext = dbDataContext;
            _jsonHelper = jsonHelper;
        }

        public Task<SignInResponse> Handle(SignInCommand request, CancellationToken cancellationToken)
        {
            const string Schema = "c"; // Crud > Create
            const string SignInEntityName = "signIn";
            const string SessionEntityName = "session";
            try
            {
                var requestDb = new SignInRequestDB
                {
                    Login = request.RequestParams.Data.Login
                };

                var dbResponse = _dbDataContext.requestDbForJson(Schema, SignInEntityName, requestDb);

                if (dbResponse.GetProperty("responseCode").GetInt32() != StatusCodes.Status200OK)
                {
                    return Task.FromResult(CreateErrorResponse(request.RequestParams.TraceUuid, "Unauthorized"));
                }

                var data = dbResponse.GetProperty("data");
                var passwordHash = data.GetProperty("passwordHash").GetString() ?? throw new ArgumentNullException("Password Hash Validation Failed");
                var passwordSalt = data.GetProperty("passwordSalt").GetString() ?? throw new ArgumentNullException("Password Salt Validation Failed");
                var userUuid = data.GetProperty("userUuid").GetString() ?? throw new ArgumentNullException("User Uuid Validation Failed");

                if (passwordHash != Convert.ToBase64String(_authHelper.GetPasswordHash(request.RequestParams.Data.Password, Convert.FromBase64String(passwordSalt))))
                {
                    return Task.FromResult(CreateErrorResponse(request.RequestParams.TraceUuid, "Invalid Login or Password"));
                }

                var sessionRequest = new SessionRequest
                {
                    TraceUuid = request.RequestParams.TraceUuid,
                    Data = new Session()
                    {
                        UserUuid = userUuid,
                        AuthToken = _authHelper.CreateToken(userUuid)
                    }
                };

                var sessionResponse = _dbDataContext.requestDbForJson(Schema, SessionEntityName, sessionRequest);
                var sessionData = _jsonHelper.DeserializeJson<SessionResponse>(sessionResponse)?.Data ?? throw new ArgumentNullException("Response Validation Failed");

                return Task.FromResult(new SignInResponse
                {
                    TraceUuid = request.RequestParams.TraceUuid,
                    ResponseCode = StatusCodes.Status201Created,
                    ResponseMessage = "Created",
                    Data = sessionData
                });
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex);
                return Task.FromResult(CreateErrorResponse(request.RequestParams.TraceUuid, "Validation Failed"));
            }
        }

        private SignInResponse CreateErrorResponse(string traceUuid, string message)
        {
            return new SignInResponse
            {
                TraceUuid = traceUuid,
                ResponseCode = StatusCodes.Status401Unauthorized,
                ResponseMessage = message
            };
        }
    }
}
