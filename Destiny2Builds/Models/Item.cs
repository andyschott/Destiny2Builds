using System.Collections.Generic;
using System.Linq;
using Destiny2;
using Destiny2.Definitions;
using Destiny2.Entities.Items;

namespace Destiny2Builds.Models
{
    public class Item : AbstractDestinyObject
    {
        public Item(string baseUrl, DestinyInventoryItemDefinition itemDef,
            DestinyInventoryBucketDefinition bucket, long instanceId = 0,
            DestinyItemInstanceComponent instance = null,
            IEnumerable<Stat> stats = null,
            IEnumerable<SocketCategory> socketCategories = null)
            : base(baseUrl, itemDef)
        {
            PowerLevel = instance?.PrimaryStat?.Value ?? 0;
            Slot = new ItemSlot(bucket);
            Tier = itemDef.Inventory.TierType;
            ClassType = itemDef.ClassType;
            InstanceId = instanceId;
            SocketCategories = socketCategories?.ToList() ?? Enumerable.Empty<SocketCategory>();
            Stats = stats?.ToList() ?? Enumerable.Empty<Stat>();
        }

        public Item(string name, ItemSlot.SlotHashes slot, int powerLevel,
            TierType tier = TierType.Superior, DestinyClass classType = DestinyClass.Unknown)
            : base(name)
        {
            Slot = new ItemSlot(slot.ToString(), slot);
            PowerLevel = powerLevel;
            Tier = tier;
            ClassType = classType;
        }

        public ItemSlot Slot { get; }
        public int PowerLevel { get; }
        public TierType Tier { get; }
        public DestinyClass ClassType { get; }
        public long InstanceId { get; }
        public IEnumerable<SocketCategory> SocketCategories { get; }
        public IEnumerable<Stat> Stats { get; }

        public bool IsWeapon => Slot.IsWeapon;
        public bool IsArmor => Slot.IsArmor;

        private static ISet<uint> _ignorePerkCategories = new HashSet<uint>
        {
            41,         // Shaders
            1404791674, // Ghost Shell Projection
        };
        private static ISet<uint> _ignoreCategoryHashes = new HashSet<uint>
        {
            1514141499, // Arc Masterwork Armor
            1514141500, // Solar Masterwork Armor
            1514141501, // Void Masterwork Armor
            3313201758, // Mobility / Resilience / Restoration
            1744546145, // Intrinsic Armor Perk
        };

        public override bool Equals(object obj)
        {
            if(!(obj is Item item))
            {
                return false;
            }

            return Name.Equals(item.Name) &&
                Slot == item.Slot &&
                PowerLevel == item.PowerLevel &&
                Tier == item.Tier &&
                ClassType == item.ClassType;
        }

        public override int GetHashCode()
        {
            return 23 ^ Name.GetHashCode() ^
                Slot.GetHashCode() ^
                PowerLevel.GetHashCode() ^
                Tier.GetHashCode() ^
                ClassType.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Name} ({PowerLevel})";
        }

        public IEnumerable<Perk> ActivePerks =>
            SocketCategories.SelectMany(category => category.Sockets)
                .Where(socket => socket.SelectedPerk != null)
                .Select(socket => socket.SelectedPerk)
                .Where(perk =>
                {
                    var intersection = _ignorePerkCategories.Intersect(perk.Categories.Select(category => category.Hash));
                    if(intersection.Any())
                    {
                        return false;
                    }

                    if(_ignoreCategoryHashes.Contains(perk.CategoryHash))
                    {
                        return false;
                    }

                    return true;
                });
    }
}