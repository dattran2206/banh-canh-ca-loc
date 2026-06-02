using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BanhCanhCaLoc.Api.Data;
using BanhCanhCaLoc.Api.Models;
using BanhCanhCaLoc.Api.Services;

namespace BanhCanhCaLoc.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BanhCanhCaLocDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(BanhCanhCaLocDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public class LoginDto
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null || !user.IsActive)
            {
                return BadRequest(new { message = "Tài khoản không tồn tại hoặc đã bị khóa" });
            }

            bool match = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!match)
            {
                return BadRequest(new { message = "Mật khẩu không đúng" });
            }

            var token = _tokenService.GenerateToken(user);

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Action = "login",
                Detail = "Đăng nhập",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token,
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

        [Authorize]
        [HttpPost("shift/start")]
        public async Task<IActionResult> StartShift()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            // Check if there is an active shift for this user
            var activeShift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.UserId == userId && s.EndTime == null);

            if (activeShift != null)
            {
                return BadRequest(new { message = "Bạn đang có ca làm việc chưa kết thúc", shiftId = activeShift.Id });
            }

            var shift = new Shift
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartTime = DateTime.UtcNow,
                EndTime = null,
                TotalRevenue = 0,
                TotalBills = 0
            };

            _context.Shifts.Add(shift);

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "start_shift",
                Detail = "Bắt đầu ca làm việc",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(shift);
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

            var activeShift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.UserId == userId && s.EndTime == null);

            if (activeShift == null)
            {
                return BadRequest(new { message = "Không tìm thấy ca làm việc đang hoạt động" });
            }

            // Calculate revenue and bills for this shift
            var shiftPayments = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.Order!.ShiftId == activeShift.Id)
                .ToListAsync();

            activeShift.EndTime = DateTime.UtcNow;
            activeShift.TotalRevenue = shiftPayments.Sum(p => p.TotalAmount);
            activeShift.TotalBills = shiftPayments.Count;

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "end_shift",
                Detail = "Kết thúc ca làm việc",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(activeShift);
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

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var activeShift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.UserId == userId && s.EndTime == null);

            return Ok(new
            {
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Role,
                    user.FullName,
                    user.IsActive
                },
            });
        }

        [Authorize]
        [HttpGet("shifts")]
        public async Task<IActionResult> GetShifts()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            IQueryable<Shift> query = _context.Shifts.Include(s => s.User);

            if (roleClaim != "admin")
            {
                var userId = Guid.Parse(userIdClaim);
                query = query.Where(s => s.UserId == userId);
            }

            var shifts = await query
                .OrderByDescending(s => s.StartTime)
                .Take(50)
                .ToListAsync();

            return Ok(shifts.Select(s => new {
                s.Id,
                s.UserId,
                User = s.User != null ? new { s.User.Id, s.User.FullName } : null,
                s.StartTime,
                s.EndTime,
                s.TotalRevenue,
                s.TotalBills
            }));
        }
    }
}
