using System.Collections.Generic;
using System.Linq;

namespace Destiny2Builds.Models
{
    public class Socket
    {
        public Socket(IEnumerable<Perk> perks)
        {
            Perks = perks?.ToList() ?? Enumerable.Empty<Perk>();
        }

        public IEnumerable<Perk> Perks { get; }

        public Perk SelectedPerk => Perks.FirstOrDefault(perk => perk.IsSelected);
    }
}