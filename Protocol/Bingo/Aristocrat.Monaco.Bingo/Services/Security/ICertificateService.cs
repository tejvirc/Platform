namespace Aristocrat.Monaco.Bingo.Services.Security
{
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using Kernel;

    /// <summary>
    ///     Manages installation and removal of certificate files.
    /// </summary>
    public interface ICertificateService : IService
    {
        /// <summary>
        ///     Gets the saved certificate file.
        /// </summary>
        /// <param name="name">Name of the x509 certificate store.</param>
        /// <param name="location">Location of the x509 certificate store.</param>
        /// <param name="findType">Type of the x509 to be retrieved.</param>
        public IEnumerable<X509Certificate2> Get(StoreName name, StoreLocation location, X509FindType findType);
    }
}
