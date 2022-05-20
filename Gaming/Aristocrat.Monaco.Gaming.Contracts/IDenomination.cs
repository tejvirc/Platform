namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;

    /// <summary>
    ///     Defines a denomination
    /// </summary>
    public interface IDenomination : IGameConfiguration
    {
        /// <summary>
        ///     Gets the identifier of the denomination
        /// </summary>
        /// <value>
        ///     The identifier of the denomination
        /// </value>
        long Id { get; }

        /// <summary>
        ///     Gets the value in millicents of each credit wagered as part of the game
        /// </summary>
        long Value { get; }

        /// <summary>
        ///     Gets a value indicating whether or not the denomination is active
        /// </summary>
        /// <value>
        ///     true, if the value is active otherwise false
        /// </value>
        bool Active { get; }

        /// <summary>
        ///     Gets a value indicating the amount of time that this denomination was active
        ///     This timespan does not include Now-ActiveDate.  Rather, it includes time that
        ///     this denomination was active before being de-activated (and possibly re-activated)
        /// </summary>
        TimeSpan PreviousActiveTime { get; }

        /// <summary>
        ///     Gets a value indicating the last time when this denomination became active
        /// </summary>
        DateTime ActiveDate { get; }

        /// <summary>
        ///     Gets whether or not the gamble feature is allowed
        /// </summary>
        bool SecondaryAllowed { get; set; }

        /// <summary>
        ///     Gets whether or not the Let It Ride feature is allowed
        /// </summary>
        bool LetItRideAllowed { get; set; }
    }
}