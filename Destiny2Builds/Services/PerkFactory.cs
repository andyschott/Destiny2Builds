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
        private readonly IManifestCache _cache;

        private static readonly ISet<uint> _loadFromReusablePlugItemSocketTypeHashes =
            new HashSet<uint>
            {
                501110267,  // Armor
                2218962841, // Weapon
                354911055,  // Ghost Shell Projections
            };
        private const uint ModsCategoryHash = 59;
        private const uint ShadersCategoryHash = 41;

        public PerkFactory(IManifestCache cache)
        {
            _cache = cache;
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

        public async Task<IEnumerable<Perk>> LoadAvailablePerks(DestinyItemSocketEntryDefinition socketEntry,
            DestinySocketTypeDefinition socketType, DestinySocketCategoryDefinition categoryDef,
            IEnumerable<Mod> mods, IEnumerable<Mod> shaders, IEnumerable<Perk> currentPerks)
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
                            var perks = await LoadReusablePlugItems(socketEntry);
                            perkGroups.Add(perks);
                        }
                        break;
                    }
                    case DestinySocketCategoryStyle.Consumable:
                    {
                        // Why does the Masterwork Armor slot get here?!?!?
                        if(ShouldLoadFromReusablePlugItems(socketType))
                        {
                            // All of the possible Masterwork armor plugs are in the
                            // ReusablePlugItems on the SocketEntry.
                            var perks = await LoadReusablePlugItems(socketEntry);
                            perkGroups.Add(perks);
                        }
                        else
                        {
                            var perks = FindCompatibleMods(socketEntry, socketType, mods, shaders);
                            perkGroups.Add(perks);
                        }
                        break;
                    }
                    case DestinySocketCategoryStyle.Unlockable:
                    {
                        // Only known case of getting here is ghost shell perks.
                        // Treat these like normal item perks and hope for the best.
                        goto case DestinySocketCategoryStyle.Reusable;
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

            return perkGroups.Where(perkGroup => perkGroup != null).SelectMany(perkGroup => perkGroup);
        }

        public async Task<Perk> LoadPerk(uint hash, bool isSelected)
        {
            var plug = await _cache.GetPlugDef(hash);
            var categories = await _cache.GetItemCategoryDefinitions(plug.ItemCategoryHashes);
            return new Perk(isSelected, plug, categories);
        }

        public async Task<(IEnumerable<Mod> mods, IEnumerable<Mod> shaders)> LoadAllMods(IEnumerable<DestinyItemComponent> inventoryItems)
        {
            var mods = new List<Mod>();
            var shaders = new List<Mod>();

            foreach(var item in inventoryItems)
            {
                var itemDef = await _cache.GetInventoryItemDef(item.ItemHash);
                if(itemDef.ItemCategoryHashes.Contains(ModsCategoryHash))
                {
                    var categories = await _cache.GetItemCategoryDefinitions(itemDef.ItemCategoryHashes);
                    mods.Add(new Mod(false, itemDef, categories, item.Quantity));
                }
                else if(itemDef.ItemCategoryHashes.Contains(ShadersCategoryHash))
                {
                    var categories = await _cache.GetItemCategoryDefinitions(itemDef.ItemCategoryHashes);
                    shaders.Add(new Mod(false, itemDef, categories, item.Quantity));
                }
            }

            return (mods, shaders);
        }

        private static bool ShouldLoadFromReusablePlugItems(DestinySocketTypeDefinition socketType) =>
            _loadFromReusablePlugItemSocketTypeHashes.Contains(socketType.Hash);
        
        private IEnumerable<Perk> FindCompatibleMods(DestinyItemSocketEntryDefinition socketEntry,
            DestinySocketTypeDefinition socketType, IEnumerable<Mod> mods, IEnumerable<Mod> shaders)
        {
            var categories = socketType.PlugWhiteList.Select(whiteListEntry => whiteListEntry.CategoryHash)
                .ToHashSet();

            var compatibleMods = mods.Where(mod => categories.Contains(mod.CategoryHash));
            if(!compatibleMods.Any())
            {
                compatibleMods = shaders.Where(shader => categories.Contains(shader.CategoryHash));
            }
            return compatibleMods;
        }

        private async Task<IEnumerable<Perk>> LoadReusablePlugItems(DestinyItemSocketEntryDefinition socketEntry)
        {
            var tasks = socketEntry.ReusablePlugItems.Select(reusablePlug =>
                LoadPerk(reusablePlug.PlugItemHash, false));
            var perks = await Task.WhenAll(tasks);
            return perks;
        }
    }
}