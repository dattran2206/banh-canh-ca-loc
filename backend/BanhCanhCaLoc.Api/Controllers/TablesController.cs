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
    public class TablesController : ControllerBase
    {
        private readonly BanhCanhCaLocDbContext _context;

        public TablesController(BanhCanhCaLocDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTables()
        {
            var tables = await _context.Tables.Include(t => t.Area).ToListAsync();
            return Ok(tables);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> AddTable([FromBody] Table table)
        {
            var existingTable = await _context.Tables.FirstOrDefaultAsync(t => t.Number == table.Number);
            if (existingTable != null)
            {
                return BadRequest(new { message = $"Số bàn {table.Number} đã tồn tại" });
            }

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();
            return Ok(table);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(int id, [FromBody] Table table)
        {
            if (id != table.Id) return BadRequest();

            var existing = await _context.Tables.FindAsync(id);
            if (existing == null) return NotFound();

            var duplicate = await _context.Tables.AnyAsync(t => t.Number == table.Number && t.Id != id);
            if (duplicate)
            {
                return BadRequest(new { message = $"Số bàn {table.Number} đã tồn tại" });
            }

            existing.Number = table.Number;
            existing.AreaId = table.AreaId;
            existing.Capacity = table.Capacity;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return NotFound();

            // Check if there are active orders for this table
            var activeOrders = await _context.Orders.AnyAsync(o => o.TableId == id && o.Status != "paid");
            if (activeOrders)
            {
                return BadRequest(new { message = "Bàn ăn đang có đơn hàng chưa hoàn tất thanh toán" });
            }

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpGet("areas")]
        public async Task<IActionResult> GetAreas()
        {
            var areas = await _context.Areas.ToListAsync();
            return Ok(areas);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("areas")]
        public async Task<IActionResult> AddArea([FromBody] Area area)
        {
            _context.Areas.Add(area);
            await _context.SaveChangesAsync();
            return Ok(area);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("areas/{id}")]
        public async Task<IActionResult> UpdateArea(int id, [FromBody] Area area)
        {
            if (id != area.Id) return BadRequest();
            var existing = await _context.Areas.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = area.Name;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("areas/{id}")]
        public async Task<IActionResult> DeleteArea(int id)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null) return NotFound();

            // Check if there are tables linked to this area
            var hasTables = await _context.Tables.AnyAsync(t => t.AreaId == id);
            if (hasTables)
            {
                return BadRequest(new { message = "Khu vực đang chứa bàn ăn. Vui lòng di dời hoặc xóa bàn ăn trước." });
            }

            _context.Areas.Remove(area);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
