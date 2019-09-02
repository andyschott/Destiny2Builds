using System.Threading.Tasks;
using Destiny2;
using Destiny2Builds.Services;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("{type}/{accountId}/{itemHash}/{instanceId}", Name="ItemIndex")]
        public async Task<IActionResult> Index(BungieMembershipType type, long accountId, uint itemHash, long instanceId)
        {
            var item = await _itemFactory.LoadItem(type, accountId, itemHash, instanceId);

            // TODO: Load perks and sockets
            
            return View(item);
        }
    }
}