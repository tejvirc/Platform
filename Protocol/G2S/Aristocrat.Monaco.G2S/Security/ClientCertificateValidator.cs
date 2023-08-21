namespace Aristocrat.Monaco.G2S.Security
{
    using System;
    using System.Collections.Concurrent;
    using CoreWCF.IdentityModel.Selectors;
    using CoreWCF.IdentityModel.Tokens;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using Common.CertificateManager;
    using Common.CertificateManager.Models;
    using log4net;

    public class ClientCertificateValidator : X509CertificateValidator
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConcurrentDictionary<string, GetCertificateStatusResult> _certificates =
            new ConcurrentDictionary<string, GetCertificateStatusResult>();

        private readonly ICertificateService _certificateService;

        public ClientCertificateValidator(ICertificateService certificateService)
        {
            _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));

            CreateChainTrustValidator(true, new X509ChainPolicy());
        }

        public override void Validate(X509Certificate2 certificate)
        {
            // Check that there is a certificate.
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

            if (!certificate.IsValid())
            {
                Logger.Error($"Certificate validity period is invalid: {certificate.Thumbprint}");
                throw new SecurityTokenValidationException("Certificate is not valid");
            }

            ValidateChain(certificate);

            if (certificate.Thumbprint == null || _certificates.TryGetValue(certificate.Thumbprint, out var currentStatus) &&
                !ReAuthenticate(currentStatus))
            {
                return;
            }

            ValidateStatus(certificate, currentStatus);
        }

        private static void ValidateChain(X509Certificate2 certificate)
        {
            var chain = new X509Chain(true)
            {
                ChainPolicy =
                {
                    RevocationMode = X509RevocationMode.Online,
                    VerificationFlags = X509VerificationFlags.IgnoreEndRevocationUnknown
                }
            };

            if (!chain.Build(certificate))
            {
                if (chain.ChainStatus.Any(s => (s.Status & X509ChainStatusFlags.PartialChain) != X509ChainStatusFlags.PartialChain))
                {
                    var status = GetChainStatusInformation(chain.ChainStatus);

                    Logger.Error($"Certificate chain is invalid: {certificate.Thumbprint} - {status}");

                    throw new SecurityTokenValidationException($"Certificate chain is invalid: {status}.");
                }
            }
        }

        private static string GetChainStatusInformation(X509ChainStatus[] chainStatus)
        {
            if (chainStatus == null)
            {
                return string.Empty;
            }

            var error = new StringBuilder(128);
            foreach (var t in chainStatus)
            {
                error.Append(t.StatusInformation);
                error.Append(" ");
            }

            return error.ToString();
        }

        private static bool ReAuthenticate(GetCertificateStatusResult currentStatus)
        {
            if (currentStatus.Status != CertificateStatus.Good)
            {
                return true;
            }

            return currentStatus.NextUpDateTime < DateTime.UtcNow;
        }

        private void ValidateStatus(X509Certificate2 certificate, GetCertificateStatusResult currentStatus)
        {
            if (certificate.Thumbprint == null)
            {
                throw new SecurityTokenValidationException("Certificate is invalid");
            }

            var status = _certificateService.GetCertificateStatus(
                new Certificate
                {
                    Thumbprint = certificate.Thumbprint,
                    Data = certificate.Export(X509ContentType.Cert),
                    VerificationDate = currentStatus?.Verified ?? DateTime.MinValue.Date,
                    Status = currentStatus?.Status ?? CertificateStatus.Revoked,
                    OcspOfflineDate = currentStatus?.Offline
                });


            _certificates.AddOrUpdate(certificate.Thumbprint, status, (k, v) => status);

            if (status.Status != CertificateStatus.Good)
            {
                Logger.Error($"Certificate status of {status.Status} is not permitted: {certificate.Thumbprint}");

                throw new SecurityTokenValidationException("Certificate status is bad or unknown.");
            }
        }
    }
}