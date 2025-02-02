using System.Net.Http.Json;
using dqapi.Infrastructure.DTOs.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using dqapi.Domain.Entities.Static.Auth;
using System.Net.Http.Headers;

namespace dqapi.IntegrationTests
{
    public class AuthControllerFullFlowIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        private const string InvitationToken = "e9139a69-163e-4e41-82b8-3a006ac5c191";
        private const string SignUpUrl = "/api/auth/signUp";
        private const string SignInUrl = "/api/auth/signIn";
        private const string RefreshTokenUrl = "/api/auth/refreshToken";
        private const string SignOutUrl = "/api/auth/signOut";

        public AuthControllerFullFlowIntegrationTest(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task FullFlow()
        {
            // Arrange
            string email = "test@" + Guid.NewGuid().ToString().ToLower() + ".dev";
            string password = Guid.NewGuid().ToString().ToLower();
            string fullName = "Test " + Guid.NewGuid().ToString().ToLower();

            // Act & Assert
            // > Test Sign Up
            var signUpResponseObj = await SignUpAsync(email, password, fullName);
            ValidateResponse(signUpResponseObj, StatusCodes.Status201Created);

            // Test Sign In
            // >> Test Sign In with invalid credentials
            var signInResponseObjFailed = await SignInAsync("invalid@e.mail", "invalidP@ssw0rd");
            ValidateResponse(signInResponseObjFailed, StatusCodes.Status401Unauthorized);

            // >> Test Sign In with valid credentials
            var signInResponseObj = await SignInAsync(email, password);
            ValidateResponse(signInResponseObj, StatusCodes.Status201Created);

            // Set up security context for further steps
            string authToken = signInResponseObj.Data!.AuthToken!;
            string userUuid = signInResponseObj.Data!.UserUuid!;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // > Test Refresh Token
            // >> Test Refresh Token with invalid credentials
            var sessionResponseObjFailed = await RefreshTokenAsync(Guid.NewGuid().ToString(), "");
            ValidateResponse(sessionResponseObjFailed, StatusCodes.Status400BadRequest);

            // >> Test Refresh Token with valid credentials
            var sessionResponseObj = await RefreshTokenAsync(userUuid, authToken);
            ValidateResponse(sessionResponseObj, StatusCodes.Status201Created);

            // > Test Sign Out
            var signOutResponseObjFailed = await SignOutAsync(Guid.NewGuid().ToString(), "");
            ValidateResponse(signOutResponseObjFailed, StatusCodes.Status400BadRequest);

            var signOutResponseObj = await SignOutAsync(userUuid, authToken);
            ValidateResponse(signOutResponseObj, StatusCodes.Status200OK);
        }

        private async Task<SignUpResponse> SignUpAsync(string email, string password, string fullName)
        {
            var signUpRequest = new SignUpRequest
            {
                Data = new UserSignUp
                {
                    InvitationToken = InvitationToken,
                    Email = email,
                    Password = password,
                    FullName = fullName
                }
            };
            var signUpResponse = await _client.PostAsJsonAsync(SignUpUrl, signUpRequest);

            return (await signUpResponse.Content.ReadFromJsonAsync<SignUpResponse>())!;
        }

        private async Task<SignInResponse> SignInAsync(string email, string password)
        {
            var signInRequest = new SignInRequest
            {
                TraceUuid = Guid.NewGuid().ToString(),
                Data = new Credentials
                {
                    Login = email,
                    Password = password
                }
            };
            var signInResponse = await _client.PostAsJsonAsync(SignInUrl, signInRequest);

            return (await signInResponse.Content.ReadFromJsonAsync<SignInResponse>())!;
        }

        private async Task<SessionResponse> RefreshTokenAsync(string userUuid, string authToken)
        {
            var refreshTokenRequest = new SessionRequest
            {
                TraceUuid = Guid.NewGuid().ToString(),
                Data = new Session
                {
                    UserUuid = userUuid,
                    AuthToken = authToken
                }
            };
            var sessionResponse = await _client.PostAsJsonAsync(RefreshTokenUrl, refreshTokenRequest);

            return (await sessionResponse.Content.ReadFromJsonAsync<SessionResponse>())!;
        }

        private async Task<SignOutResponse> SignOutAsync(string userUuid, string authToken)
        {
            var signOutRequest = new SignOutRequest
            {
                TraceUuid = Guid.NewGuid().ToString(),
                Data = new Session
                {
                    UserUuid = userUuid,
                    AuthToken = authToken
                }
            };

            var signOutResponse = await _client.PostAsJsonAsync(SignOutUrl, signOutRequest);

            return (await signOutResponse.Content.ReadFromJsonAsync<SignOutResponse>())!;
        }

        private void ValidateResponse<T>(T responseObj, int expectedStatusCode)
        {
            responseObj.Should().NotBeNull();

            var responseCodeProperty = typeof(T).GetProperty("ResponseCode");
            responseCodeProperty.Should().NotBeNull("ResponseCode property check Failed");
            var responseCodeValue = (int)responseCodeProperty!.GetValue(responseObj)!;
            responseCodeValue.Should().Be(expectedStatusCode);
        }
    }
}