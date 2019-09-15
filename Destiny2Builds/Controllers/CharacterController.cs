using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny2;
using Destiny2Builds.Helpers;
using Destiny2Builds.Models;
using Destiny2Builds.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Destiny2Builds.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class CharacterController : Controller
    {
        private readonly IDestiny2 _destiny2;
        private readonly IManifest _manifest;
        private readonly BungieSettings _bungie;
        private readonly IItemFactory _itemFactory;
        private readonly IPerkFactory _perkFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger _logger;

        public CharacterController(IDestiny2 destiny2, IManifest manifest,
            IOptions<BungieSettings> bungie, IItemFactory itemFactory,
            IPerkFactory perkFactory, IHttpContextAccessor contextAccessor,
            ILogger<CharacterController> logger)
        {
            _destiny2 = destiny2;
            _manifest = manifest;
            _bungie = bungie.Value;
            _itemFactory = itemFactory;
            _perkFactory = perkFactory;
            _contextAccessor = contextAccessor;
            _logger = logger;
        }
        
        [HttpGet("{type}/{id}", Name = "CharacterIndex")]
        public async Task<IActionResult> Index(int type, long id)
        {
            var membershipType = (BungieMembershipType)type;
            _logger.LogInformation($"{membershipType}/{id}");

            var accessToken = _contextAccessor.HttpContext.GetTokenAsync("access_token");

            var model = new CharactersViewModel(membershipType, id);

            var profileResponse = await _destiny2.GetProfile(await accessToken, membershipType, id, DestinyComponentType.Characters);
            if (profileResponse == null)
            {
                var url = Url.RouteUrl("AccountIndex");
                return Redirect(url);
            }

            var classDefTasks = profileResponse.Characters.Data.Select(item => _manifest.LoadClass(item.Value.ClassHash));

            var classDefs = await Task.WhenAll(classDefTasks);
            var characters = profileResponse.Characters.Data.Zip(classDefs,
                (item, classDef) => (id: item.Key, characters: item.Value, classDef: classDef));

            foreach (var (characterId, character, classDef) in characters)
            {
                model.Characters.Add(new Character(characterId, character, classDef, _bungie.BaseUrl));
            }

            return View(model);
        }

        [HttpGet("{type}/{id}/{characterId}", Name="CharacterDetails")]
        public async Task<IActionResult> Details(int type, long id, long characterId)
        {
            var membershipType = (BungieMembershipType)type;
            _logger.LogInformation($"{membershipType}/{id}/{characterId}");

            var accessToken = await _contextAccessor.HttpContext.GetTokenAsync("access_token");

            var info = await _destiny2.GetProfile(accessToken, membershipType, id,
                DestinyComponentType.Characters, DestinyComponentType.CharacterEquipment,
                DestinyComponentType.ItemInstances, DestinyComponentType.ItemStats,
                DestinyComponentType.ItemSockets, DestinyComponentType.ProfileInventories);

            var (mods, shaders) = await _perkFactory.LoadAllMods(info.ProfileInventory.Data.Items);

            var allItems = await _itemFactory.LoadItems(info.CharacterEquipment.Data[characterId].Items,
                info.ItemComponents.Instances.Data,
                info.ItemComponents.Stats.Data,
                info.ItemComponents.Sockets.Data,
                mods, shaders);
            var items = allItems.ToDictionary(item => item.Slot.Hash);
            var stats = GetStats(items.Values);

            var character = info.Characters.Data[characterId];

            var model = new CharacterViewModel()
            {
                Type = membershipType,
                AccountId = id,
                CharacterId = characterId,
                Items = items,
                EmblemPath = _bungie.BaseUrl + character.EmblemPath,
                EmblemBackgroundPath = _bungie.BaseUrl + character.EmblemBackgroundPath,
                Stats = stats,
            };

            return View(model);
        }

        private IDictionary<uint, Stat> GetStats(IEnumerable<Item> items)
        {
            var allStats = items.SelectMany(item => item.Stats)
                .Where(stat => stat.AggregationType == DestinyStatAggregationType.Character);
            var stats = allStats.ToLookup(stat => stat.Hash);

            return stats.ToDictionary(grouping => grouping.Key,
                grouping =>
                {
                    var value = grouping.Sum(stat => stat.Value);
                    var originalStat = grouping.First();
                    return new Stat(originalStat, value);
                });
        }
    }
}