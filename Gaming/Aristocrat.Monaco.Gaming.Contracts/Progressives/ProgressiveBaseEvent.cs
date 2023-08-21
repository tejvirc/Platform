namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;
    using Kernel;

    /// <summary>
    ///     Defines a base type for all progressive events
    /// </summary>
    public abstract class ProgressiveBaseEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveBaseEvent" /> class.
        /// </summary>
        /// <param name="jackpot">The associated jackpot transaction</param>
        protected ProgressiveBaseEvent(ICloneable jackpot)
        {
            Jackpot = (JackpotTransaction)jackpot.Clone();
        }

        /// <summary>
        ///     The associated jackpot transaction
        /// </summary>
        public JackpotTransaction Jackpot { get; }
    }
}