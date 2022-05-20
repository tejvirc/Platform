// ReSharper disable once CheckNamespace
namespace Aristocrat.Monaco.Mgam.Common
{
    using System;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using Data.Models;

    /// <summary>
    ///     Certificate method extensions
    /// </summary>
    public static class CertificateExtensions
    {
        /// <summary>
        ///     Create a certificate from embedded resource.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="password">Password (optional).</param>
        /// <returns><see cref="Certificate"/></returns>
        public static Certificate LoadCertificate(string path, string password = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Certificate file not found.", path);
            }

            var cert = new X509Certificate2(path, password);

            return new Certificate { Thumbprint = cert.Thumbprint, RawData = cert.RawData };
        }
    }
}
