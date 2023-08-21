namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;

    /// <summary>
    ///     Event arguments for storage related events
    /// </summary>
    public class StorageEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StorageEventArgs" /> class.
        /// </summary>
        /// <param name="level">The level being cleared</param>
        public StorageEventArgs(PersistenceLevel level)
        {
            Level = level;
        }

        /// <summary>
        ///     Gets the level
        /// </summary>
        public PersistenceLevel Level { get; }
    }
}