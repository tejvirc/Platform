namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;

    public class ProgressiveLevelWagers
    {
        public ProgressiveLevelWagers(IEnumerable<long> wagers)
        {
            LevelWagers = wagers;
        }

        public IEnumerable<long> LevelWagers { get; }
    }
}