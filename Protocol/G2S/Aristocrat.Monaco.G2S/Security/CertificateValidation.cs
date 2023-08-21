namespace Aristocrat.Monaco.G2S.Security
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using Common.CertificateManager;
    using log4net;

    public class CertificateValidation
    {
        private const string SubjectAlternativeNameOid = @"2.5.29.7";
        private const string SubjectAlternativeNameOid2 = @"2.5.29.17";

        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICertificateService _certificateService;

        public CertificateValidation(ICertificateService certificateService)
        {
            _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
        }

        public bool OnValidateCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            if (cert == null)
            {
                return false;
            }

            var clientCert = new X509Certificate2(cert);

            if (!ValidateValidityPeriod(clientCert))
            {
                Logger.Error(
                    $"CertificateValidation.OnValidateCertificate : The certificate ({clientCert.SerialNumber} is expired");

                return false;
            }

            if (chain.ChainStatus.Length > 0)
            {
                Logger.Warn($"Chain status is reporting: {string.Join(", ", chain.ChainStatus.Select(s => $"{s.StatusInformation} ({s.Status})"))}");

                if (chain.ChainStatus.Any(
                    s => (s.Status & X509ChainStatusFlags.UntrustedRoot) == X509ChainStatusFlags.UntrustedRoot))
                {
                    Logger.Error(
                        $"CertificateValidation.OnValidateCertificate : The certificate ({clientCert.SerialNumber} has an untrusted root");
                    return false;
                }
            }

            var configuration = _certificateService.GetConfiguration();
            if (configuration.ValidateDomain && !ValidateDomain(clientCert, sender as HttpWebRequest))
            {
                Logger.Error(
                    $"CertificateValidation.OnValidateCertificate : There is a domain name mismatch {clientCert.GetNameInfo(X509NameType.DnsName, false)}");

                return false;
            }

            Logger.Info(
                $"CertificateValidation.OnValidateCertificate : Client certificate validation accepted with - {error}");

            return true;
        }

        private static bool ValidateValidityPeriod(X509Certificate2 cert)
        {
            return cert.IsValid();
        }

        private static bool ValidateDomain(X509Certificate2 cert, HttpWebRequest request)
        {
            if (request == null)
            {
                return false;
            }

            try
            {
                IPAddress[] addresses = null;

                Logger.Debug($"ValidateDomain : RequestUri - {request.RequestUri.DnsSafeHost}");

                if (request.RequestUri.HostNameType == UriHostNameType.Dns)
                {
                    addresses = Dns.GetHostAddresses(request.RequestUri.DnsSafeHost);

                    Logger.Debug($"ValidateDomain : Resolved addresses - {string.Join(",", addresses.ToList())}");
                }

                foreach (var extension in cert.Extensions)
                {
                    if (extension.Oid.Value != SubjectAlternativeNameOid &&
                        extension.Oid.Value != SubjectAlternativeNameOid2)
                    {
                        continue;
                    }

                    var data = new AsnEncodedData(extension.Oid, extension.RawData);
                    var asnString = data.Format(false);

                    if (string.IsNullOrEmpty(asnString))
                    {
                        continue;
                    }

                    Logger.Debug($"ValidateDomain : Subject Alternative Name - {asnString} from {extension.Oid.Value}");

                    var rawEntries = asnString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var entry in rawEntries)
                    {
                        var keyVal = entry.Split('=');
                        var value = keyVal[1];

                        Logger.Debug($"ValidateDomain : Evaluating - {keyVal} : {value}");

                        if (value.Equals(request.RequestUri.Host, StringComparison.InvariantCultureIgnoreCase))
                        {
                            Logger.Debug($"ValidateDomain : {request.RequestUri.Host} matched {value}");

                            return true;
                        }

                        if (IPAddress.TryParse(value, out var ipAddress))
                        {
                            Logger.Debug($"ValidateDomain : Checking {ipAddress} ");

                            if (addresses != null && addresses.Any(address => ipAddress.Equals(address)))
                            {
                                Logger.Debug($"ValidateDomain : Addresses contains {ipAddress}");

                                return true;
                            }
                        }
                        else
                        {
                            var resolvedAddresses = Dns.GetHostAddresses(value);
                            foreach (var resolved in resolvedAddresses)
                            {
                                Logger.Debug($"ValidateDomain : Checking resolved ip {resolved} for {value}");

                                if (addresses != null && addresses.Any(address => resolved.Equals(address)))
                                {
                                    Logger.Debug($"ValidateDomain : Resolved addresses contains resolved IP {resolved}");

                                    return true;
                                }

                                if (request.RequestUri.Host.Equals(resolved.ToString(), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    Logger.Debug($"ValidateDomain : Request Uri matches resolved IP {resolved}");

                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }
}
