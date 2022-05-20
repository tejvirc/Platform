namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CommandHandlers
{
    using System;
    using System.Linq;
    using System.Reflection;
    using CaClients.Ocsp;
    using log4net;
    using Models;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Org.BouncyCastle.Security.Certificates;
    using Storage;

    /// <summary>
    ///     Get certificate status handler
    /// </summary>
    public class GetCertificateStatusHandler : IFuncHandler<GetCertificateStatusResult>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPkiConfigurationRepository _certificateConfigurationRepository;
        private readonly ICertificateRepository _certificateRepository;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly OcspCertificateClient _ocspCertificateService;
        private readonly Certificate _certificate;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCertificateStatusHandler" /> class.
        /// </summary>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="certificateRepository">The certificate repository.</param>
        /// <param name="certificateConfigurationRepository">The certificate configuration repository.</param>
        /// <param name="ocspCertificateService">The ocsp certificate service.</param>
        /// <param name="certificate">The non-default certificate to validate</param>
        public GetCertificateStatusHandler(
            IMonacoContextFactory contextFactory,
            ICertificateRepository certificateRepository,
            IPkiConfigurationRepository certificateConfigurationRepository,
            OcspCertificateClient ocspCertificateService,
            Certificate certificate = null)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _certificateRepository =
                certificateRepository ?? throw new ArgumentNullException(nameof(certificateRepository));
            _certificateConfigurationRepository = certificateConfigurationRepository ??
                                                  throw new ArgumentNullException(
                                                      nameof(certificateConfigurationRepository));
            _ocspCertificateService =
                ocspCertificateService ?? throw new ArgumentNullException(nameof(ocspCertificateService));
            _certificate = certificate;
        }

        /// <summary>
        ///     Executes this instance.
        /// </summary>
        /// <returns>An GetCertificateStatusResult</returns>
        public GetCertificateStatusResult Execute()
        {
            if (_certificate != null)
            {
                return IsExpired(_certificate)
                    ? new GetCertificateStatusResult(CertificateStatus.Expired, DateTime.MinValue, null, null)
                    : GetCertificateStatus(_certificate);
            }

            using (var context = _contextFactory.Create())
            {
                var currentCertificate = _certificateRepository.Get(context, c => c.Default).SingleOrDefault();
                if (currentCertificate == null)
                {
                    throw new CertificateException("No default certificate");
                }

                return IsExpired(currentCertificate)
                    ? new GetCertificateStatusResult(CertificateStatus.Expired, DateTime.MinValue, null, null)
                    : UpdateStatus(currentCertificate);
            }
        }

        private static DateTime GetNextUpdate(int configurationParameter)
        {
            var reAuthPeriod = GetRandomNumberFromRange(configurationParameter / 2, configurationParameter);

            return DateTime.UtcNow.AddMinutes(
                reAuthPeriod < PkiConstants.DefaultMinimumPeriod
                    ? GetRandomNumberFromRange(PkiConstants.DefaultMinimumPeriod, PkiConstants.DefaultMaximumPeriod)
                    : reAuthPeriod);
        }

        private static DateTime GetMinimumRandomPeriod()
        {
            return DateTime.UtcNow.AddMinutes(
                GetRandomNumberFromRange(PkiConstants.DefaultMinimumPeriod, PkiConstants.DefaultMaximumPeriod));
        }

        private static int GetRandomNumberFromRange(int startPoint, int endPoint)
        {
            var random = new Random(Guid.NewGuid().GetHashCode());

            return random.Next(startPoint, endPoint + 1);
        }

        private static CertificateStatus ToCertificateStatus(OcspCertificateStatus ocspCertificateStatus)
        {
            var status = CertificateStatus.Good;

            if (ocspCertificateStatus == OcspCertificateStatus.Revoked)
            {
                status = CertificateStatus.Revoked;
            }

            return status;
        }

        private static bool IsExpired(Certificate certificate)
        {
            return certificate.ToX509Certificate2().IsExpired();
        }

        private GetCertificateStatusResult UpdateStatus(Certificate certificate)
        {
            using (var context = _contextFactory.Create())
            {
                var result = GetCertificateStatus(certificate);

                _certificateRepository.Update(context, certificate);

                return result;
            }
        }

        private GetCertificateStatusResult GetCertificateStatus(Certificate certificate)
        {
            using (var context = _contextFactory.Create())
            {
                var configuration = _certificateConfigurationRepository.GetSingle(context);

                if (!configuration.OcspEnabled)
                {
                    return new GetCertificateStatusResult(CertificateStatus.Good, DateTime.UtcNow, null, null);
                }

                var timePassed = DateTime.UtcNow - certificate.VerificationDate;

                if (timePassed.TotalMinutes < (short)(configuration.OcspReAuthenticationPeriod / 2))
                {
                    return new GetCertificateStatusResult(
                        CertificateStatus.Good,
                        GetNextUpdate(configuration.OcspReAuthenticationPeriod),
                        certificate.VerificationDate,
                        certificate.OcspOfflineDate);
                }

                DateTime? nextUpdate = null;

                var status = _ocspCertificateService.GetStatus(
                    configuration.CertificateManagerLocation,
                    configuration.CertificateStatusLocation,
                    certificate.ToX509Certificate2());

                if (status.OcspServiceStatus == OcspServiceStatus.Offline)
                {
                    if (!certificate.OcspOfflineDate.HasValue)
                    {
                        certificate.OcspOfflineDate = DateTime.UtcNow;
                    }

                    var offlineDuration = DateTime.UtcNow - (certificate.OcspOfflineDate ?? DateTime.UtcNow);

                    Logger.Warn($"OCSP service has been offline for {offlineDuration}");

                    nextUpdate = GetMinimumRandomPeriod();

                    if (configuration.OcspMinimumPeriodForOffline > 0 &&
                        offlineDuration.TotalMinutes < configuration.OcspMinimumPeriodForOffline)
                    {
                        certificate.Status = CertificateStatus.Good;

                        Logger.Info(
                            $"OCSP service is offline, but duration is less than gsaOO ({configuration.OcspMinimumPeriodForOffline})");
                    }
                    else
                    {
                        switch (configuration.OfflineMethod)
                        {
                            case OfflineMethodType.OptionB:
                                certificate.Status = timePassed.TotalMinutes <
                                                     configuration.OcspAcceptPreviouslyGoodCertificatePeriod
                                    ? CertificateStatus.Good
                                    : CertificateStatus.Revoked;
                                break;
                            case OfflineMethodType.OptionC:
                                // This isn't supported yet, but we need to do something
                                certificate.Status = CertificateStatus.Revoked;
                                break;
                            default:
                                certificate.Status = CertificateStatus.Revoked;
                                break;
                        }

                        Logger.Info(
                            $"OCSP service is offline. Reporting a status of {certificate.Status} using {configuration.OfflineMethod} - last verification {certificate.VerificationDate}");
                    }
                }
                else
                {
                    if (status.OcspCertificateStatus == OcspCertificateStatus.Good)
                    {
                        certificate.VerificationDate = DateTime.UtcNow;

                        nextUpdate = GetNextUpdate(configuration.OcspReAuthenticationPeriod);
                    }

                    certificate.OcspOfflineDate = null;
                    certificate.Status = ToCertificateStatus(status.OcspCertificateStatus);
                }

                return new GetCertificateStatusResult(certificate.Status, certificate.VerificationDate, certificate.OcspOfflineDate, nextUpdate);
            }
        }
    }
}