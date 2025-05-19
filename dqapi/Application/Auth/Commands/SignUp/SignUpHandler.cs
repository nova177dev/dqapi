using dqapi.Application.Common;
using dqapi.Infrastructure.Data;
using dqapi.Infrastructure.DTOs.Auth;
using MediatR;
using System.Text.Json;

namespace dqapi.Application.Auth.Commands.SignUp
{
    public class SignUpHandler : IRequestHandler<SignUpCommand, SignUpResponse>
    {
        private readonly AppLogger _appLogger;
        private readonly AuthHelper _authHelper;
        private readonly AppDbDataContext _dbDataContext;
        private readonly JsonHelper _jsonHelper;
        public SignUpHandler(AppLogger appLogger, AuthHelper authHelper, AppDbDataContext dbDataContext, JsonHelper jsonHelper)
        {
            _appLogger = appLogger;
            _authHelper = authHelper;
            _dbDataContext = dbDataContext;
            _jsonHelper = jsonHelper;
        }
        public Task<SignUpResponse> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            const string SCHEMA = "c"; // Crud > Create
            const string ENTITY_NAME = "signUp";
            try
            {
                byte[] passwordSalt = _authHelper.GetPasswordSalt();

                SignUpRequestDB requestDb = new()
                {
                    TraceUuid = request.RequestParams.TraceUuid,
                    Data = new UserSignUpDB()
                    {
                        Email = request.RequestParams.Data.Email,
                        FullName = request.RequestParams.Data.FullName,
                        Title = request.RequestParams.Data.Title,
                        PasswordHash = Convert.ToBase64String(_authHelper.GetPasswordHash(request.RequestParams.Data.Password, passwordSalt)),
                        PasswordSalt = Convert.ToBase64String(passwordSalt),
                        InvitationToken = request.RequestParams.Data.InvitationToken
                    }

                };

                JsonElement dbResponse = _dbDataContext.RequestDbForJson(SCHEMA, ENTITY_NAME, requestDb);
                SignUpResponse response = _jsonHelper.DeserializeJson<SignUpResponse>(dbResponse) ?? throw new ArgumentNullException(nameof(dbResponse), "Response Validation Failed");

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex);

                return Task.FromResult(new SignUpResponse { TraceUuid = request.RequestParams.TraceUuid, ResponseCode = StatusCodes.Status400BadRequest, ResponseMessage = "Validation Failed" });
            }
        }
    }
}
