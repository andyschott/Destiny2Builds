using System.Collections.Generic;
using System.Linq;

namespace Destiny2Builds.Models
{
    public class Socket
    {
        public Socket(Perk selectedPerk, IEnumerable<Perk> perks)
        {
            if(perks != null)
            {
                Perks = perks.ToList();
            }
            else if(selectedPerk != null)
            {
                Perks = new[] { selectedPerk };
            }
            else
            {
                Perks = Enumerable.Empty<Perk>();
            }

            SelectedPerk = selectedPerk;
        }

        public IEnumerable<Perk> Perks { get; }

        public Perk SelectedPerk { get; }
    }
}