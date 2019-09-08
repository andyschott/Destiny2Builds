using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny2;
using Destiny2.Entities.Items;
using Destiny2Builds.Helpers;
using Destiny2Builds.Models;
using Microsoft.Extensions.Options;

namespace Destiny2Builds.Services
{
    public class SocketFactory : ISocketFactory
    {
        private readonly IManifest _manifest;
        private readonly BungieSettings _bungie;
        private readonly IPerkFactory _perkFactory;

        public SocketFactory(IManifest manifest, IOptions<BungieSettings> bungie,
            IPerkFactory perkFactory)
        {
            _manifest = manifest;
            _bungie = bungie.Value;
            _perkFactory = perkFactory;
        }

        public async Task<IEnumerable<Socket>> LoadSockets(IEnumerable<DestinyItemSocketState> itemSockets)
        {
            if(itemSockets == null)
            {
                return null;
            }

            var socketTasks = itemSockets.Where(socket => socket.IsEnabled && socket.IsVisible)
                .Select(async itemSocket =>
            {
                var perks = await _perkFactory.LoadPerks(itemSocket);
                return new Socket(perks);
            });

            return await Task.WhenAll(socketTasks);
        }
    }
}