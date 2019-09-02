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
            IHttpContextAccessor contextAccessor)
        {
            _destiny2 = destiny2;
            _manifest = manifest;
            _bungie = bungie.Value;
            _contextAccessor = contextAccessor;
        }
        
        public async Task<IEnumerable<Item>> LoadItems(IEnumerable<DestinyItemComponent> itemComponents,
            IDictionary<long, DestinyItemInstanceComponent> itemInstances)
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

                itemInstances.TryGetValue(itemComponent.ItemInstanceId, out DestinyItemInstanceComponent instance);
                items.Add(new Item(_bungie.BaseUrl, itemDef, bucket, itemComponent.ItemInstanceId, instance));
            }

            return items;
        }

        public async Task<Item> LoadItem(BungieMembershipType type, long accountId, uint itemHash, long instanceId)
        {
            var itemTask = GetItemDefinition(itemHash);
            var instanceTask = GetInstance(type, accountId, instanceId);

            await Task.WhenAll(itemTask, instanceTask);

            return new Item(_bungie.BaseUrl, itemTask.Result.item, itemTask.Result.bucket,
                instanceId, instanceTask.Result.instance, instanceTask.Result.perks);
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

        private async Task<(DestinyItemInstanceComponent instance, IEnumerable<Perk> perks)> GetInstance(BungieMembershipType type, long accountId, long instanceId)
        {
            var accessToken = await _contextAccessor.HttpContext.GetTokenAsync("access_token");
            var instance = await _destiny2.GetItem(accessToken, type, accountId, instanceId,
                DestinyComponentType.ItemInstances, DestinyComponentType.ItemSockets);

            IEnumerable<Perk> perks = null;
            var sockets = instance.Sockets?.Data.Sockets;
            if(sockets != null)
            {
                perks = await LoadPerks(sockets);
            }

            return (instance.Instance.Data, perks);
        }

        private async Task<IEnumerable<Perk>> LoadPerks(IEnumerable<DestinyItemSocketState> sockets)
        {
            var perkTasks = sockets.SelectMany(socket =>
            {
                if(socket.ReusablePlugs != null)
                {
                    var tasks = socket.ReusablePlugs.Select(reusablePlug =>
                    {
                        return LoadPerk(reusablePlug.PlugItemHash);
                    });
                    return tasks;
                }

                return new Task<Perk>[] { LoadPerk(socket.PlugHash) };
            });
            
            return await Task.WhenAll(perkTasks);
        }

        private async Task<Perk> LoadPerk(uint hash)
        {
            var plug = await _manifest.LoadPlug(hash);
            var categories = await _manifest.LoadItemCategories(plug.ItemCategoryHashes);
            return new Perk(_bungie.BaseUrl, plug, categories);
        }
    }
}