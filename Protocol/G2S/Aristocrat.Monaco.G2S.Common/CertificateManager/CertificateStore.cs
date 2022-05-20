namespace Aristocrat.Monaco.G2S.Common.CertificateManager
{
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    ///     Utility methods for interacting with the certificate store
    /// </summary>
    public static class CertificateStore
    {
        /// <summary>
        ///     Gets a certificate from the store for the specified find criteria.
        /// </summary>
        /// <param name="location">One of the enumeration values that specifies the location of the X.509 certificate store.</param>
        /// <param name="findType">One of the System.Security.Cryptography.X509Certificates.X509FindType values.</param>
        /// <param name="findValue">The search criteria as an object.</param>
        /// <returns>The certificate.</returns>
        public static X509Certificate2 Get(StoreLocation location, X509FindType findType, object findValue)
        {
            return Get(StoreName.My, location, findType, findValue);
        }

        /// <summary>
        ///     Gets a certificate from the store for the specified find criteria.
        /// </summary>
        /// <param name="name">One of the enumeration values that specifies the name of the X.509 certificate store.</param>
        /// <param name="location">One of the enumeration values that specifies the location of the X.509 certificate store.</param>
        /// <param name="findType">One of the System.Security.Cryptography.X509Certificates.X509FindType values.</param>
        /// <param name="findValue">The search criteria as an object.</param>
        /// <returns>The certificate.</returns>
        public static X509Certificate2 Get(
            StoreName name,
            StoreLocation location,
            X509FindType findType,
            object findValue)
        {
            using (var store = new X509Store(name, location))
            {
                store.Open(OpenFlags.ReadOnly);

                return store.Certificates.Find(findType, findValue, false)
                    .Cast<X509Certificate2>()
                    .SingleOrDefault();
            }
        }

        /// <summary>
        ///     Adds a certificate to the store if the certificate does not already exist
        /// </summary>
        /// <param name="location">One of the enumeration values that specifies the location of the X.509 certificate store.</param>
        /// <param name="findType">One of the System.Security.Cryptography.X509Certificates.X509FindType values.</param>
        /// <param name="findValue">The search criteria as an object.</param>
        /// <param name="certificateFactory">The function used to generate a certificate.</param>
        /// <returns>The certificate.</returns>
        public static X509Certificate2 GetOrAdd(
            StoreLocation location,
            X509FindType findType,
            object findValue,
            Func<X509Certificate2> certificateFactory)
        {
            return GetOrAdd(StoreName.My, location, findType, findValue, certificateFactory);
        }

        /// <summary>
        ///     Adds a certificate to the store if the certificate does not already exist
        /// </summary>
        /// <param name="name">One of the enumeration values that specifies the name of the X.509 certificate store.</param>
        /// <param name="location">One of the enumeration values that specifies the location of the X.509 certificate store.</param>
        /// <param name="findType">One of the System.Security.Cryptography.X509Certificates.X509FindType values.</param>
        /// <param name="findValue">The search criteria as an object.</param>
        /// <param name="certificateFactory">The function used to generate a certificate.</param>
        /// <returns>The certificate.</returns>
        public static X509Certificate2 GetOrAdd(
            StoreName name,
            StoreLocation location,
            X509FindType findType,
            object findValue,
            Func<X509Certificate2> certificateFactory)
        {
            if (certificateFactory == null)
            {
                throw new ArgumentNullException(nameof(certificateFactory));
            }

            X509Certificate2 certificate;

            using (var store = new X509Store(name, location))
            {
                store.Open(OpenFlags.ReadWrite);

                certificate = store.Certificates.Find(findType, findValue, false)
                    .Cast<X509Certificate2>()
                    .FirstOrDefault();
                if (certificate == null)
                {
                    certificate = certificateFactory();
                    store.Add(certificate);
                }
            }

            return certificate;
        }

        /// <summary>
        ///     Adds the specified certificate.
        /// </summary>
        /// <param name="location">One of the enumeration values that specifies the location of the X.509 certificate store.</param>
        /// <param name="certificate">The certificate.</param>
        public static void Add(StoreLocation location, X509Certificate2 certificate)
        {
            Add(StoreName.My, location, certificate);
        }

        /// <summary>
        ///     Adds the specified certificate.
        /// </summary>
        /// <param name="name">One of the enumeration values that specifies the name of the X.509 certificate store.</param>
        /// <param name="location">One of the enumeration values that specifies the location of the X.509 certificate store.</param>
        /// <param name="certificate">The certificate.</param>
        public static void Add(StoreName name, StoreLocation location, X509Certificate2 certificate)
        {
            if (Get(name, location, X509FindType.FindByThumbprint, certificate.Thumbprint) != null)
            {
                return;
            }

            using (var store = new X509Store(name, location))
            {
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
            }
        }

        /// <summary>
        ///     Removes a certificate from the store for the specified find criteria.
        /// </summary>
        /// <param name="location">One of the enumeration values that specifies the location of the X.509 certificate store.</param>
        /// <param name="findType">One of the System.Security.Cryptography.X509Certificates.X509FindType values.</param>
        /// <param name="findValue">The search criteria as an object.</param>
        public static void Delete(StoreLocation location, X509FindType findType, object findValue)
        {
            Delete(StoreName.My, location, findType, findValue);
        }

        /// <summary>
        ///     Removes a certificate from the store for the specified find criteria.
        /// </summary>
        /// <param name="name">One of the enumeration values that specifies the name of the X.509 certificate store.</param>
        /// <param name="location">One of the enumeration values that specifies the location of the X.509 certificate store.</param>
        /// <param name="findType">One of the System.Security.Cryptography.X509Certificates.X509FindType values.</param>
        /// <param name="findValue">The search criteria as an object.</param>
        public static void Delete(StoreName name, StoreLocation location, X509FindType findType, object findValue)
        {
            var certificate = Get(location, findType, findValue);

            if (certificate == null)
            {
                return;
            }

            using (var store = new X509Store(name, location))
            {
                store.Open(OpenFlags.ReadWrite);

                store.Remove(certificate);
            }
        }
    }
}