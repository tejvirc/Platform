namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Data.Model;

    /// <summary>
    ///     Provides a mechanism to initiate and track the disable procedures for the EGM
    /// </summary>
    public interface IDisableConditionSaga
    {
        /// <summary>
        ///     Gets the last date/time the EGM was disabled
        /// </summary>
        DateTime Disabled { get; }

        /// <summary>
        ///     Initiates the disable procedures based on the specified condition
        /// </summary>
        /// <param name="device">The device attempting to disable the EGM</param>
        /// <param name="condition">The disable condition</param>
        /// <param name="timeToLive">The time to wait before the request expires</param>
        /// <param name="message">The message to display while the EGM is disabled</param>
        /// <param name="onDisable">The callback used indicate whether or not the system was disabled. True if disabled.</param>
        /// <param name="state">The state to use when the cabinet is disabled</param>
        /// <param name="notify">Notify the device when setting it disabled.</param>
        void Enter(
            IDevice device,
            DisableCondition condition,
            TimeSpan timeToLive,
            Func<string> message,
            Action<bool> onDisable,
            EgmState state = EgmState.HostLocked,
            bool notify = false);

        /// <summary>
        ///     Initiates the disable procedures based on the specified condition
        /// </summary>
        /// <param name="device">The device attempting to disable the EGM</param>
        /// <param name="condition">The disable condition</param>
        /// <param name="timeToLive">The time to wait before the request expires</param>
        /// <param name="message">The message to display while the EGM is disabled</param>
        /// <param name="persist">
        ///     true if the state should be persisted.  If it's persisted the state will be recovered when
        ///     calling <see cref="IDisableConditionSaga.Reenter" />
        /// </param>
        /// <param name="onDisable">The callback used indicate whether or not the system was disabled. True if disabled.</param>
        /// <remarks>
        ///     Persisting the state does not apply to queued requests. It's only applied for requests that obtain a lock.  This
        ///     should only be used for very specific cases, like the host explicitly locking the cabinet
        ///     (EnterOptionConfig/EnterCommConfig)
        /// </remarks>
        void Enter(
            IDevice device,
            DisableCondition condition,
            TimeSpan timeToLive,
            Func<string> message,
            bool persist,
            Action<bool> onDisable);

        /// <summary>
        ///     Terminates the disable procedures based on the specified condition
        /// </summary>
        /// <param name="device">The device attempting to disable the EGM</param>
        /// <param name="condition">The disable condition</param>
        /// <param name="timeToLive">The time to wait before the request expires</param>
        /// <param name="onEnable">The callback used indicate whether or not the system was enabled. True if enabled.</param>
        void Exit(IDevice device, DisableCondition condition, TimeSpan timeToLive, Action<bool> onEnable);

        /// <summary>
        ///     Gets a value indicating whether the requested condition has been enabled
        /// </summary>
        /// <param name="device">The device attempting to disable the egm</param>
        /// <returns>true if the disable condition has been met for this device</returns>
        bool Enabled(IDevice device);

        /// <summary>
        ///     Initializes the saga.  This will typically result in restoring the previous state if locked/entered
        /// </summary>
        void Reenter();
    }
}
