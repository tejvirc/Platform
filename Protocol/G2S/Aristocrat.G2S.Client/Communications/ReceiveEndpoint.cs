namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using Communicator.ServiceModel;

    /// <summary>
    ///     Defines an instance of an IReceiveEndpoint.
    /// </summary>
    public class ReceiveEndpoint : IReceiveEndpoint
    {
        private readonly X509Certificate2 _certificate;
        private readonly ServiceEndpoint _endpoint;
        private readonly IG2SService _service;
        private readonly X509CertificateValidator _validator;

        private bool _disposed;
        private ICommunicationObject _serviceHost;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReceiveEndpoint" /> class.
        /// </summary>
        /// <param name="service">An instance of the IG2SService.</param>
        /// <param name="address">The address to listen on.</param>
        /// <param name="certificate">The certificate</param>
        /// <param name="validator">An optional <see cref="X509CertificateValidator" /> validator</param>
        public ReceiveEndpoint(
            IG2SService service,
            Uri address,
            X509Certificate2 certificate,
            X509CertificateValidator validator)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (!EndpointUtilities.IsSchemeValid(address))
            {
                throw new ArgumentException(@"The Uri scheme is not valid it must be https or http.", nameof(address));
            }

            _service = service ?? throw new ArgumentNullException(nameof(service));
            _certificate = certificate;
            _validator = validator;

            _endpoint = new ServiceEndpoint(
                ContractDescription.GetContract(typeof(IG2SService)),
                EndpointUtilities.Binding(address),
                new EndpointAddress(address));
        }

        /// <inheritdoc />
        public Uri Address => _endpoint?.Address.Uri;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Open()
        {
            if (Address == null)
            {
                throw new EndpointNotFoundException($"The client address {nameof(Address)} has not been specified.");
            }

            var serviceHost = new ServiceHost(_service, Address);

            var https = _endpoint.Address.Uri.IsSecure();

            serviceHost.Description.Endpoints.Add(_endpoint);

#if !(DEBUG)
            // This overrides the https binding, which somewhat satisfies a request by the operator see VLT-6118 for more info
            var debugBehavior = serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (debugBehavior != null && Address.IsSecure())
            {
                var customBinding = new System.ServiceModel.Channels.CustomBinding();

                customBinding.Elements.Add(new System.ServiceModel.Channels.HttpsTransportBindingElement
                {
                    RequireClientCertificate = true
                });

                debugBehavior.HttpsHelpPageBinding = customBinding;
            }
#endif
            serviceHost.Description.Behaviors.Add(
                new ServiceMetadataBehavior { HttpGetEnabled = !https, HttpsGetEnabled = https });

            serviceHost.AddServiceEndpoint(
                typeof(IMetadataExchange),
                https
                    ? MetadataExchangeBindings.CreateMexHttpsBinding()
                    : MetadataExchangeBindings.CreateMexHttpBinding(),
                @"mex");

            serviceHost.Credentials.ServiceCertificate.Certificate = _certificate;
            if (_validator == null)
            {
                serviceHost.Credentials.ClientCertificate.Authentication.CertificateValidationMode =
                    X509CertificateValidationMode.ChainTrust;
            }
            else
            {
                serviceHost.Credentials.ClientCertificate.Authentication.CertificateValidationMode =
                    X509CertificateValidationMode.Custom;
                serviceHost.Credentials.ClientCertificate.Authentication.CustomCertificateValidator = _validator;
            }

            serviceHost.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.Offline;

            _serviceHost = serviceHost;

            _serviceHost.Open();
        }

        /// <inheritdoc />
        public void Close()
        {
            _serviceHost.Close();
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _serviceHost?.Close();
            }

            _serviceHost = null;

            _disposed = true;
        }
    }
}