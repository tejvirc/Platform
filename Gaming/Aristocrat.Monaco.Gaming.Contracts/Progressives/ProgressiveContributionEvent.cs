namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     An event sent when progressive contributions are made.
    /// </summary>
    [Serializable]
    public class ProgressiveContributionEvent : BaseEvent
    {
        /// <summary>
        ///     Gets or sets the wager contributions
        /// </summary>
        public IEnumerable<long> Wagers { get; set; }
    }
}