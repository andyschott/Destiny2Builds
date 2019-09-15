using System.Collections.Generic;
using System.Threading.Tasks;
using Destiny2.Definitions;
using Destiny2.Definitions.Sockets;

namespace Destiny2Builds.Services
{
    public interface IManifestCache
    {
         Task<DestinyInventoryItemDefinition> GetInventoryItemDef(uint hash);
         Task<DestinyInventoryBucketDefinition> GetBucketDef(uint hash);
         Task<IEnumerable<DestinyStatDefinition>> GetStatDefs(IEnumerable<uint> hashes);
         Task<DestinySocketCategoryDefinition> GetSocketCategoryDef(uint hash);
         Task<IEnumerable<DestinyItemCategoryDefinition>> GetItemCategoryDefinitions(IEnumerable<uint> hashes);
         Task<DestinySocketTypeDefinition> GetSocketTypeDef(uint hash);
         Task<DestinyInventoryItemDefinition> GetPlugDef(uint hash);
    }
}