using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BanhCanhCaLoc.Domain.Entities;
using BanhCanhCaLoc.Application.Features.Tables.Queries.GetTables;
using BanhCanhCaLoc.Application.Features.Tables.Commands.AddTable;
using BanhCanhCaLoc.Application.Features.Tables.Commands.UpdateTable;
using BanhCanhCaLoc.Application.Features.Tables.Commands.DeleteTable;
using BanhCanhCaLoc.Application.Features.Tables.Queries.GetAreas;
using BanhCanhCaLoc.Application.Features.Tables.Commands.AddArea;
using BanhCanhCaLoc.Application.Features.Tables.Commands.UpdateArea;
using BanhCanhCaLoc.Application.Features.Tables.Commands.DeleteArea;

namespace BanhCanhCaLoc.Api.Controllers
{
    [Route("api/[controller]")]
    public class TablesController : ApiController
    {
        public TablesController(ISender sender) : base(sender)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetTables()
        {
            var result = await Sender.Send(new GetTablesQuery());
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> AddTable([FromBody] Table table)
        {
            var command = new AddTableCommand(table.Number, table.AreaId, table.Capacity);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(int id, [FromBody] Table table)
        {
            var command = new UpdateTableCommand(id, table.Number, table.AreaId, table.Capacity);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var command = new DeleteTableCommand(id);
            var result = await Sender.Send(command);
            if (result.IsSuccess)
            {
                return Ok(new { success = true });
            }
            return HandleResult(result);
        }

        [HttpGet("areas")]
        public async Task<IActionResult> GetAreas()
        {
            var result = await Sender.Send(new GetAreasQuery());
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("areas")]
        public async Task<IActionResult> AddArea([FromBody] Area area)
        {
            var command = new AddAreaCommand(area.Name);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("areas/{id}")]
        public async Task<IActionResult> UpdateArea(int id, [FromBody] Area area)
        {
            var command = new UpdateAreaCommand(id, area.Name);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("areas/{id}")]
        public async Task<IActionResult> DeleteArea(int id)
        {
            var command = new DeleteAreaCommand(id);
            var result = await Sender.Send(command);
            if (result.IsSuccess)
            {
                return Ok(new { success = true });
            }
            return HandleResult(result);
        }
    }
}
