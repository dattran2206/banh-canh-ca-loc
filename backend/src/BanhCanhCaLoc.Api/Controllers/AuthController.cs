using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BanhCanhCaLoc.Contracts.Auth;
using BanhCanhCaLoc.Application.Features.Auth.Commands.Login;
using BanhCanhCaLoc.Application.Features.Auth.Commands.StartShift;
using BanhCanhCaLoc.Application.Features.Auth.Commands.EndShift;
using BanhCanhCaLoc.Application.Features.Auth.Queries.GetMe;
using BanhCanhCaLoc.Application.Features.Auth.Queries.GetShifts;

namespace BanhCanhCaLoc.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ApiController
    {
        public AuthController(ISender sender) : base(sender)
        {
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var command = new LoginCommand(request.Username, request.Password);
            var result = await Sender.Send(command);

            if (result.IsSuccess)
            {
                var userDto = new UserResponse(
                    result.Value.User.Id,
                    result.Value.User.Username,
                    result.Value.User.Role,
                    result.Value.User.FullName,
                    result.Value.User.IsActive
                );
                return Ok(new LoginResponse(result.Value.Token, userDto));
            }

            return BadRequest(new { message = result.Error.Message });
        }

        [Authorize]
        [HttpPost("shift/start")]
        public async Task<IActionResult> StartShift()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var command = new StartShiftCommand(userId);
            var result = await Sender.Send(command);

            return HandleResult(result);
        }

        [Authorize]
        [HttpPost("shift/end")]
        public async Task<IActionResult> EndShift()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var command = new EndShiftCommand(userId);
            var result = await Sender.Send(command);

            return HandleResult(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var query = new GetMeQuery(userId);
            var result = await Sender.Send(query);

            if (result.IsSuccess)
            {
                var user = result.Value;
                return Ok(new
                {
                    user = new
                    {
                        user.Id,
                        user.Username,
                        user.Role,
                        user.FullName,
                        user.IsActive
                    }
                });
            }

            return HandleResult(result);
        }

        [Authorize]
        [HttpGet("shifts")]
        public async Task<IActionResult> GetShifts()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var query = new GetShiftsQuery(Guid.Parse(userIdClaim), roleClaim ?? string.Empty);
            var result = await Sender.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value.Select(s => new
                {
                    s.Id,
                    s.UserId,
                    User = s.User != null ? new { s.User.Id, s.User.FullName } : null,
                    s.StartTime,
                    s.EndTime,
                    s.TotalRevenue,
                    s.TotalBills
                }));
            }

            return HandleResult(result);
        }
    }
}
