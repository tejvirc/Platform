namespace Aristocrat.G2S.Client.Configuration
{
    using System;
    using System.IdentityModel.Selectors;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using Communications;
    using Communicator.ServiceModel;
    using Devices;

    /// <summary>
    ///     An implementation of <see cref="IEgmFactoryConfigurator" />
    /// </summary>
    internal class EgmFactoryConfigurator : IEgmFactoryConfigurator, IDisposable
    {
        private Uri _address;
        private X509Certificate2 _certificate;
        private X509CertificateValidator _certificateValidator;
        private string _egmId;
        private MessageBuilder _messageBuilder;
        private bool _namespaceLoaded;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EgmFactoryConfigurator" /> class.
        /// </summary>
        public EgmFactoryConfigurator()
        {
            _messageBuilder = new MessageBuilder();
            _messageBuilder.LoadSecurityNamespace(SchemaVersion.m105, null);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void WithEgmId(string egmId)
        {
            _egmId = egmId;
        }

        /// <inheritdoc />
        public IClientControl Build()
        {
            // Load the default schema if needed
            if (!_namespaceLoaded)
            {
                UsesNamespace(Constants.DefaultSchema);
            }

            var egm = new EgmIdentifier(string.IsNullOrEmpty(_egmId) ? BuildEgmId() : _egmId);

            // Most (not all) are all meant to be extensibility points.
            // Just need to expose them as needed.
            var endpointProvider = new SendEndpointProvider(_messageBuilder, _certificate);
            var receiveEndpointProvider = new ReceiveEndpointProvider(_messageBuilder);

            var idProvider = new IdProvider<int>();
            var deviceConnector = new DeviceConnector();
            var handlerConnector = new HandlerConnector();
            var commandDispatcher = new CommandDispatcher(handlerConnector, deviceConnector);

            var service = new G2SService(receiveEndpointProvider);
            var receiver = new ReceiveEndpoint(service, _address, _certificate, _certificateValidator);
            var messageConsumer = new MessageConsumer(egm);
            receiveEndpointProvider.ConnectConsumer(messageConsumer);
            var mtpClient = new MtpClient();
            mtpClient.ConnectConsumer(messageConsumer);
            var hostConnector = new HostConnector(
                egm,
                endpointProvider,
                commandDispatcher,
                idProvider,
                messageConsumer);

            return new G2SEgm(egm.Id, hostConnector, deviceConnector, handlerConnector, receiver, mtpClient);
        }

        /// <inheritdoc />
        public void ListenOn(Uri address)
        {
            ListenOn(address, null, null);
        }

        /// <inheritdoc />
        public void ListenOn(Uri address, X509Certificate2 certificate, X509CertificateValidator validator)
        {
            _address = address ?? throw new ArgumentNullException(nameof(address));
            _certificate = certificate;
            _certificateValidator = validator;
        }

        /// <inheritdoc />
        public void ListenOn(Action<IBindingInfo> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var bindingInfo = new BindingInfo();

            configure(bindingInfo);

            ListenOn(bindingInfo.Address, bindingInfo.Certificate, bindingInfo.Validator);
        }

        /// <inheritdoc />
        public void UsesNamespace(string @namespace)
        {
            UsesNamespace(@namespace, null);
        }

        /// <inheritdoc />
        public void UsesNamespace(string @namespace, Assembly assembly)
        {
            _namespaceLoaded = true;
            _messageBuilder.LoadNamespace(@namespace, assembly);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // ReSharper disable once UseNullPropagation
                if (_messageBuilder != null)
                {
                    _messageBuilder.Dispose();
                }
            }

            _messageBuilder = null;
            _certificate = null;

            _disposed = true;
        }

        private static string BuildEgmId()
        {
            var macAddress =
                NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                    .Select(nic => nic.GetPhysicalAddress().ToString())
                    .FirstOrDefault();

            return $"{Constants.ManufacturerPrefix}_{macAddress}";
        }

        private class EgmIdentifier : IEgm
        {
            public EgmIdentifier(string id)
            {
                Id = id;
            }

            public string Id { get; }
        }

        private class BindingInfo : IBindingInfo
        {
            public Uri Address { get; set; }

            public X509Certificate2 Certificate { get; set; }

            public X509CertificateValidator Validator { get; set; }
        }
    }
}