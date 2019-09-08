using System.Collections.Generic;
using System.Linq;
using Destiny2.Definitions.Sockets;

namespace Destiny2Builds.Models
{
    public class SocketCategory : AbstractDestinyObject
    {
        public SocketCategory(DestinySocketCategoryDefinition categoryDef, IEnumerable<Socket> sockets)
            : base(string.Empty, categoryDef)
        {
            Sockets = sockets?.ToList() ?? Enumerable.Empty<Socket>();
        }

        public IEnumerable<Socket> Sockets { get; }
    }
}