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
    public class PerkFactory : IPerkFactory
    {
        private readonly IManifest _manifest;
        private readonly BungieSettings _bungie;

        public PerkFactory(IManifest manifest, IOptions<BungieSettings> bungie)
        {
            _manifest = manifest;
            _bungie = bungie.Value;
        }
        
        public async Task<IEnumerable<Perk>> LoadPerks(DestinyItemSocketState socket)
        {
            if(socket.ReusablePlugs != null)
            {
                var tasks = socket.ReusablePlugs.Select(reusablePlug =>
                {
                    var isSelected = reusablePlug.PlugItemHash == socket.PlugHash;
                    return LoadPerk(reusablePlug.PlugItemHash, isSelected);
                });
                var perks = await Task.WhenAll(tasks);
                if(!perks.Any(perk => perk.Hash == socket.PlugHash))
                {
                    var selectedPerk = await LoadPerk(socket.PlugHash, true);
                    return perks.Concat(new[] { selectedPerk });
                }

                return perks;
            }

            var singlePerk = await LoadPerk(socket.PlugHash, true);
            return new[] { singlePerk };
        }

        public async Task<IEnumerable<IEnumerable<Perk>>> LoadPerks(IEnumerable<DestinyItemSocketState> sockets)
        {
            var tasks = sockets.Select(socket => LoadPerks(socket));
            return await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<Perk>> LoadAvailablePerks(DestinyItemSocketEntryDefinition socketEntry,
            DestinySocketCategoryDefinition categoryDef, IEnumerable<Perk> currentPerks)
        {
            var perkGroups = new List<IEnumerable<Perk>>();

            if(socketEntry.PlugSources.HasFlag(SocketPlugSources.InventorySourced))
            {
                switch(categoryDef.CategoryStyle)
                {
                    case DestinySocketCategoryStyle.Reusable:
                    {
                        if(!socketEntry.ReusablePlugItems?.Any() ?? true)
                        {
                            // use the (only?) perk in the current perks
                            perkGroups.Add(currentPerks);
                        }
                        else
                        {
                            var tasks = socketEntry.ReusablePlugItems.Select(reusablePlug =>
                                LoadPerk(reusablePlug.PlugItemHash, false));
                            var perks = await Task.WhenAll(tasks);
                            perkGroups.Add(perks);
                        }
                        break;
                    }
                    case DestinySocketCategoryStyle.Consumable:
                    {
                        // TODO: Load mods/shaders/whatever from inventory
                        // Why does the Masterwork Armor slot get here?!?!?
                        break;
                    }
                    default:
                    {
                        // Not sure if we'll ever get here...
                        throw new Exception($"Unexpected DestinySocketCategoryStyle: {categoryDef.CategoryStyle}");
                    }
                }
            }

            if(socketEntry.PlugSources.HasFlag(SocketPlugSources.ReusablePlugItems))
            {
                perkGroups.Add(currentPerks);
            }

            if(socketEntry.PlugSources.HasFlag(SocketPlugSources.ProfilePlugSet))
            {
                // TODO: Load from ProfilePlugSets
            }

            if(socketEntry.PlugSources.HasFlag(SocketPlugSources.CharacterPlugSet))
            {
                // TODO: Load from CharacterPlugSets
            }

            return perkGroups.SelectMany(perkGroup => perkGroup);
        }

        public async Task<Perk> LoadPerk(uint hash, bool isSelected)
        {
            var plug = await _manifest.LoadPlug(hash);
            var categories = await _manifest.LoadItemCategories(plug.ItemCategoryHashes);
            return new Perk(_bungie.BaseUrl, isSelected, plug, categories);
        }
    }
}