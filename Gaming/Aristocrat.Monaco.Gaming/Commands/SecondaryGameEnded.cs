namespace Aristocrat.Monaco.Gaming.Commands
{
    public class SecondaryGameEnded
    {
        public SecondaryGameEnded(long win, long stake)
        {
            Win = win;
            Stake = stake;
        }

        public long Win { get; }

        public long Stake { get; }
    }
}
