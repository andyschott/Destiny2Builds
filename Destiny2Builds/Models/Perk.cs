using System.Collections.Generic;
using System.Linq;
using Destiny2.Definitions;

namespace Destiny2Builds.Models
{
    public class Perk : AbstractDestinyObject
    {
        public Perk(string baseUrl, bool isSelected, DestinyInventoryItemDefinition plug,
            IEnumerable<DestinyItemCategoryDefinition> categories)
            : base(baseUrl, plug)
        {
            IsSelected = isSelected;
            CategoryHash = plug.Plug.PlugCategoryHash;
            Categories = categories;
        }

        private static ISet<uint> _modCategories = new HashSet<uint>
        {
            4104513227, // Armor Mods
            56,         // Solstice Armor Glows (I hope)
        };

        public bool IsSelected { get; set; }
        public uint CategoryHash { get; }
        public IEnumerable<DestinyItemCategoryDefinition> Categories { get; }

        // TODO: Might need to update this for weapon mods if that is ever needed.
        public bool IsMod
        {
            get
            {
                var categoryHashes = Categories.Select(category => category.Hash);
                var intersection = _modCategories.Intersect(categoryHashes);
                return intersection.Any();
            }
        }
    }
}