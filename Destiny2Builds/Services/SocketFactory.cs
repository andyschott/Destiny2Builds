using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny2;
using Destiny2.Definitions;
using Destiny2.Definitions.Sockets;
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

        public async Task<IEnumerable<Socket>> LoadSockets(DestinyItemSocketBlockDefinition socketDefs,
            IEnumerable<DestinyItemSocketState> itemSockets)
        {
            var activePerks = (await _perkFactory.LoadPerks(itemSockets)).ToList();

            var socketEntries = socketDefs.SocketEntries.ToArray();
            foreach(var category in socketDefs.SocketCategories)
            {
                var categoryDef = await _manifest.LoadSocketCategory(category.SocketCategoryHash);
                foreach(var index in category.SocketIndexes)
                {
                    var socketEntry = socketEntries[index];
                    var socketType = await _manifest.LoadSocketType(socketEntry.SocketTypeHash);
                    var perkGroup = FindPerksForSocket(socketType, activePerks);

                    // TODO: Use socketEntry.PlugSources() to find all possible plugs for this socket
                }
            }

            return Enumerable.Empty<Socket>();
        }

        private IEnumerable<Perk> FindPerksForSocket(DestinySocketTypeDefinition socketType,
            IEnumerable<IEnumerable<Perk>> allPerks)
        {
            var categories = socketType.PlugWhiteList.Select(whiteListEntry => whiteListEntry.CategoryHash)
                .ToList();
            return allPerks.FirstOrDefault(perkGroup =>
            {
                return perkGroup.Any(perk =>
                {
                    var intersection = perk.Categories.Select(category => category.Hash)
                        .Intersect(categories);
                    return intersection.Any();
                    {
                        
                    }
                });
            });
        }
    }
}