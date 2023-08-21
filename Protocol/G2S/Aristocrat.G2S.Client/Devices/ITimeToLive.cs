namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Provides a mechanism for controlling the time-to-live value for requests originated by the device.
    /// </summary>
    public interface ITimeToLive
    {
        /// <summary>
        ///     Gets the time-to-live value for requests originated by the device.
        /// </summary>
        int TimeToLive { get; }
    }
}
