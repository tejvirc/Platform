namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Threading.Tasks;
    using Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to interact with and control a BonusDevice device.
    /// </summary>
    public interface IBonusDevice : IDevice, IIdReaderId, IRestartStatus, ITimeToLive, INoResponseTimer
    {
        /// <summary>
        ///     Gets a value indicating whether or not the communications from the bonus host are active
        /// </summary>
        bool HostActive { get; }

        /// <summary>
        ///     Gets a value indicating whether or not bonus monitoring is active
        /// </summary>
        bool BonusActive { get; }

        /// <summary>
        ///     Gets the text message to display while host communications are lost
        /// </summary>
        string NoHostText { get; }

        /// <summary>
        ///     Gets a value indicating whether or not the ID reader associated with the currently active player session should be
        ///     used
        /// </summary>
        bool UsePlayerIdReader { get; }

        /// <summary>
        ///     Gets the time from the last game started that an eligibility-tested setBonusAward command will be paid
        /// </summary>
        TimeSpan EligibilityTimer { get; }

        /// <summary>
        ///     Gets the maximum bonus award amount that the EGM will pay
        /// </summary>
        long DisplayLimit { get; }

        /// <summary>
        ///     Gets the alternate text to display when a bonus award exceeds the maximum
        /// </summary>
        string DisplayLimitText { get; }

        /// <summary>
        ///     Gets the length of time to display the Maximum bonus message
        /// </summary>
        TimeSpan DisplayLimitDuration { get; }

        /// <summary>
        ///     Gets a value indicating whether or not the a card is required for wager match
        /// </summary>
        bool WagerMatchCardRequired { get; }

        /// <summary>
        ///     Gets the Wager Match limit
        /// </summary>
        long WagerMatchLimit { get; }

        /// <summary>
        ///     Gets the Wager Match limit message
        /// </summary>
        string WagerMatchLimitText { get; }

        /// <summary>
        ///     Gets the Wager Match limit message duration
        /// </summary>
        TimeSpan WagerMatchLimitDuration { get; }

        /// <summary>
        ///     Gets the Wager Match Exit message
        /// </summary>
        string WagerMatchExitText { get; }

        /// <summary>
        ///     Gets the Wager Match Exit message duration
        /// </summary>
        TimeSpan WagerMatchExitDuration { get; }

        /// <summary>
        ///     Used to notify the bonus device that the host is active.
        /// </summary>
        void NotifyActive();

        /// <summary>
        ///     Used to notify the bonus device that the host is active.
        /// </summary>
        /// <param name="enabled">true if monitoring is active</param>
        /// <param name="onStateChange">Callback for state change</param>
        void SetKeepAlive(bool enabled, Action<bool> onStateChange);

        /// <summary>
        ///     Used to report that a bonus has been awarded
        /// </summary>
        /// <returns></returns>
        Task<bool> CommitBonus(commitBonus commitBonus);
    }
}