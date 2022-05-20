namespace Aristocrat.G2S.Client.Security
{
    using System;

    /// <summary>
    ///     Defines the binding options
    /// </summary>
    public class BindingOptions
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingOptions" /> class.
        /// </summary>
        public BindingOptions()
        {
            VerifyCertificateRevocation = true;
            UsageCheck = true;
            Mapped = false;
            NegotiateClientCertificate = false;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether revocation of client certificates is enabled or disabled.
        /// </summary>
        public bool VerifyCertificateRevocation { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether usage of only cached client certificate for revocation checking is enabled
        ///     or disabled.
        /// </summary>
        public bool VerifyRevocationWithCachedCertificateOnly { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the usage is checked.
        /// </summary>
        public bool UsageCheck { get; set; }

        /// <summary>
        ///     Gets or sets the time interval after which to check for an updated certificate revocation list (CRL). If this value
        ///     is zero, the new CRL is updated only when the previous one expires.
        /// </summary>
        public TimeSpan RevocationFreshnessTime { get; set; }

        /// <summary>
        ///     Gets or sets the timeout interval on attempts to retrieve the certificate revocation list for the remote URL.
        /// </summary>
        public TimeSpan UrlRetrievalTimeout { get; set; }

        /// <summary>
        ///     Gets or sets the certificate issuers that can be trusted. This list can be a subset of the certificate issuers that
        ///     are trusted by the computer.
        /// </summary>
        public string SslControlIdentifier { get; set; }

        /// <summary>
        ///     Gets or sets the store name under LOCAL_MACHINE where SslCtlIdentifier is stored.
        /// </summary>
        public string SslControlStoreName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether if true, client certificates are mapped where possible to corresponding
        ///     operating-system user accounts based on the certificate mapping rules stored in Active Directory.
        /// </summary>
        public bool Mapped { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether client certificates are negotiated.
        /// </summary>
        public bool NegotiateClientCertificate { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether prevents SSL requests from being passed to low-level Internet Server Application Program Interface filters.
        /// </summary>
        public bool DoNotPassRequestsToRawFilters { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the RevocationFreshnessTime setting is enabled.
        /// </summary>
        public bool EnableRevocationFreshnessTime { get; set; }
    }
}