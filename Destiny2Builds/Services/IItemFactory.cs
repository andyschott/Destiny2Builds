using System.Collections.Generic;
using System.Threading.Tasks;
using Destiny2;
using Destiny2.Entities.Items;
using Destiny2Builds.Models;

namespace Destiny2Builds.Services
{
    public interface IItemFactory
    {
         Task<IEnumerable<Item>> LoadItems(IEnumerable<DestinyItemComponent> itemComponents,
            IDictionary<long, DestinyItemInstanceComponent> itemInstances,
            IDictionary<long, DestinyItemStatsComponent> itemStats);
        Task<Item> LoadItem(BungieMembershipType type, long accountId, long characterId,
            uint itemHash, long instanceId);
    }
}