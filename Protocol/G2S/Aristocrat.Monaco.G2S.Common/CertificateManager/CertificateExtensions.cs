namespace Aristocrat.Monaco.G2S.Common.CertificateManager
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Models;

    /// <summary>
    ///     A set of certificate extension methods
    /// </summary>
    public static class CertificateExtensions
    {
        /// <summary>
        ///     Creates an <see cref="X509Certificate2" /> from a byte array
        /// </summary>
        /// <param name="this">The certificate</param>
        /// <param name="keyStorage">Additional storage key flags </param>
        /// <returns>an <see cref="X509Certificate2" /></returns>
        public static X509Certificate2 ToX509Certificate2(
            this Certificate @this,
            X509KeyStorageFlags keyStorage = X509KeyStorageFlags.DefaultKeySet)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return CertificateUtilities.CertificateFromByteArray(@this.Data, @this.Password, keyStorage);
        }

        /// <summary>
        ///     Determines if the certificate is expired
        /// </summary>
        /// <param name="this">The certificate</param>
        /// <returns>true if the certificate is expired</returns>
        public static bool IsExpired(this X509Certificate2 @this)
        {
            return @this.NotAfter.ToUniversalTime() < DateTime.UtcNow;
        }

        /// <summary>
        ///     Determines if the certificate is valid for use
        /// </summary>
        /// <param name="this">The certificate</param>
        /// <returns>true if the certificate is valid</returns>
        public static bool IsValid(this X509Certificate2 @this)
        {
            var now = DateTime.UtcNow;

            return now >= @this.NotBefore.ToUniversalTime() && now <= @this.NotAfter.ToUniversalTime();
        }
    }
}