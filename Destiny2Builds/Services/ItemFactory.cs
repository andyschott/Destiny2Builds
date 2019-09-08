using System;
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
        private readonly IManifest _manifest;
        private readonly BungieSettings _bungie;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ISocketFactory _socketFactory;
        private readonly IStatFactory _statFactory;

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

        public ItemFactory(IDestiny2 destiny2, IManifest manifest, IOptions<BungieSettings> bungie,
            IHttpContextAccessor contextAccessor, ISocketFactory socketFactory,
            IStatFactory statFactory)
        {
            _destiny2 = destiny2;
            _manifest = manifest;
            _bungie = bungie.Value;
            _contextAccessor = contextAccessor;
            _socketFactory = socketFactory;
            _statFactory = statFactory;
        }
        
        public async Task<IEnumerable<Item>> LoadItems(IEnumerable<DestinyItemComponent> itemComponents,
            IDictionary<long, DestinyItemInstanceComponent> itemInstances,
            IDictionary<long, DestinyItemStatsComponent> itemStats)
        {
            var buckets = new Dictionary<long, DestinyInventoryBucketDefinition>();

            var items = new List<Item>();
            foreach(var itemComponent in itemComponents)
            {
                var itemDef = await _manifest.LoadInventoryItem(itemComponent.ItemHash);
                if(!buckets.TryGetValue(itemDef.Inventory.BucketTypeHash, out var bucket))
                {
                    bucket = await _manifest.LoadBucket(itemDef.Inventory.BucketTypeHash);
                    buckets.Add(bucket.Hash, bucket);
                }

                if(!ShouldInclude(bucket))
                {
                    continue;
                }

                itemInstances.TryGetValue(itemComponent.ItemInstanceId, out var instance);

                itemStats.TryGetValue(itemComponent.ItemInstanceId, out var statsComponent);
                var stats = await _statFactory.LoadStats(instance.PrimaryStat, statsComponent?.Stats);

                items.Add(new Item(_bungie.BaseUrl, itemDef, bucket, itemComponent.ItemInstanceId, instance, stats));
            }

            return items;
        }

        public async Task<Item> LoadItem(BungieMembershipType type, long accountId, uint itemHash, long instanceId)
        {
            var itemTask = GetItemDefinition(itemHash);
            var instanceTask = GetInstance(type, accountId, instanceId);

            await Task.WhenAll(itemTask, instanceTask);
            
            var sockets = await _socketFactory.LoadSockets(itemTask.Result.item.Sockets,
                instanceTask.Result.sockets);

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
            var itemDef = await _manifest.LoadInventoryItem(itemHash);
            var bucket = await _manifest.LoadBucket(itemDef.Inventory.BucketTypeHash);

            return (itemDef, bucket);
        }

        private async Task<(DestinyItemInstanceComponent instance, IEnumerable<Stat> stats, IEnumerable<DestinyItemSocketState> sockets)> GetInstance(BungieMembershipType type, long accountId, long instanceId)
        {
            var accessToken = await _contextAccessor.HttpContext.GetTokenAsync("access_token");
            var instance = await _destiny2.GetItem(accessToken, type, accountId, instanceId,
                DestinyComponentType.ItemInstances, DestinyComponentType.ItemSockets,
                DestinyComponentType.ItemStats);

            // var socketsTask = _socketFactory.LoadSockets(instance.Sockets?.Data.Sockets);
            var statsTask = _statFactory.LoadStats(instance.Instance.Data.PrimaryStat, instance.Stats?.Data.Stats);

            await Task.WhenAll(/*socketsTask, */statsTask);

            return (instance.Instance.Data, statsTask.Result, instance.Sockets?.Data.Sockets);
        }
    }
}