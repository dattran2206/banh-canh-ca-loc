using MediatR;
using Microsoft.AspNetCore.Mvc;
using BanhCanhCaLoc.Application.Common.Models;

namespace BanhCanhCaLoc.Api.Controllers
{
    [ApiController]
    public abstract class ApiController : ControllerBase
    {
        protected readonly ISender Sender;

        protected ApiController(ISender sender)
        {
            Sender = sender;
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
            {
                return Ok();
            }

            return MatchError(result.Error);
        }

        protected IActionResult HandleResult<TValue>(Result<TValue> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return MatchError(result.Error);
        }

        private IActionResult MatchError(Error error)
        {
            return error.Code switch
            {
                var code when code.EndsWith(".NotFound") => NotFound(new { message = error.Message }),
                var code when code.EndsWith(".Unauthorized") => Unauthorized(new { message = error.Message }),
                _ => BadRequest(new { message = error.Message })
            };
        }
    }
}
