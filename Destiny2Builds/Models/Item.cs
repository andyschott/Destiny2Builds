using Destiny2;
using Destiny2.Definitions;
using Destiny2.Entities.Items;

namespace Destiny2Builds.Models
{
    public class Item
    {
        public Item(string baseUrl, DestinyInventoryItemDefinition itemDef,
            DestinyInventoryBucketDefinition bucket, long instanceId = 0, DestinyItemInstanceComponent instance = null)
        {
            Name = itemDef.DisplayProperties.Name;
            PowerLevel = instance?.PrimaryStat?.Value ?? 0;
            Slot = new ItemSlot(bucket);
            Tier = itemDef.Inventory.TierType;
            ClassType = itemDef.ClassType;
            Icon = baseUrl + itemDef.DisplayProperties.Icon;
            Hash = itemDef.Hash;
            InstanceId = instanceId;
        }

        public Item(string name, ItemSlot.SlotHashes slot, int powerLevel,
            TierType tier = TierType.Superior, DestinyClass classType = DestinyClass.Unknown)
        {
            Name = name;
            Slot = new ItemSlot(slot.ToString(), slot);
            PowerLevel = powerLevel;
            Tier = tier;
            ClassType = classType;
        }

        public string Name { get; }
        public ItemSlot Slot { get; }
        public int PowerLevel { get; }
        public TierType Tier { get; }
        public DestinyClass ClassType { get; }
        public string Icon { get; }
        public uint Hash { get; }
        public long InstanceId { get; }

        public bool IsWeapon => Slot.IsWeapon;
        public bool IsArmor => Slot.IsArmor;

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
    }
}