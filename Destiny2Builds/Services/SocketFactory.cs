using System;
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
                var selectedPerk = perks.FirstOrDefault(perk => perk.IsSelected);
                return new Socket(selectedPerk, perks);
            });

            return await Task.WhenAll(socketTasks);
        }

        public async Task<IEnumerable<SocketCategory>> LoadSockets(DestinyItemSocketBlockDefinition socketDefs,
            IEnumerable<DestinyItemSocketState> itemSockets, IEnumerable<Mod> mods,
            IEnumerable<Mod> shaders)
        {
            var socketCategories = new List<SocketCategory>();

            var socketArray = itemSockets.ToArray();

            var socketEntries = socketDefs.SocketEntries.ToArray();
            foreach(var category in socketDefs.SocketCategories)
            {
                var categoryDef = await _manifest.LoadSocketCategory(category.SocketCategoryHash);
                var sockets = new List<Socket>();
                foreach(var index in category.SocketIndexes)
                {
                    var socketEntry = socketEntries[index];
                    if(!socketEntry.DefaultVisible)
                    {
                        continue;
                    }

                    // Technically this should use the DestinySocketTypeDefinition.PlugWhiteList
                    // to match the categories of the perks in a socket. Unfortuantely that
                    // doesn't work for weapons - the two columns of perks have the same
                    // DestinySocketTypeDefinition and thus the code can't tell them apart. :(
                    // Instead, assume that the sockets in the array of DestinyItemSocketState's
                    // are in the same order as the sockets in the DestinyItemSocketBlockDefinition
                    // and hope for the best.
                    var perks = await _perkFactory.LoadPerks(socketArray[index]);

                    var socketType = await _manifest.LoadSocketType(socketEntry.SocketTypeHash);
                    var socket = await CreateSocket(socketEntry, socketType, categoryDef,
                        mods, shaders, perks);
                    sockets.Add(socket);
                }

                var socketCategory = new SocketCategory(categoryDef, sockets);
                socketCategories.Add(socketCategory);
            }

            return socketCategories;
        }

        private async Task<Socket> CreateSocket(DestinyItemSocketEntryDefinition socketEntry,
            DestinySocketTypeDefinition socketType, DestinySocketCategoryDefinition categoryDef,
            IEnumerable<Mod> mods, IEnumerable<Mod> shaders, IEnumerable<Perk> perks)
        {
            var availablePerks = await _perkFactory.LoadAvailablePerks(socketEntry,
                socketType, categoryDef, mods, shaders, perks);
            var selectedPerk = perks?.FirstOrDefault(perk => perk.IsSelected);
            if(selectedPerk == null)
            {
                var initialMod = await _manifest.LoadInventoryItem(socketEntry.SingleInitialItemHash);
                var categoryDefs = await _manifest.LoadItemCategories(initialMod.ItemCategoryHashes);
                selectedPerk = new Perk(true, initialMod, categoryDefs);
            }

            if(!availablePerks.Contains(selectedPerk, AbstractDestinyObjectComparer.Instance))
            {
                availablePerks = availablePerks.Concat(new[] { selectedPerk });
            }
            return new Socket(selectedPerk, availablePerks);
        }
    }
}