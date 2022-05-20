namespace Aristocrat.Monaco.Gaming.Commands
{
    public class CheckBalance
    {
        public CheckBalance()
        {
        }

        public CheckBalance(long pendingAmountOut)
        {
            PendingAmountOut = pendingAmountOut;
        }

        public long PendingAmountOut { get; }

        public bool ForcedCashout { get; set; }
    }
}