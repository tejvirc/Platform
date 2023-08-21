namespace Aristocrat.Monaco.Gaming.Commands
{
    public class SelectDenomination
    {
        public SelectDenomination(long denomination)
        {
            Denomination = denomination;
        }

        public long Denomination { get; }
    }
}