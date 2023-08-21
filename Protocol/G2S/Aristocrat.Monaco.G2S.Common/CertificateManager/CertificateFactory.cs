namespace Aristocrat.Monaco.G2S.Common.CertificateManager
{
    using System;
    using System.Collections.Generic;
    using Kernel;
    using Monaco.Common.Storage;
    using Storage;
    using Validators;

    /// <summary>
    ///     Implementation of <c>ICertificateFactory</c>.
    /// </summary>
    public class CertificateFactory : ICertificateFactory, IService
    {
        /// <summary>
        ///     Gets instance of <c>ICertificateService</c>.
        /// </summary>
        /// <returns>Returns instance of <c>ICertificateService</c>.</returns>
        public ICertificateService GetCertificateService()
        {
            return new CertificateService(
                ServiceManager.GetInstance().GetService<IMonacoContextFactory>(),
                GetCertificateConfigurationRepository(),
                GetCertificateRepository(),
                new CertificateValidator(),
                new PkiConfigurationValidator());
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ICertificateFactory) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        private static IPkiConfigurationRepository GetCertificateConfigurationRepository()
        {
            return new PkiConfigurationRepository();
        }

        private static ICertificateRepository GetCertificateRepository()
        {
            return new CertificateRepository();
        }
    }
}