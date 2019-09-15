using Destiny2;
using Destiny2.Definitions;

namespace Destiny2Builds.Models
{
    public class Stat : AbstractDestinyObject
    {
        public Stat(DestinyStat stat, DestinyStatDefinition statDef)
            : base(string.Empty, statDef)
        {
            AggregationType = statDef.AggregationType;
            Value = stat.Value;
        }

        public Stat(Stat originalStat, int newValue)
            : base(originalStat.Name, originalStat.Icon, originalStat.Hash)
        {
            AggregationType = originalStat.AggregationType;
            Value = newValue;
        }

        public int Value { get; }
        public DestinyStatAggregationType AggregationType { get; }
    }
}