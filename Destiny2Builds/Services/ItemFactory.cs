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
                instanceId, instanceTask.Result.instance, instanceTask.Result.sockets);
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

        private async Task<(DestinyItemInstanceComponent instance, IEnumerable<Socket> sockets)> GetInstance(BungieMembershipType type, long accountId, long instanceId)
        {
            var accessToken = await _contextAccessor.HttpContext.GetTokenAsync("access_token");
            var instance = await _destiny2.GetItem(accessToken, type, accountId, instanceId,
                DestinyComponentType.ItemInstances, DestinyComponentType.ItemSockets);

            IEnumerable<Socket> sockets = null;
            var itemSockets = instance.Sockets?.Data.Sockets;
            if(itemSockets != null)
            {
                sockets = await LoadSockets(itemSockets);
            }

            return (instance.Instance.Data, sockets);
        }

        private async Task<IEnumerable<Socket>> LoadSockets(IEnumerable<DestinyItemSocketState> itemSockets)
        {
            var socketTasks = itemSockets.Where(socket => socket.IsEnabled && socket.IsVisible)
                .Select(async itemSocket =>
            {
                var perks = await LoadPerks(itemSocket);
                return new Socket(perks);
            });

            return await Task.WhenAll(socketTasks);
        }

        private async Task<IEnumerable<Perk>> LoadPerks(DestinyItemSocketState socket)
        {
            if(socket.ReusablePlugs != null)
            {
                var tasks = socket.ReusablePlugs.Select(reusablePlug =>
                {
                    var isSelected = reusablePlug.PlugItemHash == socket.PlugHash;
                    return LoadPerk(reusablePlug.PlugItemHash, isSelected);
                });
                var perks = await Task.WhenAll(tasks);
                if(!perks.Any(perk => perk.Hash == socket.PlugHash))
                {
                    var selectedPerk = await LoadPerk(socket.PlugHash, true);
                    return perks.Concat(new[] { selectedPerk });
                }
            }

            var singlePerk = await LoadPerk(socket.PlugHash, true);
            return new[] { singlePerk };
        }

        private async Task<Perk> LoadPerk(uint hash, bool isSelected)
        {
            var plug = await _manifest.LoadPlug(hash);
            var categories = await _manifest.LoadItemCategories(plug.ItemCategoryHashes);
            return new Perk(_bungie.BaseUrl, isSelected, plug, categories);
        }
    }
}