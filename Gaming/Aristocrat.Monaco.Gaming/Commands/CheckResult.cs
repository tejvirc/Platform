namespace Aristocrat.Monaco.Gaming.Commands
{
    public class CheckResult
    {
        public CheckResult(long result)
        {
            Result = result;
        }

        public long Result { get; }

        public bool ForcedCashout { get; set; }

        public long AmountOut { get; set; }
    }
}
