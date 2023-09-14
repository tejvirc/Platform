namespace Aristocrat.Monaco.Gaming.Contracts.Meters
{
    using System;
    using System.Collections.Generic;
    using Progressives.Linked;

    /// <summary>
    ///     Progressive Added event args
    /// </summary>
    public class LinkedProgressiveAddedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LinkedProgressiveAddedEventArgs" /> class.
        /// </summary>
        /// <param name="progressives">The progressives added.</param>
        public LinkedProgressiveAddedEventArgs(IReadOnlyCollection<IViewableLinkedProgressiveLevel> progressives)
        {
            Progressives = progressives;
        }

        /// <summary>
        ///     Gets the added progressives
        /// </summary>
        public IReadOnlyCollection<IViewableLinkedProgressiveLevel> Progressives { get; }
    }
}