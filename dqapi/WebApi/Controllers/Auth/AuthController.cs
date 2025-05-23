﻿using Microsoft.AspNetCore.Mvc;
using MediatR;
using dqapi.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;
using dqapi.Application.Common;
using dqapi.Infrastructure.DTOs.Auth;
using dqapi.Application.Auth.Commands.SignUp;
using dqapi.Application.Auth.Commands.SignIn;
using dqapi.Application.Auth.Commands.RefreshToken;
using dqapi.Application.Auth.Commands.SignOut;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace dqapi.WebApi.Controllers
{
    /// <summary>
    /// Controller for handling authentication-related operations.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ResponseHandler _responseHandler;
        public AuthController(IMediator mediator, ResponseHandler responseHandler)
        {
            _mediator = mediator;
            _responseHandler = responseHandler;
        }

        /// <summary>
        /// User SignUp.
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting("anonymous")]
        [HttpPost("sign-up")]
        [ProducesResponseType(typeof(SignUpResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SignUpResponse>> SignUp(
            [Required][FromBody] SignUpRequest requestParams)
        {
            return _responseHandler.HandleResponse(await _mediator.Send(new SignUpCommand(requestParams)));
        }

        /// <summary>
        /// User SignIn.
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting("anonymous")]
        [HttpPost("sign-in")]
        [ProducesResponseType(typeof(SignInResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<SignInResponse>> SignIn(
            [Required][FromBody] SignInRequest requestParams)
        {
            return _responseHandler.HandleResponse(await _mediator.Send(new SignInCommand(requestParams)));
        }

        /// <summary>
        /// Current user token Refresh.
        /// </summary>
        [EnableRateLimiting("authenticated")]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SessionResponse>> SignOut(
            [Required][FromBody] SessionRequest requestParams)
        {
            return _responseHandler.HandleResponse(await _mediator.Send(new RefreshTokenCommand(requestParams)));
        }

        /// <summary>
        /// User SignOut.
        /// </summary>
        [EnableRateLimiting("authenticated")]
        [HttpPost("sign-out")]
        [ProducesResponseType(typeof(SignOutResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SignOutResponse>> SignOut(
            [Required][FromBody] SignOutRequest requestParams)
        {
            return _responseHandler.HandleResponse(await _mediator.Send(new SignOutCommand(requestParams)));
        }
    }
}
