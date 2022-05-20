namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using Kernel;

    /// <summary>
    ///     Defines the base class for bonus events
    /// </summary>
    public abstract class BaseBonusEvent : BaseEvent
    {
        /// <summary>
        ///     Base class for bonus events
        /// </summary>
        /// <param name="transaction">The <see cref="BonusTransaction" /> associated with the event</param>
        protected BaseBonusEvent(BonusTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     Gets the associated bonus transaction
        /// </summary>
        public BonusTransaction Transaction { get; }
    }
}