namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Provides a mechanism for push-based notifications when the egm state changes.
    /// </summary>
    public interface IEgmStateObserver
    {
        /// <summary>
        ///     Enables subscriptions observer and evaluates the current state of the egm.
        /// </summary>
        void Subscribe();

        /// <summary>
        ///     Disables subscriptions for the observer
        /// </summary>
        void Unsubscribe();

        /// <summary>
        ///     Notifies the observer that the egm enabled attribute has changed.
        /// </summary>
        /// <param name="device">The device causing the disabled or locked state.</param>
        /// <param name="enabled">The new enabled setting.</param>
        void NotifyEnabledChanged(IDevice device, bool enabled);

        /// <summary>
        ///     Notifies the observer that the egm state has changed.
        /// </summary>
        /// <param name="device">The device causing the disabled or locked state.</param>
        /// <param name="state">The new EgmState.</param>
        /// <param name="faultId">The fault identifier associated with the lock</param>
        void NotifyStateChanged(IDevice device, EgmState state, int faultId);

        /// <summary>
        ///     Notifies the observer that a new state was added.
        /// </summary>
        /// <param name="device">The device causing the disabled or locked state.</param>
        /// <param name="state">The new EgmState.</param>
        /// <param name="faultId">The fault identifier associated with the lock</param>
        void StateAdded(IDevice device, EgmState state, int faultId);

        /// <summary>
        ///     Notifies the observer that a state was removed.
        /// </summary>
        /// <param name="device">The device that is no longer causing the disabled or locked state.</param>
        /// <param name="state">The current EgmState.</param>
        /// <param name="faultId">The fault identifier associated with the lock</param>
        void StateRemoved(IDevice device, EgmState state, int faultId);
    }
}