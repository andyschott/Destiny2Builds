using System;
using System.Collections.Generic;
using Destiny2Builds.Models;

namespace Destiny2Builds.Helpers
{
    public class AbstractDestinyObjectComparer : IEqualityComparer<AbstractDestinyObject>
    {
        private static AbstractDestinyObjectComparer _instance =
            new AbstractDestinyObjectComparer();
        public static AbstractDestinyObjectComparer Instance => _instance;

        public bool Equals(AbstractDestinyObject x, AbstractDestinyObject y)
        {
            return x.Hash == y.Hash;
        }

        public int GetHashCode(AbstractDestinyObject obj)
        {
            return Convert.ToInt32(obj.Hash);
        }
    }
}