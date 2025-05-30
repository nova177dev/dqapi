﻿using dqapi.Application.Common;
using dqapi.Domain.Entities.Static.Auth;
using dqapi.Infrastructure.Data;
using dqapi.Infrastructure.DTOs.Auth;
using MediatR;

namespace dqapi.Application.Auth.Commands.SignIn
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
            const string SCHEMA = "c"; // Crud > Create
            const string SIGN_IN_ENTITY_NAME = "signIn";
            const string SESSION_ENTITY_NAME = "session";
            try
            {
                var requestDb = new SignInRequestDB
                {
                    Login = request.RequestParams.Data.Login
                };

                var dbResponse = _dbDataContext.RequestDbForJson(SCHEMA, SIGN_IN_ENTITY_NAME, requestDb);

                if (dbResponse.GetProperty("responseCode").GetInt32() != StatusCodes.Status200OK)
                {
                    return Task.FromResult(CreateErrorResponse(request.RequestParams.TraceUuid, "Unauthorized"));
                }

                var data = dbResponse.GetProperty("data");
                var passwordHash = data.GetProperty("passwordHash").GetString() ?? throw new ArgumentNullException(nameof(data), "Password Hash Validation Failed");
                var passwordSalt = data.GetProperty("passwordSalt").GetString() ?? throw new ArgumentNullException(nameof(data), "Password Salt Validation Failed");
                var userUuid = data.GetProperty("userUuid").GetString() ?? throw new ArgumentNullException(nameof(data), "User Uuid Validation Failed");

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

                var sessionResponse = _dbDataContext.RequestDbForJson(SCHEMA, SESSION_ENTITY_NAME, sessionRequest);
                var sessionData = _jsonHelper.DeserializeJson<SessionResponse>(sessionResponse)?.Data ?? throw new ArgumentNullException(nameof(data), "Response Validation Failed");

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
