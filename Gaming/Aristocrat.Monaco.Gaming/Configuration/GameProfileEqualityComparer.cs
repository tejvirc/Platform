namespace Aristocrat.Monaco.Gaming.Configuration
{
    using System.Collections.Generic;
    using Contracts;

    public class GameProfileEqualityComparer : IEqualityComparer<IGameDetail>
    {
        public bool Equals(IGameDetail x, IGameDetail y)
        {
            return ReferenceEquals(x, y) || x?.Id == y?.Id;
        }

        public int GetHashCode(IGameDetail obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}