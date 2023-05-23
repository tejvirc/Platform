namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Communicator.ServiceModel;

    using CoreWCF;
    using CoreWCF.Configuration;
    using CoreWCF.Description;
    using CoreWCF.IdentityModel.Selectors;
    using CoreWCF.Security;

    /// <summary>
    ///     Defines an instance of an IReceiveEndpoint.
    /// </summary>
    public class ReceiveEndpoint : IReceiveEndpoint
    {
        private readonly X509Certificate2 _certificate;
        private readonly CoreWCF.Description.ServiceEndpoint _endpoint;
        private readonly X509CertificateValidator _validator;

        private bool _disposed;
        //private System.ServiceModel.ICommunicationObject _serviceHost;
        private IWcfApplicationRuntime _app;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReceiveEndpoint" /> class.
        /// </summary>
        /// <param name="address">The address to listen on.</param>
        /// <param name="certificate">The certificate</param>
        /// <param name="validator">An optional <see cref="X509CertificateValidator" /> validator</param>
        /// <param name="app"></param>
        public ReceiveEndpoint(
            Uri address,
            X509Certificate2 certificate,
            X509CertificateValidator validator,
            IWcfApplicationRuntime app)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (!EndpointUtilities.IsSchemeValid(address))
            {
                throw new ArgumentException(@"The Uri scheme is not valid it must be https or http.", nameof(address));
            }

            _certificate = certificate;
            _validator = validator;
            _app = app ?? throw new ArgumentNullException(nameof(app));

            _endpoint = new CoreWCF.Description.ServiceEndpoint(
                ContractDescription.GetContract<IG2SService>(typeof(IG2SService)),
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

            //var serviceHost = new System.ServiceModel.ServiceHost(_service, Address);
            //serviceHost.Description.Endpoints.Add(_endpoint);

            RegisterEndpoint(_endpoint.Address.Uri, _certificate, _validator);


#if !(DEBUG)
            // This overrides the https binding, which somewhat satisfies a request by the operator see VLT-6118 for more info
            //var debugBehavior = serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
            var debugBehavior = _app.GetRequiredService<ServiceDebugBehavior>();
            if (debugBehavior != null && Address.IsSecure())
            {
                debugBehavior.HttpHelpPageEnabled = false;
                debugBehavior.HttpsHelpPageEnabled = true;
            }
#endif

            _app.Start();
        }

        /// <summary>
        /// Solution 2
        /// </summary>
        /// <param name="address"></param>
        /// <param name="certificate"></param>
        /// <param name="validator"></param>
        private void RegisterEndpoint(
            Uri address,
            X509Certificate2 certificate,
            X509CertificateValidator validator)
        {
            _app.UseServiceModel(builder =>
            {
                var serviceBuilder = builder.AddService<G2SService>((options) => { });
                const int maxReceiveMessageSize = 2147483647;

                if (!address.IsSecure())
                {
                    // Add a BasicHttpBinding at a specific endpoint
                    serviceBuilder.AddServiceEndpoint<G2SService, IG2SService>(new BasicHttpBinding { MaxReceivedMessageSize = maxReceiveMessageSize }, address);
                }
                else
                {
                    // Configure an explicit none credential type for WSHttpBinding as it defaults to Windows which requires extra configuration in ASP.NET
                    var myWSHttpBinding = new WSHttpBinding(SecurityMode.Transport);
                    myWSHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                    myWSHttpBinding.MaxReceivedMessageSize = maxReceiveMessageSize;
                    // Add a WSHttpBinding with Transport Security for TLS
                    serviceBuilder.AddServiceEndpoint<G2SService, IG2SService>(myWSHttpBinding, address);
                }
                builder.ConfigureServiceHostBase<G2SService>(serviceHost =>
                {
                    serviceHost.Credentials.ServiceCertificate.Certificate = certificate;

                    if (validator == null)
                    {
                        serviceHost.Credentials.ClientCertificate.Authentication.CertificateValidationMode =
                            X509CertificateValidationMode.ChainTrust;
                    }
                    else
                    {
                        serviceHost.Credentials.ClientCertificate.Authentication.CertificateValidationMode =
                            X509CertificateValidationMode.Custom;
                        serviceHost.Credentials.ClientCertificate.Authentication.CustomCertificateValidator = validator;
                    }

                    serviceHost.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.Offline;
                });
            });
        }

        /// <inheritdoc />
        public void Close()
        {
            //_serviceHost.Close();
            _app.StopAsync().GetAwaiter().GetResult();
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
                //_serviceHost?.Close();
                _app.DisposeAsync().GetAwaiter().GetResult();
            }

            //_serviceHost = null;
            _app = null;

            _disposed = true;
        }
    }
}