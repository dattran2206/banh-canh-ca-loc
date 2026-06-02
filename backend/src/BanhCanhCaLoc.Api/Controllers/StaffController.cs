using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BanhCanhCaLoc.Application.Features.Staff.Queries.GetStaff;
using BanhCanhCaLoc.Application.Features.Staff.Commands.AddStaff;
using BanhCanhCaLoc.Application.Features.Staff.Commands.ToggleStaffActive;
using BanhCanhCaLoc.Application.Features.Staff.Commands.UpdateStaff;
using BanhCanhCaLoc.Application.Features.Staff.Commands.DeleteStaff;

namespace BanhCanhCaLoc.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class StaffController : ApiController
    {
        public StaffController(ISender sender) : base(sender)
        {
        }

        public class CreateUserDto
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
        }

        public class UpdateUserDto
        {
            public string Username { get; set; } = string.Empty;
            public string? Password { get; set; }
            public string Role { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> GetStaff()
        {
            var result = await Sender.Send(new GetStaffQuery());
            if (result.IsSuccess)
            {
                return Ok(result.Value.Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Role,
                    u.FullName,
                    u.IsActive
                }));
            }
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddStaff([FromBody] CreateUserDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? currentUserId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var command = new AddStaffCommand(dto.Username, dto.Password, dto.Role, dto.FullName, currentUserId);
            var result = await Sender.Send(command);

            if (result.IsSuccess)
            {
                var user = result.Value;
                return Ok(new
                {
                    user.Id,
                    user.Username,
                    user.Role,
                    user.FullName,
                    user.IsActive
                });
            }

            return HandleResult(result);
        }

        [HttpPut("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? currentUserId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var command = new ToggleStaffActiveCommand(id, currentUserId);
            var result = await Sender.Send(command);

            if (result.IsSuccess)
            {
                var user = result.Value;
                return Ok(new
                {
                    user.Id,
                    user.Username,
                    user.Role,
                    user.FullName,
                    user.IsActive
                });
            }

            return HandleResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(Guid id, [FromBody] UpdateUserDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? currentUserId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var command = new UpdateStaffCommand(id, dto.Username, dto.Password, dto.Role, dto.FullName, currentUserId);
            var result = await Sender.Send(command);

            if (result.IsSuccess)
            {
                var user = result.Value;
                return Ok(new
                {
                    user.Id,
                    user.Username,
                    user.Role,
                    user.FullName,
                    user.IsActive
                });
            }

            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? currentUserId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var command = new DeleteStaffCommand(id, currentUserId);
            var result = await Sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(new { success = true });
            }

            return HandleResult(result);
        }
    }
}
