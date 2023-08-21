namespace Aristocrat.G2S.Client
{
    using System;
    using Configuration;
    using Devices;

    /// <summary>
    ///     Definition of a G2S Client
    /// </summary>
    public interface IClient :
        IHostConnector,
        IDeviceConnector,
        IHandlerConnector,
        IHostConfigurator,
        IMessageObservable
    {
        /// <summary>
        ///     Gets the address of the client.
        /// </summary>
        Uri Address { get; }
    }
}