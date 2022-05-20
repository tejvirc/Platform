namespace Aristocrat.G2S.Client.Security
{
    using System;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    ///     Defines a record in the SSL configuration store
    /// </summary>
    public class CertificateBinding
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateBinding" /> class.
        /// </summary>
        /// <param name="certificateThumbprint">The certificate thumbprint</param>
        /// <param name="certificateStoreName">The certificate store</param>
        /// <param name="endpoint">The Ip Port</param>
        /// <param name="appId">The application identifier</param>
        /// <param name="options">The options</param>
        public CertificateBinding(
            string certificateThumbprint,
            StoreName certificateStoreName,
            IPEndPoint endpoint,
            Guid appId,
            BindingOptions options = null)
            : this(certificateThumbprint, certificateStoreName.ToString(), endpoint, appId, options)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateBinding" /> class.
        /// </summary>
        /// <param name="certificateThumbprint">The certificate thumbprint</param>
        /// <param name="certificateStoreName">The certificate store</param>
        /// <param name="endpoint">The Ip Port</param>
        /// <param name="appId">The application identifier</param>
        /// <param name="options">The options</param>
        public CertificateBinding(
            string certificateThumbprint,
            string certificateStoreName,
            IPEndPoint endpoint,
            Guid appId,
            BindingOptions options = null)
        {
            if (certificateThumbprint == null)
            {
                throw new ArgumentNullException(nameof(certificateThumbprint));
            }

            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (certificateStoreName == null)
            {
                // StoreName of null is assumed to be My / Personal
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364647(v=vs.85).aspx
                certificateStoreName = "MY";
            }

            Thumbprint = certificateThumbprint;
            StoreName = certificateStoreName;
            Endpoint = endpoint;
            AppId = appId;
            Options = options ?? new BindingOptions();
        }

        /// <summary>
        ///     Gets a string representation of the SSL certificate hash.
        /// </summary>
        public string Thumbprint { get; }

        /// <summary>
        ///     Gets the name of the store from which the server certificate is to be read. If set to NULL, "MY" is assumed as the
        ///     default name.
        ///     The specified certificate store name must be present in the Local Machine store location.
        /// </summary>
        public string StoreName { get; }

        /// <summary>
        ///     Gets an IP address and port with which this SSL certificate is associated.
        ///     If the <see cref="IPEndPoint.Address" /> property is set to 0.0.0.0, the certificate is applicable to all IPv4 and
        ///     IPv6 addresses. If the <see cref="IPEndPoint.Address" /> property is set to [::], the certificate is applicable to
        ///     all IPv6 addresses.
        /// </summary>
        public IPEndPoint Endpoint { get; }

        /// <summary>
        ///     Gets a unique identifier of the application setting this record.
        /// </summary>
        public Guid AppId { get; }

        /// <summary>
        ///     Gets additional options.
        /// </summary>
        public BindingOptions Options { get; }
    }
}