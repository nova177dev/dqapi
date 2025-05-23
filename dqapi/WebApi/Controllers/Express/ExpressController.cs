﻿using Microsoft.AspNetCore.Mvc;
using MediatR;
using dqapi.Application.Express.Queries;
using dqapi.Domain.Entities.Common;
using dqapi.Application.Express.Commands.CreateEntity;
using System.ComponentModel.DataAnnotations;
using dqapi.Application.Express.Commands;
using dqapi.Application.Common;
using dqapi.Infrastructure.DTOs.Express;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Net.Http.Headers;


namespace dqapi.WebApi.Controllers.Express
{
    [Authorize]
    [ApiController]
    [EnableRateLimiting("express")]
    [Route("api/express")]
    public class ExpressController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ResponseHandler _responseHandler;
        private readonly IWebHostEnvironment _env;
        public ExpressController(IMediator mediator, ResponseHandler responseHandler, IWebHostEnvironment env)
        {
            _mediator = mediator;
            _responseHandler = responseHandler;
            _env = env;
        }

        /// <summary>
        /// Creates the specified entity.
        /// </summary>
        [HttpPost("{entityName}")]
        [ProducesResponseType(typeof(ExpressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ExpressResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExpressResponse>> CreateEntity(
            [Required][FromBody] ExpressRequest requestParams,
            string entityName)
        {
            if (!IsDevelopment()) return NotAvailableInProduction();
            return _responseHandler.HandleResponse(await _mediator.Send(new CreateEntityCommand(entityName, requestParams)));
        }

        /// <summary>
        /// Returns the specified entity by the provided UUID.
        /// </summary>
        [HttpGet("{entityName}/{entityUuid}")]
        [ProducesResponseType(typeof(ExpressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExpressResponse>> GetEntity(string entityName, string entityUuid)
        {
            if (!IsDevelopment()) return NotAvailableInProduction();
            return _responseHandler.HandleResponse(await _mediator.Send(new GetEntityQuery(entityName, entityUuid)));
        }

        /// <summary>
        /// Creates the specified entity.
        /// </summary>
        [HttpPut("{entityName}/{entityUuid}")]
        [ProducesResponseType(typeof(ExpressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]

        public async Task<ActionResult<ExpressResponse>> UpdateEntity(
            [Required][FromBody] ExpressRequest requestParams,
            string entityName,
            string entityUuid
        )
        {
            if (!IsDevelopment()) return NotAvailableInProduction();
            return _responseHandler.HandleResponse(await _mediator.Send(new UpdateEntityCommand(entityName, entityUuid, requestParams)));
        }

        /// <summary>
        /// Deletes the specified entity by the provided UUID.
        /// </summary>
        [HttpDelete("{entityName}/{entityUuid}")]
        [ProducesResponseType(typeof(ExpressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExpressResponse>> DeleteEntity(string entityName, string entityUuid)
        {
            if (!IsDevelopment()) return NotAvailableInProduction();
            return _responseHandler.HandleResponse(await _mediator.Send(new DeleteEntityCommand(entityName, entityUuid)));
        }

        [HttpPost("{entityName}/{entityUuid}/compressed")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateEntityCompressed(
            [Required][FromBody] ExpressRequest requestParams,
            string entityName)
        {
            if (!IsDevelopment()) return NotFound();
            try
            {
                var result = await _mediator.Send(new CreateEntityCompressedCommand(entityName, requestParams));

                Response.ContentType = "application/json; charset=utf-16";
                Response.Headers.Append("Content-Encoding", "gzip");
                Response.Headers.Vary = HeaderNames.AcceptEncoding;

                return File(result, "application/json; charset=utf-16");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{entityName}/{entityUuid}/compressed")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEntityCompressed(string entityName, string entityUuid)
        {
            if (!IsDevelopment()) return NotFound();
            try
            {
                var result = await _mediator.Send(new GetEntityCompressedQuery(entityName, entityUuid));

                Response.ContentType = "application/json; charset=utf-16";
                Response.Headers.Append("Content-Encoding", "gzip");
                Response.Headers.Vary = HeaderNames.AcceptEncoding;

                return File(result, "application/json; charset=utf-16");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool IsDevelopment()
        {
            return _env.IsDevelopment() || _env.EnvironmentName == "Testing";
        }

        private ActionResult<ExpressResponse> NotAvailableInProduction()
        {
            return NotFound(new ExpressResponse
            {
                TraceUuid = HttpContext.TraceIdentifier,
                ResponseCode = StatusCodes.Status404NotFound,
                ResponseMessage = "This endpoint is not available in the production environment."
            });
        }
    }
}
