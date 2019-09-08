using System.Collections.Generic;
using System.Threading.Tasks;
using Destiny2.Definitions;
using Destiny2.Definitions.Sockets;
using Destiny2.Entities.Items;
using Destiny2Builds.Models;

namespace Destiny2Builds.Services
{
    public interface IPerkFactory
    {
         Task<IEnumerable<Perk>> LoadPerks(DestinyItemSocketState socket);
         Task<IEnumerable<IEnumerable<Perk>>> LoadPerks(IEnumerable<DestinyItemSocketState> sockets);
         Task<IEnumerable<Perk>> LoadAvailablePerks(DestinyItemSocketEntryDefinition socketEntry,
            DestinySocketCategoryDefinition categoryDef, IEnumerable<Perk> currentPerks);
        Task<Perk> LoadPerk(uint hash, bool isSelected);
    }
}