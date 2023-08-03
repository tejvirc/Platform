namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Kernel;

    /// <summary>
    ///     Interface for cash out calculator, which is a way for a market to restrict the
    ///     amount that can be cashed out by the player. For example, in Georgia COAM a
    ///     player can only cash out some multiple of the number of games they have played.
    /// </summary>
    public interface ICashOutAmountCalculator : IService
    {
        /// <summary>
        ///     Returns the amount that is allowed to be cashed out by this calculator.
        /// </summary>
        long GetCashableAmount(long amount);

        /// <summary>
        ///     If cashing out the amount allowed by GetCashableAmount requires updating
        ///     some other state, then that can be done here when the transfer provider
        ///     is finished.
        /// </summary>
        void PostProcessTransaction(long amount);
    }
}