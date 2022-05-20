namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    /// <summary>
    ///     The <see cref="MultipleJackpotTimeStartedEvent" /> is published when an MJT Session starts
    /// </summary>
    public class MultipleJackpotTimeStartedEvent : BaseBonusEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MultipleJackpotTimeStartedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The bonus transaction</param>
        public MultipleJackpotTimeStartedEvent(BonusTransaction transaction)
            : base(transaction)
        {
        }
    }
}