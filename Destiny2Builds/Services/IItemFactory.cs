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
            IDictionary<long, DestinyItemInstanceComponent> itemInstances);
        Task<Item> LoadItem(BungieMembershipType type, long accountId, uint itemHash, long instanceId);
    }
}