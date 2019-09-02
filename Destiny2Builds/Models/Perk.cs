using System.Collections.Generic;
using Destiny2.Definitions;

namespace Destiny2Builds.Models
{
    public class Perk : AbstractDestinyObject
    {
        public Perk(string baseUrl, DestinyInventoryItemDefinition plug,
            IEnumerable<DestinyItemCategoryDefinition> categories)
            : base(baseUrl, plug)
        {
            Categories = categories;
        }

        public IEnumerable<DestinyItemCategoryDefinition> Categories { get; }
    }
}