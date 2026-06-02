using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BanhCanhCaLoc.Domain.Entities;
using BanhCanhCaLoc.Application.Features.Inventory.Queries.GetIngredients;
using BanhCanhCaLoc.Application.Features.Inventory.Commands.AddIngredient;
using BanhCanhCaLoc.Application.Features.Inventory.Commands.UpdateIngredient;
using BanhCanhCaLoc.Application.Features.Inventory.Commands.DeleteIngredient;
using BanhCanhCaLoc.Application.Features.Inventory.Queries.GetStockEntries;
using BanhCanhCaLoc.Application.Features.Inventory.Commands.CreateStockEntry;
using BanhCanhCaLoc.Application.Features.Inventory.Queries.GetWasteRecords;
using BanhCanhCaLoc.Application.Features.Inventory.Commands.CreateWasteRecord;
using BanhCanhCaLoc.Application.Features.Inventory.Queries.GetStockTakes;
using BanhCanhCaLoc.Application.Features.Inventory.Commands.CreateStockTake;

namespace BanhCanhCaLoc.Api.Controllers
{
    [Route("api/[controller]")]
    public class InventoryController : ApiController
    {
        public InventoryController(ISender sender) : base(sender)
        {
        }

        [HttpGet("ingredients")]
        public async Task<IActionResult> GetIngredients()
        {
            var result = await Sender.Send(new GetIngredientsQuery());
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("ingredients")]
        public async Task<IActionResult> AddIngredient([FromBody] Ingredient ingredient)
        {
            var command = new AddIngredientCommand(ingredient.Name, ingredient.Unit, ingredient.MinThreshold, ingredient.CurrentStock);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("ingredients/{id}")]
        public async Task<IActionResult> UpdateIngredient(int id, [FromBody] Ingredient ingredient)
        {
            var command = new UpdateIngredientCommand(id, ingredient.Name, ingredient.Unit, ingredient.MinThreshold, ingredient.CurrentStock);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("ingredients/{id}")]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            var command = new DeleteIngredientCommand(id);
            var result = await Sender.Send(command);
            if (result.IsSuccess)
            {
                return Ok(new { success = true });
            }
            return HandleResult(result);
        }

        [HttpGet("stock-entries")]
        public async Task<IActionResult> GetStockEntries()
        {
            var result = await Sender.Send(new GetStockEntriesQuery());
            return HandleResult(result);
        }

        [Authorize]
        [HttpPost("stock-entries")]
        public async Task<IActionResult> CreateStockEntry([FromBody] StockEntry entry)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var command = new CreateStockEntryCommand(entry.IngredientId, entry.Quantity, entry.UnitPrice, entry.Note, userId);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }

        [HttpGet("waste-records")]
        public async Task<IActionResult> GetWasteRecords()
        {
            var result = await Sender.Send(new GetWasteRecordsQuery());
            return HandleResult(result);
        }

        [Authorize]
        [HttpPost("waste-records")]
        public async Task<IActionResult> CreateWasteRecord([FromBody] WasteRecord record)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var command = new CreateWasteRecordCommand(record.IngredientId, record.Quantity, record.Reason, userId);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }

        [HttpGet("stock-takes")]
        public async Task<IActionResult> GetStockTakes()
        {
            var result = await Sender.Send(new GetStockTakesQuery());
            return HandleResult(result);
        }

        [Authorize]
        [HttpPost("stock-takes")]
        public async Task<IActionResult> CreateStockTake([FromBody] StockTake take)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);

            var command = new CreateStockTakeCommand(take.IngredientId, take.ActualQty, take.Note, userId);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }
    }
}
