using Destiny2.Definitions;

namespace Destiny2Builds.Models
{
    public abstract class AbstractDestinyObject
    {
        protected AbstractDestinyObject(string baseUrl, DestinyInventoryItemDefinition itemDef)
        {
            Name = itemDef.DisplayProperties.Name;
            Icon = baseUrl + itemDef.DisplayProperties.Icon;
            Hash = itemDef.Hash;
        }

        protected AbstractDestinyObject(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public string Icon { get; }
        public uint Hash { get; }
    }
}