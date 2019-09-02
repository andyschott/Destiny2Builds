using System.Collections.Generic;
using Destiny2;

namespace Destiny2Builds.Models
{
    public class CharactersViewModel
    {
        public CharactersViewModel(BungieMembershipType type, long id)
        {
            Type = type;
            Id = id;
        }

        public BungieMembershipType Type { get; }
        public long Id { get; }
        public IList<Character> Characters { get; set; } = new List<Character>();
    }
}