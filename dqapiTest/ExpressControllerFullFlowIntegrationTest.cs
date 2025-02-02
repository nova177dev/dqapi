using System.Net.Http.Json;
using dqapi.Infrastructure.DTOs.Auth;
using dqapi.Infrastructure.DTOs.Express;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using dqapi.Domain.Entities.Static.Auth;
using System.Net.Http.Headers;
using System.Text.Json;

namespace dqapi.IntegrationTests
{
    public class ExpressControllerIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        private const string InvitationToken = "e9139a69-163e-4e41-82b8-3a006ac5c191";
        private const string SignUpUrl = "/api/auth/signUp/";
        private const string SignInUrl = "/api/auth/signIn/";
        private const string EntityUrl = "/api/express/poi/";

        public ExpressControllerIntegrationTest(WebApplicationFactory<Program> factory)
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

            // > Test Sign In
            var signInResponseObj = await SignInAsync(email, password);
            ValidateResponse(signInResponseObj, StatusCodes.Status201Created);

            // Set up security context for further steps
            string authToken = signInResponseObj.Data!.AuthToken!;
            string userUuid = signInResponseObj.Data!.UserUuid!;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // > Test Create Entity
            var createEntityRequestParams = new ExpressRequest
            {
                TraceUuid = Guid.NewGuid().ToString(),
                AuthToken = authToken,
                Data = JsonDocument.Parse($@"
                {{
                    ""code"": ""{Guid.NewGuid().ToString()}"",
                    ""name"": ""{Guid.NewGuid().ToString()}"",
                    ""address"": ""{Guid.NewGuid().ToString()}"",
                    ""description"": ""{Guid.NewGuid().ToString()}"",
                    ""latitude"": 25.276987,
                    ""longitude"": 55.296249
                }}").RootElement
            };

            var createEntityResponseObj = await CreateEntity(createEntityRequestParams);
            ValidateResponse(createEntityResponseObj, StatusCodes.Status201Created);

            // Set up Entity UUID for further steps
            string uuid = createEntityResponseObj.Uuid!;

            // > Test Read Entity
            // Act & Assert
            var readEntityResponseObj = await GetEntity(uuid);
            ValidateResponse(createEntityResponseObj, StatusCodes.Status201Created);

            // > Test Update Entity
            var updateEntityRequestParams = new ExpressRequest
            {
                TraceUuid = Guid.NewGuid().ToString(),
                AuthToken = authToken,
                Data = JsonDocument.Parse($@"
                {{
                    ""code"": ""{Guid.NewGuid().ToString()}"",
                    ""name"": ""{Guid.NewGuid().ToString()}"",
                    ""address"": ""{Guid.NewGuid().ToString()}"",
                    ""description"": ""{Guid.NewGuid().ToString()}"",
                    ""latitude"": 25.276987,
                    ""longitude"": 55.296249
                }}").RootElement
            };

            var updateEntityResponseObj = await UpdateEntity(uuid, updateEntityRequestParams);
            ValidateResponse(updateEntityResponseObj, StatusCodes.Status200OK);

            // > Test Delete Entity
            // Act & Assert
            var deleteEntityResponseObj = await DeleteEntity(uuid);
            ValidateResponse(deleteEntityResponseObj, StatusCodes.Status200OK);
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

        private async Task<ExpressResponse> CreateEntity(ExpressRequest requestParams)
        {
            var response = await _client.PostAsJsonAsync(EntityUrl, requestParams);

            return (await response.Content.ReadFromJsonAsync<ExpressResponse>())!;
        }

        private async Task<ExpressResponse> GetEntity(string uuid)
        {
            var response = await _client.GetAsync(EntityUrl + uuid);

            return (await response.Content.ReadFromJsonAsync<ExpressResponse>())!;
        }

        private async Task<ExpressResponse> UpdateEntity(string uuid, ExpressRequest requestParams)
        {
            var response = await _client.PutAsJsonAsync(EntityUrl + uuid, requestParams);

            return (await response.Content.ReadFromJsonAsync<ExpressResponse>())!;
        }

        private async Task<ExpressResponse> DeleteEntity(string uuid)
        {
            var response = await _client.DeleteAsync(EntityUrl + uuid);

            return (await response.Content.ReadFromJsonAsync<ExpressResponse>())!;
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