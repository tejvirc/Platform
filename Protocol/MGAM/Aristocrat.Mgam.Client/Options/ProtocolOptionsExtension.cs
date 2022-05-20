namespace Aristocrat.Mgam.Client.Options
{
    using System.Net;
    using SimpleInjector;

    /// <summary>
    ///     EGM options extension.
    /// </summary>
    internal class ProtocolOptionsExtension : IOptionsExtension
    {
        /// <summary>
        ///     Gets or sets the directory address.
        /// </summary>
        public virtual IPEndPoint DirectoryAddress { get; private set; }

        /// <summary>
        ///     Gets or sets the Directory service response address.
        /// </summary>
        public virtual IPEndPoint DirectoryResponseAddress { get; private set; }

        /// <summary>
        ///     Gets or sets the network address.
        /// </summary>
        public virtual IPAddress NetworkAddress { get; private set; }

        /// <summary>
        ///     Gets or sets a flag that indicates whether the host server certificates is validated.
        /// </summary>
        public virtual bool ServerCertificateValidation { get; private set; }

        /// <summary>
        ///     Adds registrations to the container.
        /// </summary>
        /// <param name="container">Dependency injection container.</param>
        public void RegisterServices(Container container)
        {
            container.RegisterSingleton(() => new ProtocolOptions(container.GetInstance<ProtocolOptionsBuilder>()));
            container.RegisterSingleton<IOptions<ProtocolOptions>>(container.GetInstance<ProtocolOptions>);
            container.RegisterSingleton<IOptionsMonitor<ProtocolOptions>>(container.GetInstance<ProtocolOptions>);
        }

        /// <summary>
        ///     Sets the broadcast end-point.
        /// </summary>
        /// <param name="endPoint">Broadcast end-point.</param>
        /// <returns><see cref="ProtocolOptionsExtension"/>.</returns>
        public ProtocolOptionsExtension WithBroadcastOn(IPEndPoint endPoint)
        {
            DirectoryAddress = endPoint;
            return this;
        }

        /// <summary>
        ///     Sets the broadcast response end-point.
        /// </summary>
        /// <param name="address">Network address.</param>
        /// <returns><see cref="ProtocolOptionsExtension"/>.</returns>
        public ProtocolOptionsExtension WithReceiveBroadcastOn(IPEndPoint address)
        {
            DirectoryResponseAddress = address;
            return this;
        }

        /// <summary>
        ///     Sets the network address.
        /// </summary>
        /// <param name="address">Network address.</param>
        /// <returns><see cref="ProtocolOptionsExtension"/>.</returns>
        public ProtocolOptionsExtension WithNetworkAddress(IPAddress address)
        {
            NetworkAddress = address;
            return this;
        }

        /// <summary>
        ///     Sets a flag that indicates whether server certificate validation is enabled.
        /// </summary>
        /// <param name="enable">Enable/Disable server certificate validation.</param>
        /// <returns><see cref="ProtocolOptionsExtension"/>.</returns>
        public ProtocolOptionsExtension WithServerCertificateValidation(bool enable)
        {
            ServerCertificateValidation = enable;
            return this;
        }
    }
}
