namespace Aristocrat.G2S.Client.Configuration
{
    using System;
    using System.IdentityModel.Selectors;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    ///     Provides a mechanism to configure the receive endpoint.
    /// </summary>
    public interface IReceiveEndpointConfigurator
    {
        /// <summary>
        ///     Configures the address to listen on.
        /// </summary>
        /// <param name="address">The address to listen on</param>
        void ListenOn(Uri address);

        /// <summary>
        ///     Configures the address to listen on.
        /// </summary>
        /// <param name="address">The address to listen on</param>
        /// <param name="certificate">The certificate</param>
        /// <param name="bypassCertificateValidation">Determines whether or not to bypass SSL certificate validation</param>
        /// <param name="validator">An optional validator used to override the built-in certificate validation</param>
        void ListenOn(Uri address, X509Certificate2 certificate, bool bypassCertificateValidation, X509CertificateValidator validator);

        /// <summary>
        ///     Configures the address to listen on.
        /// </summary>
        /// <param name="configure">A delegate that is invoked allowing for configuration of the binding information.</param>
        void ListenOn(Action<IBindingInfo> configure);

        /// <summary>
        ///     Loads the G2S Assembly into memory to use to build messages.
        /// </summary>
        /// <param name="namespace">The namespace to load.</param>
        void UsesNamespace(string @namespace);

        /// <summary>
        ///     Loads the G2S Assembly into memory to use to build messages.
        /// </summary>
        /// <param name="namespace">The namespace to load.</param>
        /// <param name="assembly">The assembly to use to build messages.</param>
        void UsesNamespace(string @namespace, Assembly assembly);
    }
}