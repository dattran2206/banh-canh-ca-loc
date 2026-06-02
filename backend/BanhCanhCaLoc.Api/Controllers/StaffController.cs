using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BanhCanhCaLoc.Api.Data;
using BanhCanhCaLoc.Api.Models;

namespace BanhCanhCaLoc.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class StaffController : ControllerBase
    {
        private readonly BanhCanhCaLocDbContext _context;

        public StaffController(BanhCanhCaLocDbContext context)
        {
            _context = context;
        }

        public class CreateUserDto
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> GetStaff()
        {
            var staff = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Role,
                    u.FullName,
                    u.IsActive
                })
                .ToListAsync();

            return Ok(staff);
        }

        [HttpPost]
        public async Task<IActionResult> AddStaff([FromBody] CreateUserDto dto)
        {
            var existing = await _context.Users.AnyAsync(u => u.Username == dto.Username);
            if (existing)
            {
                return BadRequest(new { message = "Tên tài khoản đã tồn tại" });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                FullName = dto.FullName,
                IsActive = true
            };

            _context.Users.Add(user);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? currentUserId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Action = "add_user",
                Detail = $"Thêm nhân viên {dto.FullName} ({dto.Role})",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new
            {
                user.Id,
                user.Username,
                user.Role,
                user.FullName,
                user.IsActive
            });
        }

        [HttpPut("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(currentUserIdClaim) && Guid.Parse(currentUserIdClaim) == id)
            {
                return BadRequest(new { message = "Không thể tự khóa tài khoản của chính mình" });
            }

            user.IsActive = !user.IsActive;

            Guid? currentUserId = string.IsNullOrEmpty(currentUserIdClaim) ? null : Guid.Parse(currentUserIdClaim);

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Action = "toggle_active_user",
                Detail = $"{(user.IsActive ? "Mở khóa" : "Khóa")} tài khoản nhân viên {user.FullName}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new
            {
                user.Id,
                user.Username,
                user.Role,
                user.FullName,
                user.IsActive
            });
        }

        public class UpdateUserDto
        {
            public string Username { get; set; } = string.Empty;
            public string? Password { get; set; }
            public string Role { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(Guid id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (user.Username != dto.Username)
            {
                var existing = await _context.Users.AnyAsync(u => u.Username == dto.Username);
                if (existing)
                {
                    return BadRequest(new { message = "Tên tài khoản đã tồn tại" });
                }
            }

            user.Username = dto.Username;
            user.FullName = dto.FullName;
            user.Role = dto.Role;

            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? currentUserId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Action = "edit_user",
                Detail = $"Sửa nhân viên {dto.FullName} ({dto.Role})",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new
            {
                user.Id,
                user.Username,
                user.Role,
                user.FullName,
                user.IsActive
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(currentUserIdClaim) && Guid.Parse(currentUserIdClaim) == id)
            {
                return BadRequest(new { message = "Không thể tự xóa tài khoản của chính mình" });
            }

            _context.Users.Remove(user);

            Guid? currentUserId = string.IsNullOrEmpty(currentUserIdClaim) ? null : Guid.Parse(currentUserIdClaim);

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Action = "delete_user",
                Detail = $"Xóa tài khoản nhân viên {user.FullName}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
