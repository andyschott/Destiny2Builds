using System.Collections.Generic;
using Destiny2.Definitions;

namespace Destiny2Builds.Models
{
    public class Perk : AbstractDestinyObject
    {
        public Perk(bool isSelected, DestinyInventoryItemDefinition plug,
            IEnumerable<DestinyItemCategoryDefinition> categories)
            : base(string.Empty, plug)
        {
            IsSelected = isSelected;
            CategoryHash = plug.Plug.PlugCategoryHash;
            Categories = categories;
        }

        public bool IsSelected { get; set; }
        public uint CategoryHash { get; }
        public IEnumerable<DestinyItemCategoryDefinition> Categories { get; }
    }
}