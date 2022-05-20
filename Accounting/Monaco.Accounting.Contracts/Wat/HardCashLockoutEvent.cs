namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     The Event is raised when HardCashLockout has occured during WAT Transfer.
    /// </summary>
    [Serializable]
    public class HardCashLockoutEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardCashLockoutEvent" /> class.
        /// </summary>
        public HardCashLockoutEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardCashLockoutEvent" /> class.
        /// </summary>
        /// <param name="cashableAmount">The cashable amount being handpaid. </param>
        /// <param name="nonCashableAmount">The noncashable amount being handpaid.</param>
        /// <param name="promotionalAmount">The promotional amount being handpaid.</param>
        public HardCashLockoutEvent(long cashableAmount, long nonCashableAmount, long promotionalAmount)
        {
            CashableAmount = cashableAmount;
            NonCashableAmount = nonCashableAmount;
            PromotionalAmount = promotionalAmount;
        }

        /// <summary>
        ///     Gets the cashable amount being handpaid.
        /// </summary>
        public long CashableAmount { get; }

        /// <summary>
        ///     Gets the noncashable amount being handpaid.
        /// </summary>
        public long NonCashableAmount { get; }

        /// <summary>
        ///     Gets the promotional amount being handpaid.
        /// </summary>
        public long PromotionalAmount { get; }
    }
}