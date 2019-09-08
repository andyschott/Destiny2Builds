using System.Collections.Generic;
using System.Threading.Tasks;
using Destiny2.Entities.Items;
using Destiny2Builds.Models;

namespace Destiny2Builds.Services
{
    public interface ISocketFactory
    {
        Task<IEnumerable<Socket>> LoadSockets(IEnumerable<DestinyItemSocketState> itemSockets);
    }
}