namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     The mediaDisplay class is used by a host to access display windows or media display on an EGM. The mediaDisplay
    ///     class is a multi-device class. Each configured mediaDisplay device used within an EGM is assigned its own deviceId.
    /// </summary>
    public interface IMediaDisplay : IDevice, IRestartStatus
    {
        /// <summary>
        ///     Gets the time-to-live value for requests originated by the device.
        /// </summary>
        int TimeToLive { get; }

        /// <summary>
        ///     Get an IClass for this device
        /// </summary>
        IClass MediaDisplayClassInstance { get; }
    }
}
