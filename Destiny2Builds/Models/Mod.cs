using System.Collections.Generic;
using Destiny2.Definitions;

namespace Destiny2Builds.Models
{
    public class Mod : Perk
    {
        public Mod(bool isSelected, DestinyInventoryItemDefinition mod,
            IEnumerable<DestinyItemCategoryDefinition> categories,
            int quantity)
            : base(isSelected, mod, categories)
        {
            Quantity = quantity;
        }

        public int Quantity { get; }
    }
}