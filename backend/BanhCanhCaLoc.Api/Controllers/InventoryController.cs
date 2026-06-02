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
    public class InventoryController : ControllerBase
    {
        private readonly BanhCanhCaLocDbContext _context;

        public InventoryController(BanhCanhCaLocDbContext context)
        {
            _context = context;
        }

        [HttpGet("ingredients")]
        public async Task<IActionResult> GetIngredients()
        {
            var ingredients = await _context.Ingredients.ToListAsync();
            return Ok(ingredients);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("ingredients")]
        public async Task<IActionResult> AddIngredient([FromBody] Ingredient ingredient)
        {
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();
            return Ok(ingredient);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("ingredients/{id}")]
        public async Task<IActionResult> UpdateIngredient(int id, [FromBody] Ingredient ingredient)
        {
            if (id != ingredient.Id) return BadRequest();

            var existing = await _context.Ingredients.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = ingredient.Name;
            existing.Unit = ingredient.Unit;
            existing.MinThreshold = ingredient.MinThreshold;
            existing.CurrentStock = ingredient.CurrentStock;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("ingredients/{id}")]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return NotFound();

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpGet("stock-entries")]
        public async Task<IActionResult> GetStockEntries()
        {
            var entries = await _context.StockEntries
                .Include(s => s.Ingredient)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            return Ok(entries);
        }

        [Authorize]
        [HttpPost("stock-entries")]
        public async Task<IActionResult> CreateStockEntry([FromBody] StockEntry entry)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var ingredient = await _context.Ingredients.FindAsync(entry.IngredientId);
            if (ingredient == null) return BadRequest(new { message = "Nguyên liệu không tồn tại" });

            entry.Id = Guid.NewGuid();
            entry.CreatedAt = DateTime.UtcNow;

            _context.StockEntries.Add(entry);
            ingredient.CurrentStock += entry.Quantity;

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "add_stock",
                Detail = $"Nhập kho {entry.Quantity} {ingredient.Unit} {ingredient.Name}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(entry);
        }

        [HttpGet("waste-records")]
        public async Task<IActionResult> GetWasteRecords()
        {
            var records = await _context.WasteRecords
                .Include(w => w.Ingredient)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
            return Ok(records);
        }

        [Authorize]
        [HttpPost("waste-records")]
        public async Task<IActionResult> CreateWasteRecord([FromBody] WasteRecord record)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var ingredient = await _context.Ingredients.FindAsync(record.IngredientId);
            if (ingredient == null) return BadRequest(new { message = "Nguyên liệu không tồn tại" });

            if (ingredient.CurrentStock < record.Quantity)
            {
                return BadRequest(new { message = "Lượng hao hụt vượt quá lượng tồn kho hiện tại" });
            }

            record.Id = Guid.NewGuid();
            record.CreatedAt = DateTime.UtcNow;

            _context.WasteRecords.Add(record);
            ingredient.CurrentStock = Math.Max(0, ingredient.CurrentStock - record.Quantity);

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "waste_stock",
                Detail = $"Hủy kho {record.Quantity} {ingredient.Unit} {ingredient.Name}. Lý do: {record.Reason}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(record);
        }

        [HttpGet("stock-takes")]
        public async Task<IActionResult> GetStockTakes()
        {
            var takes = await _context.StockTakes
                .Include(s => s.Ingredient)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            return Ok(takes);
        }

        [Authorize]
        [HttpPost("stock-takes")]
        public async Task<IActionResult> CreateStockTake([FromBody] StockTake take)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var ingredient = await _context.Ingredients.FindAsync(take.IngredientId);
            if (ingredient == null) return BadRequest(new { message = "Nguyên liệu không tồn tại" });

            take.Id = Guid.NewGuid();
            take.SystemQty = ingredient.CurrentStock;
            take.Difference = take.ActualQty - take.SystemQty;
            take.CreatedAt = DateTime.UtcNow;

            _context.StockTakes.Add(take);
            ingredient.CurrentStock = take.ActualQty;

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "stock_take",
                Detail = $"Kiểm kê {ingredient.Name}. Thực tế: {take.ActualQty}, Lệch: {take.Difference}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(take);
        }
    }
}
