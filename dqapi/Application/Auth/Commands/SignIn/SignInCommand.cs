using dqapi.Infrastructure.DTOs.Auth;
using MediatR;

namespace dqapi.Application.Auth.Commands.SignIn
{
    public class SignInCommand : IRequest<SignInResponse>
    {
        public SignInRequest RequestParams { get; }
        public SignInCommand(SignInRequest request)
        {
            RequestParams = request;
        }
    }
}
