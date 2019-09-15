using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny2;
using Destiny2.Definitions;
using Destiny2.Definitions.Sockets;

namespace Destiny2Builds.Services
{
    public class ManifestCache : IManifestCache
    {
        private readonly IManifest _manifest;

        private readonly Cache<DestinyInventoryItemDefinition> _itemDefs;
        private readonly Cache<DestinyInventoryBucketDefinition> _bucketDefs;
        private readonly CollectionCache<DestinyStatDefinition> _statDefs;
        private readonly Cache<DestinySocketCategoryDefinition> _socketCategoryDefs;
        private readonly CollectionCache<DestinyItemCategoryDefinition> _itemCategoryDefs;
        private readonly Cache<DestinySocketTypeDefinition> _socketTypeDefs;
        private readonly Cache<DestinyInventoryItemDefinition> _plugDefs;

        public ManifestCache(IManifest manifest)
        {
            _manifest = manifest;

            _itemDefs = new Cache<DestinyInventoryItemDefinition>(hash => _manifest.LoadInventoryItem(hash));
            _bucketDefs = new Cache<DestinyInventoryBucketDefinition>(hash => _manifest.LoadBucket(hash));
            _statDefs = new CollectionCache<DestinyStatDefinition>(hashes => _manifest.LoadStats(hashes));
            _socketCategoryDefs = new Cache<DestinySocketCategoryDefinition>(hash => _manifest.LoadSocketCategory(hash));
            _itemCategoryDefs = new CollectionCache<DestinyItemCategoryDefinition>(hashes => _manifest.LoadItemCategories(hashes));
            _socketTypeDefs = new Cache<DestinySocketTypeDefinition>(hash => _manifest.LoadSocketType(hash));
            _plugDefs = new Cache<DestinyInventoryItemDefinition>(hash => _manifest.LoadPlug(hash));
        }

        public Task<DestinyInventoryItemDefinition> GetInventoryItemDef(uint hash)
        {
            return _itemDefs.Get(hash);
        }

        public Task<DestinyInventoryBucketDefinition> GetBucketDef(uint hash)
        {
            return _bucketDefs.Get(hash);
        }

        public Task<IEnumerable<DestinyStatDefinition>> GetStatDefs(IEnumerable<uint> hashes)
        {
            return _statDefs.Get(hashes);
        }

        public Task<DestinySocketCategoryDefinition> GetSocketCategoryDef(uint hash)
        {
            return _socketCategoryDefs.Get(hash);
        }

        public Task<IEnumerable<DestinyItemCategoryDefinition>> GetItemCategoryDefinitions(IEnumerable<uint> hashes)
        {
            return _itemCategoryDefs.Get(hashes);
        }

        public Task<DestinySocketTypeDefinition> GetSocketTypeDef(uint hash)
        {
            return _socketTypeDefs.Get(hash);
        }

        public Task<DestinyInventoryItemDefinition> GetPlugDef(uint hash)
        {
            return _plugDefs.Get(hash);
        }

        class Cache<T> where T: AbstractDefinition
        {
            private readonly ConcurrentDictionary<uint, T> _cache = new ConcurrentDictionary<uint, T>();
            private readonly Func<uint, Task<T>> _loader;

            public Cache(Func<uint, Task<T>> loader)
            {
                _loader = loader;
            }

            public async Task<T> Get(uint hash)
            {
                if(_cache.TryGetValue(hash, out var value))
                {
                    return value;
                }

                value = await _loader(hash);
                return _cache.GetOrAdd(hash, value);
            }
        }

        class CollectionCache<T> where T: AbstractDefinition
        {
            private readonly ConcurrentDictionary<uint, T> _cache = new ConcurrentDictionary<uint, T>();
            private readonly Func<IEnumerable<uint>, Task<IEnumerable<T>>> _loader;

            public CollectionCache(Func<IEnumerable<uint>, Task<IEnumerable<T>>> loader)
            {
                _loader = loader;
            }

            public async Task<IEnumerable<T>> Get(IEnumerable<uint> hashes)
            {
                var results = new List<T>();

                var needToLoad = new List<uint>();
                foreach(var hash in hashes)
                {
                    if(_cache.TryGetValue(hash, out var value))
                    {
                        results.Add(value);
                    }
                    else
                    {
                        needToLoad.Add(hash);
                    }
                }

                if(needToLoad.Any())
                {
                    var loaded = await _loader(needToLoad);
                    foreach(var value in loaded)
                    {
                        results.Add(_cache.GetOrAdd(value.Hash, value));
                    }
                }

                return results;
            }
        }
    }
}