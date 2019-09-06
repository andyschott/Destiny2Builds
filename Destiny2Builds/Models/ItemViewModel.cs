using System.Collections.Generic;
using Destiny2;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Destiny2Builds.Models
{
    public class ItemViewModel
    {
        public BungieMembershipType Type { get; set; }
        public long AccountId { get; set; }
        public long CharacterId { get; set; }
        public Item Item { get; set; }
        public IList<SocketViewModel> Sockets { get; set; }
    }
}