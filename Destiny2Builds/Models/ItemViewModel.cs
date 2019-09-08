using System.Collections.Generic;
using Destiny2;

namespace Destiny2Builds.Models
{
    public class ItemViewModel
    {
        public BungieMembershipType Type { get; set; }
        public long AccountId { get; set; }
        public long CharacterId { get; set; }
        public Item Item { get; set; }
        public IList<SocketCategoryViewModel> SocketCategories { get; set; }
    }
}