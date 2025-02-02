using dqapi.Domain.Entities.Common;
using dqapi.Infrastructure.DTOs.Auth;
using dqapi.Infrastructure.DTOs.Express;
using Microsoft.AspNetCore.Mvc;

namespace dqapi.Application.Common
{
    public class ResponseHandler
    {
        public ActionResult<T> HandleResponse<T>(T entity) where T : class
        {
            var responseCodeProperty = typeof(T).GetProperty("ResponseCode");
            var traceUuidProperty = typeof(T).GetProperty("TraceUuid");
            var responseMessageProperty = typeof(T).GetProperty("ResponseMessage");

            if (responseCodeProperty == null || traceUuidProperty == null || responseMessageProperty == null)
            {
                return new BadRequestObjectResult(new ErrorMessage { ResponseCode = StatusCodes.Status400BadRequest, ResponseMessage = "Invalid response entity" });
            }

            int responseCode = (int)(responseCodeProperty.GetValue(entity) ?? StatusCodes.Status400BadRequest);
            string traceUuid = (string)(traceUuidProperty.GetValue(entity) ?? string.Empty);
            string responseMessage = (string)(responseMessageProperty.GetValue(entity) ?? "Bad Request");

            return responseCode switch
            {
                StatusCodes.Status200OK => new OkObjectResult(entity),
                StatusCodes.Status201Created => new CreatedResult("", entity),
                StatusCodes.Status401Unauthorized => new UnauthorizedObjectResult(new ErrorMessage { TraceUuid = traceUuid, ResponseCode = StatusCodes.Status401Unauthorized, ResponseMessage = "Unauthorized" }),
                StatusCodes.Status404NotFound => new NotFoundObjectResult(new ErrorMessage { TraceUuid = traceUuid, ResponseCode = StatusCodes.Status404NotFound, ResponseMessage = "Not Found" }),
                _ => new BadRequestObjectResult(new ErrorMessage { TraceUuid = traceUuid, ResponseCode = StatusCodes.Status400BadRequest, ResponseMessage = responseMessage })
            };
        }

        public ActionResult<ExpressResponse> HandleResponse(ExpressResponse entity)
        {
            return HandleResponse<ExpressResponse>(entity);
        }

        public ActionResult<SignUpResponse> HandleResponse(SignUpResponse entity)
        {
            return HandleResponse<SignUpResponse>(entity);
        }

        public ActionResult<SignInResponse> HandleResponse(SignInResponse entity)
        {
            return HandleResponse<SignInResponse>(entity);
        }

        public ActionResult<SignOutResponse> HandleResponse(SignOutResponse entity)
        {
            return HandleResponse<SignOutResponse>(entity);
        }
    }
}