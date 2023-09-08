namespace Aristocrat.G2S.Client.Configuration
{
    using System;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    ///     Describes the binding information for an endpoint
    /// </summary>
    public interface IBindingInfo
    {
        /// <summary>
        ///     Gets or sets the address
        /// </summary>
        Uri Address { get; set; }

        /// <summary>
        ///     Gets or sets the certificate
        /// </summary>
        X509Certificate2 Certificate { get; set; }

        /// <summary>
        ///     GDetermines whether or not to bypass SSL certificate validation for this host
        /// </summary>
        bool BypassCertificateValidation { get; set; }

        /// <summary>
        ///     Gets the custom validator, if any
        /// </summary>
        X509CertificateValidator Validator { get; set; }
    }
}