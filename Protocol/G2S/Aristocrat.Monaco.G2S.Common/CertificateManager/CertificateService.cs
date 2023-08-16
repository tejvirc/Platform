namespace Aristocrat.Monaco.G2S.Common.CertificateManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using CaClients.Ocsp;
    using CaClients.Scep;
    using CommandHandlers;
    using Events;
    using FluentValidation;
    using Kernel;
    using log4net;
    using Models;
    using Monaco.Common.Models;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Certificate service implementation
    /// </summary>
    public class CertificateService : ICertificateService
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IValidator<PkiConfiguration> _certificateConfigurationEntityValidator;
        private readonly IValidator<Certificate> _certificateEntityValidator;
        private readonly ICertificateRepository _certificateRepository;
        private readonly IPkiConfigurationRepository _configurationRepository;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly OcspCertificateClient _ocspClient;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateService" /> class.
        /// </summary>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="configurationRepository">Certificate configuration repository.</param>
        /// <param name="certificateRepository">Certificate repository.</param>
        /// <param name="certificateEntityValidator">Certificate entity validator.</param>
        /// <param name="certificateConfigurationEntityValidator">Certificate configuration entity validator.</param>
        public CertificateService(
            IMonacoContextFactory contextFactory,
            IPkiConfigurationRepository configurationRepository,
            ICertificateRepository certificateRepository,
            IValidator<Certificate> certificateEntityValidator,
            IValidator<PkiConfiguration> certificateConfigurationEntityValidator)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _configurationRepository =
                configurationRepository ?? throw new ArgumentNullException(nameof(configurationRepository));
            _certificateRepository =
                certificateRepository ?? throw new ArgumentNullException(nameof(certificateRepository));
            _certificateEntityValidator = certificateEntityValidator ??
                                          throw new ArgumentNullException(nameof(certificateEntityValidator));
            _certificateConfigurationEntityValidator = certificateConfigurationEntityValidator ??
                                                       throw new ArgumentNullException(
                                                           nameof(certificateConfigurationEntityValidator));

            _ocspClient = new OcspCertificateClient(_contextFactory, _certificateRepository, _configurationRepository);
        }

        /// <inheritdoc />
        public string GetCaCertificateThumbprint(string certificateManagerLocation)
        {
            try
            {
                var client = new ScepClient();

                var thumbprint = client.GetCaCertificateThumbprint(certificateManagerLocation);

                Logger.Debug($"Retrieved CA certificate thumbprint - {thumbprint}");

                return thumbprint;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to get CA thumbprint", ex);
                return string.Empty;
            }
        }

        /// <inheritdoc />
        public CertificateActionResult Enroll(string secret)
        {
            Logger.Debug("Initiating certificate enrollment");

            var configuration = GetConfiguration() ?? new PkiConfiguration();

            var client = new ScepClient();

            var chain = client.GetCaCertificateChain(configuration);
            if (chain == null)
            {
                Logger.Error("Failed to enroll certificate. Failed to get certificate chain");
                return new CertificateActionResult(CertificateRequestStatus.Error);
            }

            foreach (var cert in chain)
            {
                Logger.Info($"Installing certificate from CA certificate chain with thumbprint {cert.Thumbprint}");

                InstallCertificate(cert);
            }

            var signingCertificate = CertificateUtilities.CreateSelfSignedCertificate(
                configuration.CommonName,
                configuration.KeySize);

            Logger.Debug($"Signing certificate thumbprint - {signingCertificate.Thumbprint}");

            try
            {
                return client.Enroll(signingCertificate, configuration, secret);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to enroll certificate", ex);
                return new CertificateActionResult(CertificateRequestStatus.Error);
            }
        }

        /// <inheritdoc />
        public CertificateActionResult Poll(byte[] requestData, X509Certificate2 signingCertificate)
        {
            if (requestData == null)
            {
                throw new ArgumentNullException(nameof(requestData));
            }

            Logger.Debug("Checking enrollment status");

            try
            {
                var configuration = GetConfiguration();

                var client = new ScepClient();

                return client.CheckStatus(requestData, signingCertificate, configuration);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to check enrollment status", ex);
                return new CertificateActionResult(CertificateRequestStatus.Error);
            }
        }

        /// <inheritdoc />
        public DateTime NextRenewal()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var current = _certificateRepository.Get(context, c => c.Default).FirstOrDefault();
                if (current == null)
                {
                    return DateTime.MaxValue;
                }

                if (current.Status == CertificateStatus.Expired)
                {
                    return DateTime.UtcNow;
                }

                var cert = current.ToX509Certificate2();

                var validity = cert.NotAfter - cert.NotBefore;

                return cert.NotBefore.ToUniversalTime().AddMilliseconds(validity.TotalMilliseconds / 2);
            }
        }

        /// <inheritdoc />
        public CertificateActionResult Renew()
        {
            Logger.Debug("Initiating certificate renewal");

            using (var context = _contextFactory.CreateDbContext())
            {
                var current = _certificateRepository.Get(context, c => c.Default).FirstOrDefault();
                if (current == null)
                {
                    Logger.Error("Failed to renew certificate. A certificate must be issued first");

                    return new CertificateActionResult(CertificateRequestStatus.Error);
                }

                try
                {
                    var issued = current.ToX509Certificate2(X509KeyStorageFlags.Exportable);

                    var client = new ScepClient();

                    var configuration = GetConfiguration();

                    return client.Renew(issued, configuration);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to renew certificate", ex);
                    return new CertificateActionResult(CertificateRequestStatus.Error);
                }
            }
        }

        /// <inheritdoc />
        public DateTime NextExchange()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                // A non-default cert with a private key is waiting to be activated
                var pending = _certificateRepository.Get(context, c => !c.Default).ToList()
                    .FirstOrDefault(c => c.ToX509Certificate2().HasPrivateKey);

                return pending?.ToX509Certificate2().NotBefore.ToUniversalTime() ?? DateTime.MaxValue;
            }
        }

        /// <inheritdoc />
        public void Exchange(X509Certificate2 certificate)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var current = _certificateRepository.Get(context, c => c.Default).FirstOrDefault();

                if (certificate == null)
                {
                    var pending = _certificateRepository.Get(context, c => !c.Default).ToList()
                        .FirstOrDefault(c => c.ToX509Certificate2().HasPrivateKey);
                    if (pending == null)
                    {
                        return;
                    }

                    certificate = pending.ToX509Certificate2();
                }

                InstallCertificate(certificate, true);

                if (current != null)
                {
                    _certificateRepository.Delete(context, current);

                    CertificateStore.Delete(StoreLocation.LocalMachine, X509FindType.FindByThumbprint, current.Thumbprint);
                }

                Logger.Info(
                    $"Exchanged certificate {current?.ToX509Certificate2().Thumbprint ?? "'None'"} with {certificate?.Thumbprint}");

                ServiceManager.GetInstance().GetService<IEventBus>().Publish(new CertificateRenewedEvent());
            }
        }

        /// <inheritdoc />
        public Certificate InstallCertificate(X509Certificate2 certificate, bool isDefault = false)
        {
            Logger.Info($"Installing certificate with thumbprint {certificate.Thumbprint}");

            try
            {
                var addHandler = new AddCertificateHandler(
                    _contextFactory,
                    _certificateRepository,
                    _certificateEntityValidator);

                var result = addHandler.Execute(certificate);
                if (!isDefault)
                {
                    return result;
                }

                var setDefault = new SetDefaultCertificateHandler(_contextFactory, _certificateRepository);

                return setDefault.Execute(certificate);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to install certificate", ex);
                throw;
            }
        }

        /// <inheritdoc />
        public bool RemoveCertificate(X509Certificate2 certificate)
        {
            Logger.Debug("Removing certificate");

            try
            {
                var handler = new DeleteCertificateHandler(_contextFactory, _certificateRepository);

                return handler.Execute(certificate);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to remove certificate", ex);
                throw;
            }
        }

        /// <inheritdoc />
        public GetCertificateStatusResult GetCertificateStatus()
        {
            Logger.Debug("Retrieving certificate status");

            using (var context = _contextFactory.CreateDbContext())
            {
                var current = _certificateRepository.GetDefault(context);

                var handler = new GetCertificateStatusHandler(
                    _contextFactory,
                    _certificateRepository,
                    _configurationRepository,
                    _ocspClient);

                var result = handler.Execute();

                if (current.Status != result.Status)
                {
                    Logger.Info($"Certificate status changed from {current.Status} to {result.Status}");

                    if (result.Status == CertificateStatus.Good)
                    {
                        ServiceManager.GetInstance().GetService<IEventBus>().Publish(new CertificateValidatedEvent());
                    }
                    else
                    {
                        ServiceManager.GetInstance().GetService<IEventBus>().Publish(new CertificateInvalidatedEvent());
                    }
                }

                return result;
            }
        }

        /// <inheritdoc />
        public GetCertificateStatusResult GetCertificateStatus(Certificate certificate)
        {
            Logger.Debug($"Retrieving certificate status for: {certificate.Thumbprint}");

            var handler = new GetCertificateStatusHandler(
                _contextFactory,
                _certificateRepository,
                _configurationRepository,
                _ocspClient,
                certificate);

            return handler.Execute();
        }

        /// <inheritdoc />
        public OcspQueryResult TestOcsp()
        {
            Logger.Debug("Performing OCSP test.");

            var handler = new OcspQueryHandler(
                _contextFactory,
                _certificateRepository,
                _configurationRepository,
                _ocspClient);

            return handler.Execute();
        }

        /// <inheritdoc />
        public IList<Certificate> GetCertificates()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return _certificateRepository.GetAll(context).ToList();
            }
        }

        /// <inheritdoc />
        public bool IsEnrolled()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var current = _certificateRepository.Get(context, c => c.Default).SingleOrDefault();

                return current?.Status == CertificateStatus.Good;
            }
        }

        /// <inheritdoc />
        public bool IsCertificateManagementEnabled()
        {
            var config = GetConfiguration();
            return config != null && config.ScepEnabled;
        }

        /// <inheritdoc />
        public bool HasValidCertificate()
        {
            var config = GetConfiguration();

            using (var context = _contextFactory.CreateDbContext())
            {
                var current = _certificateRepository.Get(context, c => c.Default).SingleOrDefault();

                if (current == null)
                {
                    return false;
                }

                if (!config.OcspEnabled)
                {
                    return !current.ToX509Certificate2().IsExpired();
                }

                return current.Status == CertificateStatus.Good && !current.ToX509Certificate2().IsExpired();
            }
        }

        /// <inheritdoc />
        public PkiConfiguration GetConfiguration()
        {
            var handler = new GetPkiConfigurationHandler(_contextFactory, _configurationRepository);

            return handler.Execute();
        }

        /// <inheritdoc />
        public SaveEntityResult SaveConfiguration(PkiConfiguration configuration)
        {
            Logger.Debug("Saving certificate configuration");

            var handler = new SavePkiConfigurationHandler(
                _contextFactory,
                _configurationRepository,
                _certificateConfigurationEntityValidator);

            return handler.Execute(configuration);
        }
    }
}