using System.Linq;
using System.Threading.Tasks;
using Destiny2;
using Destiny2Builds.Models;
using Destiny2Builds.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Destiny2Builds.Controllers
{
    [Route("[controller]")]
    public class ItemController : Controller
    {
        private readonly IItemFactory _itemFactory;

        public ItemController(IDestiny2 destiny2, IItemFactory itemFactory)
        {
            _itemFactory = itemFactory;
        }

        [HttpGet("{type}/{accountId}/{characterId}/{itemHash}/{instanceId}", Name="ItemIndex")]
        public async Task<IActionResult> Index(BungieMembershipType type, long accountId,
            long characterId, uint itemHash, long instanceId)
        {
            var item = await _itemFactory.LoadItem(type, accountId, itemHash, instanceId);
            var model = new ItemViewModel
            {
                Type = type,
                AccountId = accountId,
                CharacterId = characterId,
                Item = item,
                Sockets = item.Sockets.Select(socket => new SocketViewModel
                {
                    Perks = socket.Perks.Select(perk => new SelectListItem
                    {
                        Text = perk.Name,
                        Value = perk.Hash.ToString(),
                        Selected = perk.IsSelected,
                    }).ToList()
                }).ToList()
            };         
            return View(model);
        }
    }
}