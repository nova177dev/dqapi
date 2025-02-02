using Microsoft.AspNetCore.Mvc;
using MediatR;
using dqapi.Application.Express.Queries.GetEntity;
using dqapi.Domain.Entities.Common;
using dqapi.Application.Express.Commands.CreateEntity;
using System.ComponentModel.DataAnnotations;
using dqapi.Application.Express.Commands.DeleteEntity;
using dqapi.Application.Common;
using dqapi.Infrastructure.DTOs.Express;
using Microsoft.AspNetCore.Authorization;

namespace dqapi.WebApi.Controllers.Express
{
    [Authorize]
    [ApiController]
    [Route("api/express")]
    public class ExpressController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ResponseHandler _responseHandler;
        public ExpressController(IMediator mediator, ResponseHandler responseHandler)
        {
            _mediator = mediator;
            _responseHandler = responseHandler;
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
            return _responseHandler.HandleResponse(await _mediator.Send(new DeleteEntityCommand(entityName, entityUuid)));
        }
    }
}