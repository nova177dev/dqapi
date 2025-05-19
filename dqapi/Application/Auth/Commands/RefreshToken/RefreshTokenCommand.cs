using dqapi.Infrastructure.DTOs.Auth;
using MediatR;

namespace dqapi.Application.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<SessionResponse>
    {
        public SessionRequest RequestParams { get; }
        public RefreshTokenCommand(SessionRequest request)
        {
            RequestParams = request;
        }
    }
}
