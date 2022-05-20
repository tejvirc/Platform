namespace Aristocrat.G2S.Client.Devices.v21
{
    using Protocol.v21;

    /// <summary>
    ///     Provides a mechanism for push-based notifications when the transport state changes.
    /// </summary>
    public interface ITransportStateObserver
    {
        /// <summary>
        ///     Notifies the observer that the transport has changed.
        /// </summary>
        /// <param name="device">The communications device.</param>
        /// <param name="state">The new t_transportStates.</param>
        void Notify(ICommunicationsDevice device, t_transportStates state);
    }
}