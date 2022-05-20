namespace Aristocrat.G2S.Client.Devices
{
    using Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to interact with and control a Meters device.
    /// </summary>
    public interface IMetersDevice : IDevice
    {
        /// <summary>
        ///     Sends meter info command to the host
        /// </summary>
        /// <param name="command">MeterInfo command</param>
        /// <param name="waitForAck">Wait for acknowledgement</param>
        /// <returns>Sent result and time to live.</returns>
        (bool, int) SendMeterInfo(meterInfo command, bool waitForAck);
    }
}