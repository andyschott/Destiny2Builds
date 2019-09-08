using System.Collections.Generic;
using System.Threading.Tasks;
using Destiny2.Entities.Items;
using Destiny2Builds.Models;

namespace Destiny2Builds.Services
{
    public interface IPerkFactory
    {
         Task<IEnumerable<Perk>> LoadPerks(DestinyItemSocketState socket);
         Task<IEnumerable<IEnumerable<Perk>>> LoadPerks(IEnumerable<DestinyItemSocketState> sockets);
    }
}