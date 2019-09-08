using System.Collections.Generic;
using System.Linq;
using Destiny2;

namespace Destiny2Builds.Models
{
    public class CharacterViewModel
    {
        public BungieMembershipType Type { get; set; }
        public long AccountId { get; set; }
        public long CharacterId { get; set; }
        public IDictionary<ItemSlot.SlotHashes, Item> Items { get; set; } = new Dictionary<ItemSlot.SlotHashes, Item>();
        public string EmblemPath { get; set; }
        public string EmblemBackgroundPath {get; set; }

        public IEnumerable<Item> AllItems => Items.Values.OrderBy(item => item.Slot.Order);
    }
}