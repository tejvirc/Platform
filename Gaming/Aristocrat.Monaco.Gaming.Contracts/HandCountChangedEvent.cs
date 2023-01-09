namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A Hand Count Changed Event is posted when a then hand count changed.  This typically occurs when a new primary game started,
    ///     the hand count reset or cashout 
    /// </summary>
    [Serializable]
    public class HandCountChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HandCountChangedEvent" /> class.
        /// </summary>
        /// <param name="handCount"> Current hand count.</param>
        public HandCountChangedEvent(int handCount)
        {
            HandCount = handCount;
        }

        /// <summary>
        ///     Gets the current hand count.
        /// </summary>
        public int HandCount { get; }
    }
}