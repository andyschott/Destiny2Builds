using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny2;
using Destiny2.Definitions;
using Destiny2.Entities.Items;
using Destiny2Builds.Helpers;
using Destiny2Builds.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Destiny2Builds.Services
{
    public class ItemFactory : IItemFactory
    {
        private readonly IDestiny2 _destiny2;
        private readonly BungieSettings _bungie;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ISocketFactory _socketFactory;
        private readonly IStatFactory _statFactory;
        private readonly IPerkFactory _perkFactory;
        private readonly IManifestCache _cache;

        private static readonly ISet<ItemSlot.SlotHashes> _includedBuckets =
            new HashSet<ItemSlot.SlotHashes>
            {
                ItemSlot.SlotHashes.Kinetic,
                ItemSlot.SlotHashes.Energy,
                ItemSlot.SlotHashes.Power,
                ItemSlot.SlotHashes.Helmet,
                ItemSlot.SlotHashes.Gauntlet,
                ItemSlot.SlotHashes.ChestArmor,
                ItemSlot.SlotHashes.LegArmor,
                ItemSlot.SlotHashes.ClassArmor,
                ItemSlot.SlotHashes.Ghost,
            };

        public ItemFactory(IDestiny2 destiny2, IOptions<BungieSettings> bungie,
            IHttpContextAccessor contextAccessor, ISocketFactory socketFactory,
            IStatFactory statFactory, IPerkFactory perkFactory, IManifestCache cache)
        {
            _destiny2 = destiny2;
            _bungie = bungie.Value;
            _contextAccessor = contextAccessor;
            _socketFactory = socketFactory;
            _statFactory = statFactory;
            _perkFactory = perkFactory;
            _cache = cache;
        }
        
        public async Task<IEnumerable<Item>> LoadItems(IEnumerable<DestinyItemComponent> itemComponents,
            IDictionary<long, DestinyItemInstanceComponent> itemInstances,
            IDictionary<long, DestinyItemStatsComponent> itemStats,
            IDictionary<long, DestinyItemSocketsComponent> itemSockets,
            IEnumerable<Mod> mods, IEnumerable<Mod> shaders)
        {
            var tasks = itemComponents.Select(async itemComponent => 
            {
                var itemDef = await _cache.GetInventoryItemDef(itemComponent.ItemHash);
                var bucket = await _cache.GetBucketDef(itemDef.Inventory.BucketTypeHash);

                return new { itemComponent, itemDef, bucket };
            });

            var info = await Task.WhenAll(tasks);
            var info2 = info.Where(item => ShouldInclude(item.bucket));

            var itemTasks = info2.Select(async item =>
            {
                itemInstances.TryGetValue(item.itemComponent.ItemInstanceId, out var instance);
                itemStats.TryGetValue(item.itemComponent.ItemInstanceId, out var statsComponent);
                itemSockets.TryGetValue(item.itemComponent.ItemInstanceId, out var socketsComponent);

                var statsTask = _statFactory.LoadStats(instance.PrimaryStat, statsComponent?.Stats);
                var socketsTask = _socketFactory.LoadActiveSockets(item.itemDef.Sockets, socketsComponent.Sockets);

                await Task.WhenAll(statsTask, socketsTask);

                return new Item(_bungie.BaseUrl, item.itemDef, item.bucket, item.itemComponent.ItemInstanceId, instance,
                    statsTask.Result, socketsTask.Result);
            });

            var items = await Task.WhenAll(itemTasks);
            return items;
        }

        public async Task<Item> LoadItem(BungieMembershipType type, long accountId, long characterId,
            uint itemHash, long instanceId)
        {
            var accessToken = await _contextAccessor.HttpContext.GetTokenAsync("access_token");

            var modsTask = LoadAllMods(accessToken, type, accountId);
            var itemTask = GetItemDefinition(itemHash);
            var instanceTask = GetInstance(accessToken, type, accountId, instanceId);

            await Task.WhenAll(modsTask, itemTask, instanceTask);
            
            var sockets = await _socketFactory.LoadSockets(itemTask.Result.item.Sockets,
                instanceTask.Result.sockets, modsTask.Result.mods, modsTask.Result.shaders);

            return new Item(_bungie.BaseUrl, itemTask.Result.item, itemTask.Result.bucket,
                instanceId, instanceTask.Result.instance, instanceTask.Result.stats,
                sockets);
        }

        private static bool ShouldInclude(DestinyInventoryBucketDefinition bucket)
        {
            return _includedBuckets.Contains((ItemSlot.SlotHashes)bucket.Hash);
        }

        private async Task<(DestinyInventoryItemDefinition item, DestinyInventoryBucketDefinition bucket)> GetItemDefinition(uint itemHash)
        {
            var itemDef = await _cache.GetInventoryItemDef(itemHash);
            var bucket = await _cache.GetBucketDef(itemDef.Inventory.BucketTypeHash);

            return (itemDef, bucket);
        }

        private async Task<(DestinyItemInstanceComponent instance, IEnumerable<Stat> stats, IEnumerable<DestinyItemSocketState> sockets)> GetInstance(string accessToken, BungieMembershipType type, long accountId, long instanceId)
        {
            var instance = await _destiny2.GetItem(accessToken, type, accountId, instanceId,
                DestinyComponentType.ItemInstances, DestinyComponentType.ItemSockets,
                DestinyComponentType.ItemStats);

            var stats = await _statFactory.LoadStats(instance.Instance.Data.PrimaryStat, instance.Stats?.Data?.Stats);

            return (instance.Instance.Data, stats, instance.Sockets?.Data.Sockets);
        }

        private async Task<(IEnumerable<Mod> mods, IEnumerable<Mod> shaders)> LoadAllMods(string accessToken, BungieMembershipType type,
            long accountId)
        {
            var inventory = await _destiny2.GetProfile(accessToken, type, accountId,
                DestinyComponentType.ProfileInventories);

            return await _perkFactory.LoadAllMods(inventory.ProfileInventory.Data.Items);
        }
    }
}