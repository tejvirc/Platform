namespace Aristocrat.Monaco.Gaming.Commands
{
    public class SecondaryGameStarted
    {
        public SecondaryGameStarted(long stake)
        {
            Stake = stake;
        }

        public long Stake { get; }
    }
}
