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
    public class MenuController : ControllerBase
    {
        private readonly BanhCanhCaLocDbContext _context;

        public MenuController(BanhCanhCaLocDbContext context)
        {
            _context = context;
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("categories")]
        public async Task<IActionResult> AddCategory([FromBody] Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            if (id != category.Id) return BadRequest();
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = category.Name;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            // Check if there are menu items in this category
            var hasItems = await _context.MenuItems.AnyAsync(m => m.CategoryId == id);
            if (hasItems)
            {
                return BadRequest(new { message = "Danh mục đang chứa món ăn, không thể xóa." });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetMenu()
        {
            var menu = await _context.MenuItems.Include(m => m.Category).ToListAsync();
            return Ok(menu);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> AddMenuItem([FromBody] MenuItem item)
        {
            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] MenuItem item)
        {
            if (id != item.Id) return BadRequest();

            var existing = await _context.MenuItems.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = item.Name;
            existing.CategoryId = item.CategoryId;
            existing.Price = item.Price;
            existing.Description = item.Description;
            existing.IsAvailable = item.IsAvailable;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return NotFound();

            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpGet("recipes/{menuItemId}")]
        public async Task<IActionResult> GetRecipes(int menuItemId)
        {
            var recipes = await _context.RecipeItems
                .Include(r => r.Ingredient)
                .Where(r => r.MenuItemId == menuItemId)
                .ToListAsync();
            return Ok(recipes);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("recipes")]
        public async Task<IActionResult> SaveRecipeItem([FromBody] RecipeItem item)
        {
            var existing = await _context.RecipeItems
                .FirstOrDefaultAsync(r => r.MenuItemId == item.MenuItemId && r.IngredientId == item.IngredientId);

            if (existing != null)
            {
                existing.Quantity = item.Quantity;
                existing.YieldPercent = item.YieldPercent;
            }
            else
            {
                _context.RecipeItems.Add(item);
            }

            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("recipes/{menuItemId}/{ingredientId}")]
        public async Task<IActionResult> DeleteRecipeItem(int menuItemId, int ingredientId)
        {
            var item = await _context.RecipeItems
                .FirstOrDefaultAsync(r => r.MenuItemId == menuItemId && r.IngredientId == ingredientId);

            if (item == null) return NotFound();

            _context.RecipeItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
