using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BanhCanhCaLoc.Domain.Entities;
using BanhCanhCaLoc.Application.Features.Shop.Queries.GetShopInfo;
using BanhCanhCaLoc.Application.Features.Shop.Commands.UpdateShopInfo;

namespace BanhCanhCaLoc.Api.Controllers
{
    [Route("api/[controller]")]
    public class ShopController : ApiController
    {
        public ShopController(ISender sender) : base(sender)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetShopInfo()
        {
            var result = await Sender.Send(new GetShopInfoQuery());
            return HandleResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateShopInfo([FromBody] ShopInfo info)
        {
            var command = new UpdateShopInfoCommand(info.Name, info.Address, info.Phone);
            var result = await Sender.Send(command);
            return HandleResult(result);
        }
    }
}
