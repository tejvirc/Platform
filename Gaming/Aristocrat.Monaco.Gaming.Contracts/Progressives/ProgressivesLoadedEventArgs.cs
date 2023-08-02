namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;
    using System.Collections.Generic;
    using Progressives;

    /// <summary>
    ///     Progressives loaded event args
    /// </summary>
    public class ProgressivesLoadedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressivesLoadedEventArgs" /> class.
        /// </summary>
        /// <param name="progressiveLevels">The loaded progressive levels</param>
        public ProgressivesLoadedEventArgs(IReadOnlyCollection<ProgressiveLevel> progressiveLevels)
        {
            ProgressiveLevels = progressiveLevels;  
        }

        /// <summary>
        ///     Gets the games being added
        /// </summary>
        public IReadOnlyCollection<ProgressiveLevel> ProgressiveLevels { get; }
    }
}