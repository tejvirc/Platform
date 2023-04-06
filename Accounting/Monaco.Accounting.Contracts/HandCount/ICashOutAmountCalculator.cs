namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Kernel;

    /// <summary>
    ///     Contract for cash out calculator.
    /// </summary>
    public interface ICashOutAmountCalculator : IService
    {
        /// <summary>
        ///     Gets cashable amount for given available amount
        /// </summary>
        /// <returns>the amount</returns>
        long GetCashableAmount(long amount);

        /// <summary>
        ///     Gets hand count used for the given amount
        /// </summary>
        /// <returns>hand count</returns>
        int GetHandCountUsed(long amount);
    }
}
