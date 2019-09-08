using Destiny2.Definitions;

namespace Destiny2Builds.Models
{
    public abstract class AbstractDestinyObject
    {
        protected AbstractDestinyObject(string baseUrl, AbstractDefinition item)
            : this(item.DisplayProperties.Name, baseUrl + item.DisplayProperties.Icon, item.Hash)
        {
        }

        protected AbstractDestinyObject(string name)
        {
            Name = name;
        }

        protected AbstractDestinyObject(string name, string icon, uint hash)
        {
            Name = name;
            Icon = icon;
            Hash = hash;
        }

        public string Name { get; }
        public string Icon { get; }
        public uint Hash { get; }
    }
}