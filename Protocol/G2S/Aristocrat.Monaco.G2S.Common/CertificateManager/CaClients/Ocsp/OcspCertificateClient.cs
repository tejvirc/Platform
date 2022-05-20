namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CaClients.Ocsp
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using log4net;
    using Models;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Simple Online Certificate Status Protocol client implementation.
    /// </summary>
    public class OcspCertificateClient
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMonacoContextFactory _contextFactory;
        private readonly IPkiConfigurationRepository _pkiConfiguration;

        private readonly ICertificateRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OcspCertificateClient" /> class.
        ///     Default constructor.
        /// </summary>
        /// <param name="contextFactory">The context factory</param>
        /// <param name="repository">An <see cref="ICertificateFactory" /> instance</param>
        /// <param name="pkiConfiguration">Am <see cref="IPkiConfigurationRepository" /> instance</param>
        public OcspCertificateClient(
            IMonacoContextFactory contextFactory,
            ICertificateRepository repository,
            IPkiConfigurationRepository pkiConfiguration)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _pkiConfiguration = pkiConfiguration ?? throw new ArgumentNullException(nameof(pkiConfiguration));
        }

        /// <summary>
        ///     Gets certificate status by using Online Certificate Status Protocol.
        /// </summary>
        /// <param name="certificateManagerLocation">The address of the certificate manager</param>
        /// <param name="certificateStatusLocation">The address of the certificate status server</param>
        /// <returns>Returns certificate status.</returns>
        public virtual GetOcspCertificateStatusResult GetStatus(
            string certificateManagerLocation,
            string certificateStatusLocation)
        {
            using (var context = _contextFactory.Create())
            {
                var current = _repository.Get(context, c => c.Default).Single();
                var certificate = current.ToX509Certificate2();

                return GetStatus(certificateManagerLocation, certificateStatusLocation, certificate);
            }
        }

        /// <summary>
        ///     Gets certificate status by using Online Certificate Status Protocol.
        /// </summary>
        /// <param name="certificateManagerLocation">The address of the certificate manager</param>
        /// <param name="certificateStatusLocation">The address of the certificate status server</param>
        /// <param name="certificate">The certificate to verify</param>
        /// <returns>Returns certificate status.</returns>
        public virtual GetOcspCertificateStatusResult GetStatus(
            string certificateManagerLocation,
            string certificateStatusLocation,
            X509Certificate2 certificate)
        {
            using (var context = _contextFactory.Create())
            {
                var configuration = _pkiConfiguration.GetSingle(context);

                try
                {
                    var issuerCertificate = GetSigningCertificate(certificateManagerLocation);

                    byte[] nonce = null;

                    if (configuration.NoncesEnabled)
                    {
                        var rng = new RNGCryptoServiceProvider();

                        nonce = new byte[16];
                        rng.GetBytes(nonce);
                    }

                    var request = OcspHelper.GenerateOcspRequest(issuerCertificate, certificate, nonce);

                    var response = OcspHelper.SubmitRequestToOcsp(certificateStatusLocation, request.GetEncoded());

                    if (response == null || response.Length == 0)
                    {
                        Logger.Warn($"Received an empty status response  from {certificateStatusLocation}");

                        return new GetOcspCertificateStatusResult(
                            OcspServiceStatus.Offline,
                            OcspCertificateStatus.Unknown);
                    }

                    var status = OcspHelper.ProcessOcspResponse(issuerCertificate, certificate, response, nonce);

                    Logger.Debug($"Received a status of {status} from {certificateStatusLocation}");

                    return new GetOcspCertificateStatusResult(OcspServiceStatus.Online, status);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to retrieve the certificate status from {certificateStatusLocation}", ex);

                    return new GetOcspCertificateStatusResult(OcspServiceStatus.Offline, OcspCertificateStatus.Unknown);
                }
            }
        }

        private static X509Certificate2 GetSigningCertificate(string certificateManagerLocation)
        {
            var certificates = OcspHelper.GetCertificateAuthorityFromServer(certificateManagerLocation);

            return certificates[0];
        }
    }
}