using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IManifestCache _cache;
        private readonly BungieSettings _bungie;
        private readonly IPerkFactory _perkFactory;

        public SocketFactory(IManifestCache cache, IOptions<BungieSettings> bungie,
            IPerkFactory perkFactory)
        {
            _cache = cache;
            _bungie = bungie.Value;
            _perkFactory = perkFactory;
        }

        public async Task<IEnumerable<SocketCategory>> LoadSockets(DestinyItemSocketBlockDefinition socketDefs,
            IEnumerable<DestinyItemSocketState> itemSockets, IEnumerable<Mod> mods,
            IEnumerable<Mod> shaders)
        {
            return await IterateOverSockets(socketDefs, itemSockets, async (socketEntry, socketType, categoryDef, socketState) =>
            {
                var perksTask = _perkFactory.LoadPerks(socketState);
                var socketTypeTask = _cache.GetSocketTypeDef(socketEntry.SocketTypeHash);

                await Task.WhenAll(perksTask, socketTypeTask);

                var socket = await CreateSocket(socketEntry, socketTypeTask.Result, categoryDef,
                    mods, shaders, perksTask.Result);
                return socket;
            });
        }

        public Task<IEnumerable<SocketCategory>> LoadActiveSockets(DestinyItemSocketBlockDefinition socketDefs,
            IEnumerable<DestinyItemSocketState> itemSockets)
        {
            return IterateOverSockets(socketDefs, itemSockets, CreateSocket);
        }

        private async Task<IEnumerable<SocketCategory>> IterateOverSockets(DestinyItemSocketBlockDefinition socketDefs,
            IEnumerable<DestinyItemSocketState> itemSockets,
            Func<DestinyItemSocketEntryDefinition, DestinySocketTypeDefinition, DestinySocketCategoryDefinition, DestinyItemSocketState, Task<Socket>> func)
        {
            var socketArray = itemSockets.ToArray();
            var socketEntries = socketDefs.SocketEntries.ToArray();

            var socketCategoryTasks = socketDefs.SocketCategories.Select(async category =>
            {
                var categoryDef = await _cache.GetSocketCategoryDef(category.SocketCategoryHash);

                // Technically this should use the DestinySocketTypeDefinition.PlugWhiteList
                // to match the categories of the perks in a socket. Unfortuantely that
                // doesn't work for weapons - the two columns of perks have the same
                // DestinySocketTypeDefinition and thus the code can't tell them apart. :(
                // Instead, assume that the sockets in the array of DestinyItemSocketState's
                // are in the same order as the sockets in the DestinyItemSocketBlockDefinition
                // and hope for the best.
                var categoryEntries = category.SocketIndexes.Select(index => (entry: socketEntries[index], socket: socketArray[index]))
                    .Where(item => item.entry.DefaultVisible);
                
                var socketTasks = categoryEntries.Select(async item =>
                {
                    var socketType = await _cache.GetSocketTypeDef(item.entry.SocketTypeHash);
                    return await func(item.entry, socketType, categoryDef, item.socket);
                });
                
                var sockets = await Task.WhenAll(socketTasks);
                return new SocketCategory(categoryDef, sockets);
            });

            return await Task.WhenAll(socketCategoryTasks);
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
                selectedPerk = await _perkFactory.LoadPerk(socketEntry.SingleInitialItemHash, true);
            }

            var currentPerk = availablePerks.FirstOrDefault(perk => perk.Hash == selectedPerk.Hash);
            if(currentPerk == null)
            {
                availablePerks = availablePerks.Concat(new[] { selectedPerk });
            }
            else if(!currentPerk.IsSelected)
            {
                currentPerk.IsSelected = true;
            }
            
            return new Socket(selectedPerk, availablePerks);
        }

        private async Task<Socket> CreateSocket(DestinyItemSocketEntryDefinition socketEntry,
            DestinySocketTypeDefinition socketType, DestinySocketCategoryDefinition categoryDef,
            DestinyItemSocketState socketState)
        {
            Perk selectedPerk = null;
            if(socketState.PlugHash != 0)
            {
                selectedPerk = await _perkFactory.LoadPerk(socketState.PlugHash, true);
            }
            
            return new Socket(selectedPerk, null);
        }
    }
}