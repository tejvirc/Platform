namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CommandHandlers
{
    using System;
    using System.Linq;
    using System.Net;
    using CaClients.Ocsp;
    using Models;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     OCSP Query handler for the UI test
    /// </summary>
    public class OcspQueryHandler : IFuncHandler<OcspQueryResult>
    {
        private readonly IPkiConfigurationRepository _certificateConfigurationRepository;
        private readonly ICertificateRepository _certificateRepository;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly OcspCertificateClient _ocspCertificateService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OcspQueryHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="certificateRepository">The certificate repository.</param>
        /// <param name="certificateConfigurationRepository">The certificate configuration repository.</param>
        /// <param name="ocspCertificateService">The ocsp certificate service.</param>
        public OcspQueryHandler(
            IMonacoContextFactory contextFactory,
            ICertificateRepository certificateRepository,
            IPkiConfigurationRepository certificateConfigurationRepository,
            OcspCertificateClient ocspCertificateService)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _certificateRepository =
                certificateRepository ?? throw new ArgumentNullException(nameof(certificateRepository));
            _certificateConfigurationRepository = certificateConfigurationRepository ??
                                                  throw new ArgumentNullException(
                                                      nameof(certificateConfigurationRepository));
            _ocspCertificateService =
                ocspCertificateService ?? throw new ArgumentNullException(nameof(ocspCertificateService));
        }

        /// <summary>
        ///     Executes this instance.
        /// </summary>
        /// <returns>An GetCertificateStatusResult</returns>
        public OcspQueryResult Execute()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var currentCertificate = _certificateRepository.Get(context, c => c.Default).SingleOrDefault();
                if (currentCertificate == null)
                {
                    return new OcspQueryResult(false, "No certificate");
                }

                return QueryOcsp();
            }
        }

        private OcspQueryResult QueryOcsp()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var configuration = _certificateConfigurationRepository.GetSingle(context);

                GetOcspCertificateStatusResult ocspCertificateStatusResult;

                try
                {
                    ocspCertificateStatusResult =
                        _ocspCertificateService.GetStatus(
                            configuration.CertificateManagerLocation,
                            configuration.CertificateStatusLocation);
                }
                catch (WebException)
                {
                    // 404, etc.
                    return new OcspQueryResult(false, "HTTP error");
                }

                if (ocspCertificateStatusResult.OcspServiceStatus == OcspServiceStatus.Online)
                {
                    return new OcspQueryResult(true, "Server Online");
                }

                return new OcspQueryResult(false, "Server Offline");
            }
        }
    }
}