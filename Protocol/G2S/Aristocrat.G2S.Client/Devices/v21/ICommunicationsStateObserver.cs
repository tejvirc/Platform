namespace Aristocrat.G2S.Client.Devices.v21
{
    using Protocol.v21;

    /// <summary>
    ///     Provides a mechanism for push-based notifications when the communications state changes.
    /// </summary>
    public interface ICommunicationsStateObserver
    {
        /// <summary>
        ///     Notifies the observer that the transport has changed.
        /// </summary>
        /// <param name="device">The communications device.</param>
        /// <param name="state">The new t_commsStates.</param>
        void Notify(ICommunicationsDevice device, t_commsStates state);
    }
}