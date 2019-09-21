using System.Collections.Generic;
using System.Linq;
using Destiny2;
using Destiny2.Definitions.Sockets;

namespace Destiny2Builds.Models
{
    public class SocketCategory : AbstractDestinyObject
    {
        public SocketCategory(DestinySocketCategoryDefinition categoryDef, IEnumerable<Socket> sockets)
            : base(string.Empty, categoryDef)
        {
            Sockets = sockets?.ToList() ?? Enumerable.Empty<Socket>();
            Style = categoryDef.CategoryStyle;
        }

        public IEnumerable<Socket> Sockets { get; }
        public DestinySocketCategoryStyle Style { get; }
    }
}