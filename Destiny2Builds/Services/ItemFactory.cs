using System.Collections.Generic;
using System.Threading.Tasks;
using Destiny2;
using Destiny2.Definitions;
using Destiny2.Entities.Items;
using Destiny2Builds.Helpers;
using Destiny2Builds.Models;
using Microsoft.Extensions.Options;

namespace Destiny2Builds.Services
{
    public class ItemFactory : IItemFactory
    {
        private readonly IManifest _manifest;
        private readonly BungieSettings _bungie;

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
            };

        public ItemFactory(IManifest manifest, IOptions<BungieSettings> bungie)
        {
            _manifest = manifest;
            _bungie = bungie.Value;
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
                items.Add(new Item(_bungie.BaseUrl, itemComponent, itemDef, bucket, instance));
            }

            return items;
        }

        private static bool ShouldInclude(DestinyInventoryBucketDefinition bucket)
        {
            return _includedBuckets.Contains((ItemSlot.SlotHashes)bucket.Hash);
        }
    }
}