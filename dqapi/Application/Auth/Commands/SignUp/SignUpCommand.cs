using dqapi.Infrastructure.DTOs.Auth;
using MediatR;

namespace dqapi.Application.Auth.Commands.SignUp
{
    public class SignUpCommand : IRequest<SignUpResponse>
    {
        public SignUpRequest RequestParams { get; }
        public SignUpCommand(SignUpRequest request)
        {
            RequestParams = request;
        }
    }
}
