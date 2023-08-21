namespace Aristocrat.Monaco.Gaming.Contracts.Meters
{
    using System;
    using System.Collections.Generic;
    using Progressives;

    /// <summary>
    ///     Progressive Added event args
    /// </summary>
    public class ProgressiveAddedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveAddedEventArgs" /> class.
        /// </summary>
        /// <param name="progressives">The progressives added.</param>
        public ProgressiveAddedEventArgs(IReadOnlyCollection<IViewableProgressiveLevel> progressives)
        {
            Progressives = progressives;
        }

        /// <summary>
        ///     Gets the added progressives
        /// </summary>
        public IReadOnlyCollection<IViewableProgressiveLevel> Progressives { get; }
    }
}