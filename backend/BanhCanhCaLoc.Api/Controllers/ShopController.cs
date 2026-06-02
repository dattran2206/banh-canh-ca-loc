using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BanhCanhCaLoc.Api.Data;
using BanhCanhCaLoc.Api.Models;

namespace BanhCanhCaLoc.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopController : ControllerBase
    {
        private readonly BanhCanhCaLocDbContext _context;

        public ShopController(BanhCanhCaLocDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetShopInfo()
        {
            var info = await _context.ShopInfos.FirstOrDefaultAsync();
            if (info == null) return NotFound();
            return Ok(info);
        }

        [Authorize(Roles = "admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateShopInfo([FromBody] ShopInfo info)
        {
            var existing = await _context.ShopInfos.FirstOrDefaultAsync();
            if (existing == null)
            {
                _context.ShopInfos.Add(info);
            }
            else
            {
                existing.Name = info.Name;
                existing.Address = info.Address;
                existing.Phone = info.Phone;
            }

            await _context.SaveChangesAsync();
            return Ok(existing ?? info);
        }
    }
}
