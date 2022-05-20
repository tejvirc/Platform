namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    using System;
    using Hardware.Contracts.IdReader;

    /// <summary>
    ///     Indicates the states of the game
    /// </summary>
    [Flags]
    public enum PlayerStatus
    {
        /// <summary>
        ///     Indicates the player service is enabled
        /// </summary>
        None = 0,

        /// <summary>
        ///     Indicates disabled by the System/EGM
        /// </summary>
        DisabledBySystem = 1,

        /// <summary>
        ///     Indicates disabled by the backend
        /// </summary>
        DisabledByBackend = 2
    }

    /// <summary>
    ///     Provides a mechanism to interact with the player service
    /// </summary>
    public interface IPlayerService
    {
        /// <summary>
        ///     Gets a value indicating whether there is an active player session
        /// </summary>
        bool HasActiveSession { get; }

        /// <summary>
        ///     Returns the current player session
        /// </summary>
        IPlayerSession ActiveSession { get; }

        /// <summary>
        ///     Gets a value indicating whether player sessions are enabled
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        ///     Gets the status of the player service
        /// </summary>
        PlayerStatus Status { get; }

        /// <summary>
        ///     Gets the player options
        /// </summary>
        PlayerOptions Options { get; set; }

        /// <summary>
        ///     Sets the session parameters for the active session
        /// </summary>
        /// <param name="transactionId">The session transaction identifier</param>
        /// <param name="pointBalance">The current point balance</param>
        /// <param name="overrideId">
        ///     The identifier for the set of generic override parameters that should be used. Set to 0 (zero)
        ///     if no override should be in use
        /// </param>
        void SetSessionParameters(long transactionId, long pointBalance, long overrideId);

        /// <summary>
        ///     Sets the session parameters for the active session
        /// </summary>
        /// <param name="transactionId">The session transaction identifier</param>
        /// <param name="pointBalance">The current point balance</param>
        /// <param name="carryOver">The carry over value</param>
        /// <param name="overrideParameters">The optional override parameters</param>
        void SetSessionParameters(
            long transactionId,
            long pointBalance,
            long carryOver,
            GenericOverrideParameters overrideParameters);

        /// <summary>
        ///     Sets the identity parameter for the active session.
        /// </summary>
        /// <param name="identity">Identity from the ID Reader.</param>
        void SetSessionParameters(Identity identity);

        /// <summary>
        ///     Marks the session as committed
        /// </summary>
        /// <param name="transactionId">The session transaction identifier</param>
        void CommitSession(long transactionId);

        /// <summary>
        ///     Enables the player session tracking with the provided reason
        /// </summary>
        /// <param name="status">The </param>
        void Enable(PlayerStatus status);

        /// <summary>
        /// </summary>
        /// <param name="status"></param>
        void Disable(PlayerStatus status);

        /// <summary>
        ///     Gets the specified award parameters if configured
        /// </summary>
        /// <typeparam name="TParam">The parameter type</typeparam>
        /// <returns>The award parameters if configured</returns>
        TParam GetParameters<TParam>()
            where TParam : AwardParameters;
    }
}
