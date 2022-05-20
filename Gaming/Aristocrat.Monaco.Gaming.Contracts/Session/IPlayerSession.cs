namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    using System;
    using Hardware.Contracts.IdReader;

    /// <summary>
    ///     Describes an active player session
    /// </summary>
    public interface IPlayerSession
    {
        /// <summary>
        ///     Gets the current player for the active player session
        /// </summary>
        Identity Player { get; }

        /// <summary>
        ///     Gets the start date/time for the player session
        /// </summary>
        DateTime Start { get; }

        /// <summary>
        ///     Gets the end date/time for the player session
        /// </summary>
        DateTime End { get; }

        /// <summary>
        ///     Gets the current point balance.  This includes the starting point balance and any points earned this session (<see cref="SessionPoints"/>)
        /// </summary>
        long PointBalance { get; }

        /// <summary>
        ///     Gets the current point countdown
        /// </summary>
        long PointCountdown { get; }

        /// <summary>
        ///     Gets the points earned this session
        /// </summary>
        long SessionPoints { get; }

        /// <summary>
        ///     Gets the log entry associated with the player session
        /// </summary>
        IPlayerSessionLog Log { get; }
    }
}
