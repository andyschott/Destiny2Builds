using System.Collections.Generic;
using Destiny2.Definitions;

namespace Destiny2Builds.Models
{
    public class Mod : Perk
    {
        public Mod(string baseUrl, bool isSelected, DestinyInventoryItemDefinition mod,
            IEnumerable<DestinyItemCategoryDefinition> categories,
            int quantity)
            : base(baseUrl, isSelected, mod, categories)
        {
            Quantity = quantity;
        }

        public int Quantity { get; }
    }
}