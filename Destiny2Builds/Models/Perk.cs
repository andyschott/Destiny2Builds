using System.Collections.Generic;
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
            Categories = categories;
        }

        public bool IsSelected { get; }
        public IEnumerable<DestinyItemCategoryDefinition> Categories { get; }
    }
}