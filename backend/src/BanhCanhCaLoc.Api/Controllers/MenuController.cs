using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BanhCanhCaLoc.Domain.Entities;
using BanhCanhCaLoc.Application.Features.Menu.Queries.GetCategories;
using BanhCanhCaLoc.Application.Features.Menu.Commands.AddCategory;
using BanhCanhCaLoc.Application.Features.Menu.Commands.UpdateCategory;
using BanhCanhCaLoc.Application.Features.Menu.Commands.DeleteCategory;
using BanhCanhCaLoc.Application.Features.Menu.Queries.GetMenu;
using BanhCanhCaLoc.Application.Features.Menu.Commands.AddMenuItem;
using BanhCanhCaLoc.Application.Features.Menu.Commands.UpdateMenuItem;
using BanhCanhCaLoc.Application.Features.Menu.Commands.DeleteMenuItem;
using BanhCanhCaLoc.Application.Features.Menu.Queries.GetRecipes;
using BanhCanhCaLoc.Application.Features.Menu.Commands.SaveRecipeItem;
using BanhCanhCaLoc.Application.Features.Menu.Commands.DeleteRecipeItem;

namespace BanhCanhCaLoc.Api.Controllers
{
    [Route("api/[controller]")]
    public class MenuController : ApiController
    {
        public MenuController(ISender sender) : base(sender)
        {
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var result = await Sender.Send(new GetCategoriesQuery());
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("categories")]
        public async Task<IActionResult> AddCategory([FromBody] Category category)
        {
            var command = new AddCategoryCommand(category.Name);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            var command = new UpdateCategoryCommand(id, category.Name);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var command = new DeleteCategoryCommand(id);
            var result = await Sender.Send(command);
            if (result.IsSuccess)
            {
                return Ok(new { success = true });
            }
            return HandleResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMenu()
        {
            var result = await Sender.Send(new GetMenuQuery());
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> AddMenuItem([FromBody] MenuItem item)
        {
            var command = new AddMenuItemCommand(item.Name, item.CategoryId, item.Price, item.Description, item.IsAvailable);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] MenuItem item)
        {
            var command = new UpdateMenuItemCommand(id, item.Name, item.CategoryId, item.Price, item.Description, item.IsAvailable);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var command = new DeleteMenuItemCommand(id);
            var result = await Sender.Send(command);
            if (result.IsSuccess)
            {
                return Ok(new { success = true });
            }
            return HandleResult(result);
        }

        [HttpGet("recipes/{menuItemId}")]
        public async Task<IActionResult> GetRecipes(int menuItemId)
        {
            var result = await Sender.Send(new GetRecipesQuery(menuItemId));
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("recipes")]
        public async Task<IActionResult> SaveRecipeItem([FromBody] RecipeItem item)
        {
            var command = new SaveRecipeItemCommand(item.MenuItemId, item.IngredientId, item.Quantity, item.YieldPercent);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("recipes/{menuItemId}/{ingredientId}")]
        public async Task<IActionResult> DeleteRecipeItem(int menuItemId, int ingredientId)
        {
            var command = new DeleteRecipeItemCommand(menuItemId, ingredientId);
            var result = await Sender.Send(command);
            if (result.IsSuccess)
            {
                return Ok(new { success = true });
            }
            return HandleResult(result);
        }
    }
}
