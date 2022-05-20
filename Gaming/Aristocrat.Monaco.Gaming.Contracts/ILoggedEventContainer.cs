namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;
    using Models;

    /// <summary>
    ///     Provides a mechanism to interact with a bucket of events that happen during a game round
    /// </summary>
    public interface ILoggedEventContainer
    {
        /// <summary>
        ///     Gets the events associated to the game round
        /// </summary>
        IEnumerable<GameEventLogEntry> Events { get; }

        /// <summary>
        ///     Clear and return the accumulated events.
        /// </summary>
        /// <returns>A list of GameEventLogEntry</returns>
        List<GameEventLogEntry> HandOffEvents();
    }
}
