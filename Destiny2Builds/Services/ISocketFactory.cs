using System.Collections.Generic;
using System.Threading.Tasks;
using Destiny2.Definitions;
using Destiny2.Entities.Items;
using Destiny2Builds.Models;

namespace Destiny2Builds.Services
{
    public interface ISocketFactory
    {
        Task<IEnumerable<Socket>> LoadSockets(IEnumerable<DestinyItemSocketState> itemSockets);
        Task<IEnumerable<SocketCategory>> LoadSockets(DestinyItemSocketBlockDefinition socketDefs,
            IEnumerable<DestinyItemSocketState> itemSockets,
            IEnumerable<Mod> mods);
    }
}