using System.Collections.Generic;
using System.Threading.Tasks;
using Destiny2;
using Destiny2Builds.Models;

namespace Destiny2Builds.Services
{
    public interface IStatFactory
    {
        Task<IEnumerable<Stat>> LoadStats(DestinyStat primaryStat, IDictionary<uint, DestinyStat> stats);
    }
}