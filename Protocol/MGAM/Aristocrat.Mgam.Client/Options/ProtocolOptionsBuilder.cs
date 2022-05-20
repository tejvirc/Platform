namespace Aristocrat.Mgam.Client.Options
{
    using System;
    using System.Net;
    using SimpleInjector;

    /// <summary>
    ///     Configuration for Mgam client.
    /// </summary>
    public sealed class ProtocolOptionsBuilder : IOptionsBuilder
    {
        private readonly Container _container;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProtocolOptionsBuilder"/> class.
        /// </summary>
        /// <param name="container"></param>
        internal ProtocolOptionsBuilder(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));

            Options = new OptionsExtender();
        }

        /// <summary>
        ///     Gets or sets the set of options.
        /// </summary>
        public OptionsExtender Options { get; set; }

        /// <inheritdoc />
        public void AddOrUpdateExtension(IOptionsExtension extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (Options.FindExtension(extension.GetType()) == null)
            {
                extension.RegisterServices(_container);
            }

            Options = Options.WithExtension(extension);
        }

        /// <summary>
        ///     Sets the directory service address.
        /// </summary>
        /// <param name="endPoint">Directory service address.</param>
        public ProtocolOptionsBuilder BroadcastOn(IPEndPoint endPoint)
        {
            return WithOption(e => e.WithBroadcastOn(endPoint));
        }

        /// <summary>
        ///     Sets the the local network address.
        /// </summary>
        /// <param name="address">Local network address.</param>
        public ProtocolOptionsBuilder UseNetworkAddress(IPAddress address)
        {
            return WithOption(e => e.WithNetworkAddress(address));
        }

        /// <summary>
        ///     Enables server certificate validation.
        /// </summary>
        public ProtocolOptionsBuilder EnableServerCertificateValidation()
        {
            return WithOption(e => e.WithServerCertificateValidation(true));
        }

        private ProtocolOptionsBuilder WithOption(Func<ProtocolOptionsExtension, ProtocolOptionsExtension> withFunc)
        {
            AddOrUpdateExtension(
                withFunc(Options.FindExtension<ProtocolOptionsExtension>() ?? new ProtocolOptionsExtension()));

            return this;
        }
    }
}
