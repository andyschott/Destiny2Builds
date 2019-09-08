using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny2;
using Destiny2Builds.Helpers;
using Destiny2Builds.Models;
using Microsoft.Extensions.Options;

namespace Destiny2Builds.Services
{
    public class StatFactory : IStatFactory
    {
        private readonly IManifest _manifest;
        private readonly BungieSettings _bungie;

        public StatFactory(IManifest manifest, IOptions<BungieSettings> bungie)
        {
            _manifest = manifest;
            _bungie = bungie.Value;
        }
        
        public async Task<IEnumerable<Stat>> LoadStats(DestinyStat primaryStat, IDictionary<uint, DestinyStat> stats)
        {
            if(stats == null)
            {
                return null;
            }
            
            var statDefs = await _manifest.LoadStats(stats.Keys.Concat(new[] { primaryStat.StatHash }));

            return statDefs.Select(statDef =>
            {
                if(!stats.TryGetValue(statDef.Hash, out var stat))
                {
                    if(statDef.Hash == primaryStat.StatHash)
                    {
                        stat = primaryStat;
                    }
                    else
                    {
                        throw new Exception($"Unexpected stat {statDef.DisplayProperties.Name}");
                    }
                }
                return new Stat(_bungie.BaseUrl, stat, statDef);
            });
        }
    }
}