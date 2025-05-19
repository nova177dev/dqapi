using dqapi.Infrastructure.DTOs.Auth;
using MediatR;

namespace dqapi.Application.Auth.Commands.SignOut
{
    public class SignOutCommand : IRequest<SignOutResponse>
    {
        public SignOutRequest RequestParams { get; }
        public SignOutCommand(SignOutRequest request)
        {
            RequestParams = request;
        }
    }
}
