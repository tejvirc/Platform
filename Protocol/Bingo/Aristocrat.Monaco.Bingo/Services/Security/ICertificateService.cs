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
        ///     Gets the saved certificates
        /// </summary>
        public IEnumerable<X509Certificate2> GetCertificates();
    }
}
