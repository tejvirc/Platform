namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Provides a mechanism to interact with the informedPlayer device
    /// </summary>
    public interface IInformedPlayerDevice : IDevice, IRestartStatus
    {
        /// <summary>
        ///     Gets or sets the player to use for player tracking
        /// </summary>
        IPlayerDevice Player { get; set; }

        /// <summary>
        ///     Get an IClass for this device
        /// </summary>
        IClass InformedPlayerClassInstance { get; }

        /// <summary>
        ///     Gets the time-to-live value for requests originated by the device.
        /// </summary>
        int TimeToLive { get; }

        /// <summary>
        ///     Whether the host comms have been active
        /// </summary>
        bool HostActive { get; set; }

        /// <summary>
        ///     Whether game play is enabled from this device
        /// </summary>
        bool GamePlayEnabled { get; set; }

        /// <summary>
        ///     Whether money-in is enabled from this device
        /// </summary>
        bool MoneyInEnabled { get; set; }

        /// <summary>
        ///     The maximum time period without comms before IP functionality is disabled (ms).
        /// </summary>
        int NoMessageTimer { get; set; }

        /// <summary>
        ///     The message to display while IP functionality is disabled due to loss of comms.
        /// </summary>
        string NoHostText { get; set; }

        /// <summary>
        ///     Whether to enable money-in devices for un-carded sessions.
        /// </summary>
        bool UnCardedMoneyIn { get; set; }

        /// <summary>
        ///     Whether to enable game-play for un-carded sessions.
        /// </summary>
        bool UnCardedGamePlay { get; set; }

        /// <summary>
        ///     Whether to enable money-in devices upon player session start.
        /// </summary>
        bool SessionStartMoneyIn { get; set; }

        /// <summary>
        ///     Whether to enable game-play upon player session start.
        /// </summary>
        bool SessionStartGamePlay { get; set; }

        /// <summary>
        ///     Whether to force a cash-out if current balance exists, upon player session start.
        /// </summary>
        bool SessionStartCashOut { get; set; }

        /// <summary>
        ///     Whether to force a cash-out if current balance exists, upon player session end.
        /// </summary>
        bool SessionEndCashOut { get; set; }

        /// <summary>
        ///     Whether PIN entry is required for some players.
        /// </summary>
        bool SessionStartPinEntry { get; set; }

        /// <summary>
        ///     The amount at which player wagers cause reporting events (unless 0).
        /// </summary>
        long SessionLimit { get; set; }

        /// <summary>
        ///     The amount at which player wagers cause reporting events (unless 0),
        ///     after player session start.
        /// </summary>
        long SessionStartLimit { get; set; }
    }
}
