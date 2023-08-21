namespace Aristocrat.Monaco.Application.Contracts
{
    using System;

    /// <summary>
    ///     An event argument class used to carry data when a MeterChangedEvent is triggered
    /// </summary>
    public class MeterChangedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterChangedEventArgs" /> class.
        /// </summary>
        /// <param name="amount">The amount changed.</param>
        public MeterChangedEventArgs(long amount)
        {
            Amount = amount;
        }

        /// <summary>
        ///     Gets the changed amount.
        /// </summary>
        public long Amount { get; }
    }
}