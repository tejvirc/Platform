namespace Aristocrat.Monaco.G2S.CompositionRoot
{
    using System;
    using Common.CertificateManager;
    using Common.CertificateManager.Models;
    using Common.CertificateManager.Storage;
    using Common.CertificateManager.Validators;
    using FluentValidation;
    using Security;
    using SimpleInjector;

    /// <summary>
    ///     Handles configuring the Certificate Manager.
    /// </summary>
    internal static class CertificateManagerBuilder
    {
        /// <summary>
        ///     Registers the package manager with the container.
        /// </summary>
        /// <param name="this">The container.</param>
        /// <param name="connectionString">The connection string.</param>
        internal static void RegisterCertificateManager(this Container @this, string connectionString)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            @this.Register<IPkiConfigurationRepository, PkiConfigurationRepository>(Lifestyle.Singleton);
            @this.Register<ICertificateRepository, CertificateRepository>(Lifestyle.Singleton);

            @this.RegisterSingleton<IValidator<Certificate>, CertificateValidator>();
            @this.RegisterSingleton<IValidator<PkiConfiguration>, PkiConfigurationValidator>();

            @this.Register<ICertificateService, CertificateService>(Lifestyle.Singleton);
            @this.Register<ICertificateMonitor, CertificateMonitor>(Lifestyle.Singleton);
        }
    }
}