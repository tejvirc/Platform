namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Provides a mechanism to interact with and control a coinAcceptor device.
    /// </summary>
    public interface ICoinAcceptor : IDevice, ISingleDevice, IRestartStatus
    {
    }
}