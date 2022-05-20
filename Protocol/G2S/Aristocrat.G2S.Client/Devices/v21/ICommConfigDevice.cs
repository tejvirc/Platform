namespace Aristocrat.G2S.Client.Devices.v21
{
    using Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to interact with and control a commConfig device.
    /// </summary>
    public interface ICommConfigDevice : ICommConfigDevice<commHostList, commChangeStatus>
    {
    }
}