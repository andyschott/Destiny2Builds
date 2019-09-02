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
        public IEnumerable<Item> Items { get; set; } = new List<Item>();
        public string EmblemPath { get; set; }
        public string EmblemBackgroundPath {get; set; }

        public IEnumerable<Item> Weapons => Items.Where(item => item.IsWeapon)
                                                 .OrderBy(item => item.Slot.Order);
        
        public IEnumerable<Item> Armor => Items.Where(item => item.IsArmor)
                                               .OrderBy(item => item.Slot.Order);
    }
}