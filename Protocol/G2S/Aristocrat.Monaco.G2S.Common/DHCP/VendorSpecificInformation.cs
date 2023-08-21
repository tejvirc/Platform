namespace Aristocrat.Monaco.G2S.Common.DHCP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Information provided by DHCP server. GSA vendor-specific information provides network location, protocol to be
    ///     used,
    ///     and configuration parameters of a G2S and/or S2S communications configuration server, and servers involved in
    ///     certificate
    ///     management.It is expected that the location and required parameters of other GSA servers (device owners)
    ///     will be provided by a communications configuration server via methods defined in the G2S and/or S2S protocol.
    /// </summary>
    public class VendorSpecificInformation
    {
        private static readonly Dictionary<string, string> TransportCombinations = new Dictionary
            <string, string>
            {
                { "tsu", Uri.UriSchemeHttp },
                { "tou", Uri.UriSchemeHttp },
                { "shs", Uri.UriSchemeHttps },
                { "shu", Uri.UriSchemeHttp },
                { "SCEP:HTTP", Uri.UriSchemeHttp },
                { "OCSP:HTTP", Uri.UriSchemeHttp }
            };

        /// <summary>
        ///     Gets a vendor specific information communication config service definitions.
        /// </summary>
        public virtual ICollection<ServiceDefinition> CommConfigDefinitions { get; private set; }

        /// <summary>
        ///     Gets a vendor specific information certificate manager service definitions.
        /// </summary>
        public virtual ICollection<ServiceDefinition> CertificateManagerDefinitions { get; private set; }

        /// <summary>
        ///     Gets a vendor specific information certificate status service definitions.
        /// </summary>
        public virtual ICollection<ServiceDefinition> CertificateStatusDefinitions { get; private set; }

        /// <summary>
        ///     Gets a vendor specific information custom service definitions.
        /// </summary>
        public virtual ICollection<ServiceDefinition> CustomServiceDefinitions { get; private set; }

        /// <summary>
        ///     Gets the maximum period that an entity must
        ///     attempt to authenticate a certificate with an
        ///     OCSP server before resorting to offline
        ///     behavior.
        /// </summary>
        public virtual short OcspMinimumPeriodForOfflineMin { get; private set; }

        /// <summary>
        ///     Gets the maximum period that an entity can use a
        ///     certificate without attempting to re-authenticate
        ///     the certificate.
        /// </summary>
        public virtual short OcspReauthPeriodMin { get; private set; }

        /// <summary>
        ///     Gets the maximum period that an entity can use a
        ///     previously good certificate when OCSP servers
        ///     are non-responsive.
        /// </summary>
        public virtual short OcspAcceptPrevGoodPeriodMin { get; private set; }

        /// <summary>
        ///     Gets a vendor specific information custom parameters.
        /// </summary>
        public ICollection<Parameter> CustomParameters { get; private set; }

        private Dictionary<string, bool> ChangedDefaults { get; set; }

        /// <summary>
        ///     Creates an updated vendor specific information by string value provided.
        /// </summary>
        /// <param name="value">DHCP code 43 value.</param>
        /// <returns>Vendor specific information.</returns>
        public static VendorSpecificInformation Create(string value)
        {
            var valueToParse = Uri.UnescapeDataString(value);

            if (valueToParse.Length > 255)
            {
                throw new ArgumentException("Value must be less than 256 characters length");
            }

            var vendorSpecificInformation = Create();

            Parse(vendorSpecificInformation, valueToParse);

            return vendorSpecificInformation;
        }

        private static VendorSpecificInformation Create()
        {
            var vendorSpecificInformation = new VendorSpecificInformation();

            SetDefaults(vendorSpecificInformation);

            return vendorSpecificInformation;
        }

        private static void SetDefaults(VendorSpecificInformation vendorSpecificInformation)
        {
            vendorSpecificInformation.OcspMinimumPeriodForOfflineMin = 240;
            vendorSpecificInformation.OcspReauthPeriodMin = 600;
            vendorSpecificInformation.OcspAcceptPrevGoodPeriodMin = 720;

            vendorSpecificInformation.CommConfigDefinitions = new List<ServiceDefinition>();
            vendorSpecificInformation.CertificateManagerDefinitions = new List<ServiceDefinition>();
            vendorSpecificInformation.CertificateStatusDefinitions = new List<ServiceDefinition>();
            vendorSpecificInformation.CustomServiceDefinitions = new List<ServiceDefinition>();

            vendorSpecificInformation.CommConfigDefinitions.Add(
                new ServiceDefinition(
                    DhcpConstants.CommConfigServiceName,
                    new Uri($"{Uri.UriSchemeHttps}://g2sCC.g2s.com:443"),
                    1,
                    new Dictionary<string, string>()));

            vendorSpecificInformation.CertificateManagerDefinitions.Add(
                new ServiceDefinition(
                    DhcpConstants.CertificateManagerServiceName,
                    new Uri($"{Uri.UriSchemeHttp}://gsaCM.gsa.com:80"),
                    0,
                    new Dictionary<string, string> { { DhcpConstants.CaIdent, "gsaCA" } }));

            vendorSpecificInformation.CertificateStatusDefinitions.Add(
                new ServiceDefinition(
                    DhcpConstants.CertificateStatusServiceName,
                    new Uri($"{Uri.UriSchemeHttp}://gsaCS.gsa.com:80"),
                    0,
                    new Dictionary<string, string>()));

            vendorSpecificInformation.CustomParameters = new List<Parameter>();

            vendorSpecificInformation.ChangedDefaults = new Dictionary<string, bool>
            {
                { DhcpConstants.CertificateManagerServiceName, false },
                { DhcpConstants.CertificateStatusServiceName, false },
                { DhcpConstants.CommConfigServiceName, false }
            };
        }

        private static ServiceDefinition GetServiceDefinition(string name, string value)
        {
            var hostId = 0;
            var serviceParameters = new Dictionary<string, string>();
            var protocol = Uri.UriSchemeHttp;

            var hostDelimiter = value.LastIndexOf(@"+", StringComparison.InvariantCultureIgnoreCase);
            if (hostDelimiter != -1)
            {
                var firstParam = value.IndexOf(@"^", StringComparison.InvariantCultureIgnoreCase);
                if (firstParam == -1)
                {
                    int.TryParse(value.Substring(hostDelimiter + 1), out hostId);

                    value = value.Remove(hostDelimiter);
                }
                else
                {
                    var length = firstParam - hostDelimiter;

                    int.TryParse(value.Substring(hostDelimiter + 1, length - 1), out hostId);

                    value = value.Remove(hostDelimiter, length);
                }
            }

            var paramDelimiter = value.LastIndexOf(@"^", StringComparison.InvariantCultureIgnoreCase);
            while (paramDelimiter != -1)
            {
                var parameters = value.Substring(paramDelimiter + 1).Split('=');
                if (parameters.Length == 2)
                {
                    serviceParameters.Add(parameters[0], parameters[1]);
                }

                value = value.Remove(paramDelimiter);

                paramDelimiter = value.LastIndexOf(@"^", StringComparison.InvariantCultureIgnoreCase);
            }

            var transport = TransportCombinations.FirstOrDefault(
                t => value.StartsWith(t.Key, StringComparison.CurrentCultureIgnoreCase));
            if (!string.IsNullOrEmpty(transport.Key))
            {
                protocol = transport.Value;

                value = value.Remove(0, transport.Key.Length + 1);
            }

            return new ServiceDefinition(
                name,
                new Uri($"{protocol}://{value}"),
                hostId,
                serviceParameters);
        }

        private static void Parse(VendorSpecificInformation vendorSpecificInformation, string value)
        {
            var parameters = value.Split('|');

            foreach (var parameter in parameters)
            {
                var keyValue = parameter.Split(new[] { '=' }, 2);
                if (keyValue.Length != 2)
                {
                    continue;
                }

                switch (keyValue[0])
                {
                    case DhcpConstants.CommConfigServiceName:
                        UpdateServiceDefinitions(
                            vendorSpecificInformation.ChangedDefaults,
                            vendorSpecificInformation.CommConfigDefinitions,
                            GetServiceDefinition(keyValue[0], keyValue[1]));
                        break;
                    case DhcpConstants.CertificateManagerServiceName:
                        UpdateServiceDefinitions(
                            vendorSpecificInformation.ChangedDefaults,
                            vendorSpecificInformation.CertificateManagerDefinitions,
                            GetServiceDefinition(keyValue[0], keyValue[1]));
                        break;
                    case DhcpConstants.CertificateStatusServiceName:
                        UpdateServiceDefinitions(
                            vendorSpecificInformation.ChangedDefaults,
                            vendorSpecificInformation.CertificateStatusDefinitions,
                            GetServiceDefinition(keyValue[0], keyValue[1]));
                        break;
                    case DhcpConstants.OcspMinimumPeriodForOfflineMinParameterName:
                        vendorSpecificInformation.OcspMinimumPeriodForOfflineMin =
                            short.TryParse(keyValue[1], out var oo) ? oo : default(short);
                        break;
                    case DhcpConstants.OcspReauthPeriodMinParameterName:
                        vendorSpecificInformation.OcspReauthPeriodMin =
                            short.TryParse(keyValue[1], out var or) ? or : default(short);
                        break;
                    case DhcpConstants.OcspAcceptPrevGoodPeriodMinParameterName:
                        vendorSpecificInformation.OcspAcceptPrevGoodPeriodMin =
                            short.TryParse(keyValue[1], out var oa) ? oa : default(short);
                        break;
                    default:
                        vendorSpecificInformation.CustomParameters.Add(new Parameter(keyValue[0], keyValue[1]));
                        break;
                }
            }
        }

        private static void UpdateServiceDefinitions(
            Dictionary<string, bool> changedDefaults,
            ICollection<ServiceDefinition> serviceDefinitions,
            ServiceDefinition serviceDefinition)
        {
            if (changedDefaults.ContainsKey(serviceDefinition.Name))
            {
                if (!changedDefaults[serviceDefinition.Name])
                {
                    serviceDefinitions.Clear();
                    changedDefaults[serviceDefinition.Name] = true;
                }
            }

            serviceDefinitions.Add(serviceDefinition);
        }
    }
}