using System.Collections.Generic;
using System.Linq;

namespace Destiny2Builds.Models
{
    public class Socket
    {
        public Socket(Perk selectedPerk, IEnumerable<Perk> perks)
        {
            Perks = perks?.ToList() ?? Enumerable.Empty<Perk>();
            SelectedPerk = selectedPerk;
        }

        public IEnumerable<Perk> Perks { get; }

        public Perk SelectedPerk { get; }
    }
}