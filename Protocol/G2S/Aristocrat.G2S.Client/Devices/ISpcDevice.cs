namespace Aristocrat.G2S.Client.Devices
{
    using System.Threading.Tasks;
    using Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to interact with and control a SpcDevice device.
    /// </summary>
    public interface ISpcDevice : IDevice, IRestartStatus, ITimeToLive, ITransactionLogProvider
    {
        /// <summary>
        ///     Gets the controller type.
        /// </summary>
        string ControllerType { get; }

        /// <summary>
        ///     Gets the level ID.
        /// </summary>
        int LevelId { get; }

        /// <summary>
        ///     Gets the reset amount.
        /// </summary>
        int ResetAmount { get; }

        /// <summary>
        ///     Gets the maximum level amount.
        /// </summary>
        int MaxLevelAmount { get; }

        /// <summary>
        ///     Gets the contribution percentage.
        /// </summary>
        int ContribPercent { get; }

        /// <summary>
        ///     A value that indicates whether rounding is enabled.
        /// </summary>
        bool RoundingEnabled { get; }

        /// <summary>
        ///     Gets the mystery minimum.
        /// </summary>
        int MysteryMinimum { get; }

        /// <summary>
        ///     Gets the mystery maximum.
        /// </summary>
        int MysteryMaximum { get; }

        /// <summary>
        ///     Gets game play ID.
        /// </summary>
        int GamePlayId { get; }

        /// <summary>
        ///     Gets win level index.
        /// </summary>
        int WinLevelIndex { get; }

        /// <summary>
        ///     Gets the paytable ID.
        /// </summary>
        string PaytableId { get; }

        /// <summary>
        ///     Gets the theme ID.
        /// </summary>
        string ThemeId { get; }

        /// <summary>
        ///     Gets the denomination ID.
        /// </summary>
        int DenomId { get; }

        /// <summary>
        ///     Reports the reset of a standalone progressive level.
        /// </summary>
        /// <param name="command"><see cref="spcLevelReset"/> command to send to host.</param>
        /// <returns><see cref="Task"/> value that indicates whether the command was sent and acknowledged by the host.</returns>
        Task<bool> LevelReset(spcLevelReset command);
    }
}
