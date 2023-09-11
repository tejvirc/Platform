namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    //using System.ServiceModel.Channels;
    //using CoreWCF;
    using CoreWCF.Channels;
    using System.Xml;
    //using System.ServiceModel;

    /// <summary>
    ///     Utility methods for endpoints
    /// </summary>
    public static class EndpointUtilities
    {
        private const int MaxDepth = 32;
        private const int MaxReceivedMessageSize = 2147483647;
        private const int MaxStringContentLength = 2147483647;
        private const int MaxArrayLength = 65536;
        private const int MaxBytesPerRead = 16384;
        private const int MaxNameTableCharCount = 65536;
        private const int MaxBufferPoolSize = 524288;
        private const int MaxBufferSize = 2147483647;


        /// <summary>
        ///     Creates a BasicHttpBinding from given address
        /// </summary>
        /// <param name="address">endpoint address to bind to</param>
        /// <param name="bypassCertificateValidation">Determines whether or not to bypass SSL certificate validation</param>
        /// <returns>a BasicHttpBinding as a Binding</returns>
        public static Binding Binding(Uri address, bool bypassCertificateValidation)
        {
            var readerQuotas = new XmlDictionaryReaderQuotas
            {
                MaxDepth = MaxDepth,
                MaxStringContentLength = MaxStringContentLength,
                MaxArrayLength = MaxArrayLength,
                MaxBytesPerRead = MaxBytesPerRead,
                MaxNameTableCharCount = MaxNameTableCharCount
            };

            var customBinding = new CustomBinding();

            customBinding.Elements.Add(
                new HostTextMessageBindingElement
                {
                    ReaderQuotas = readerQuotas
                });

            TransportBindingElement bindingElement = null;

            //PLANA: Some of the following features have been deprecated in CoreWCF.
            if (address.IsSecure())
            {
                bindingElement = new HttpsTransportBindingElement
                {
                    RequireClientCertificate = !bypassCertificateValidation,
                    //AllowCookies = false,
                    AuthenticationScheme = AuthenticationSchemes.Anonymous,
                    //BypassProxyOnLocal = false,
                    //DecompressionEnabled = true,
                    ExtendedProtectionPolicy = new ExtendedProtectionPolicy(
                        PolicyEnforcement.Always,
                        ProtectionScenario.TransportSelected,
                        null),
                    //HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                    KeepAliveEnabled = true,
                    ManualAddressing = false,
                    MaxBufferPoolSize = MaxBufferPoolSize,
                    MaxBufferSize = MaxBufferSize,
                    MaxReceivedMessageSize = MaxReceivedMessageSize,
                    //MaxPendingAccepts = 0,
                    //ProxyAuthenticationScheme = AuthenticationSchemes.Anonymous,
                    TransferMode = CoreWCF.TransferMode.Buffered, //TransferMode.Buffered,
                    //UseDefaultWebProxy = true
                };
            }
            else
            {
                bindingElement = new HttpTransportBindingElement
                {
                    //AllowCookies = false,
                    AuthenticationScheme = AuthenticationSchemes.Anonymous,
                    //BypassProxyOnLocal = false,
                    //DecompressionEnabled = true,
                    ExtendedProtectionPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.Never),
                    //HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                    KeepAliveEnabled = true,
                    ManualAddressing = false,
                    MaxBufferPoolSize = MaxBufferPoolSize,
                    MaxBufferSize = MaxBufferSize,
                    MaxReceivedMessageSize = MaxReceivedMessageSize,
                    //MaxPendingAccepts = 0,
                    //ProxyAuthenticationScheme = AuthenticationSchemes.Anonymous,
                    TransferMode = CoreWCF.TransferMode.Buffered,
                    //UseDefaultWebProxy = true
                };
            }
            
            customBinding.Elements.Add(bindingElement);

            return customBinding;
        }

        /// <summary>
        ///     Creates a BasicHttpBinding from given address
        /// </summary>
        /// <param name="address">endpoint address to bind to</param>
        /// <returns>a BasicHttpBinding as a Binding</returns>
        public static System.ServiceModel.Channels.Binding ClientBinding(Uri address)
        {
            var readerQuotas = new XmlDictionaryReaderQuotas
            {
                MaxDepth = MaxDepth,
                MaxStringContentLength = MaxStringContentLength,
                MaxArrayLength = MaxArrayLength,
                MaxBytesPerRead = MaxBytesPerRead,
                MaxNameTableCharCount = MaxNameTableCharCount
            };

            if (address.IsSecure())
            {
                var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport)
                {
                    MaxReceivedMessageSize = MaxReceivedMessageSize,
                    ReaderQuotas = readerQuotas
                };

                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

                return binding;
            }

            return new BasicHttpBinding
            {
                MaxReceivedMessageSize = MaxReceivedMessageSize,
                ReaderQuotas = readerQuotas
            };
        }

        /// <summary>
        ///     Checks for valid URI scheme (http or https)
        /// </summary>
        /// <param name="address">the address</param>
        /// <returns>true if address's scheme is valid</returns>
        public static bool IsSchemeValid(Uri address)
        {
            if (string.Compare(address.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }

            return string.Compare(address.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        ///     Checks if the URI is accessed through the Secure Hypertext Transfer Protocol.
        /// </summary>
        /// <param name="this">The address</param>
        /// <returns>true if address's scheme is accessed through the Secure Hypertext Transfer Protocol</returns>
        public static bool IsSecure(this Uri @this)
        {
            return @this.Scheme == Uri.UriSchemeHttps;
        }

        /// <summary>
        ///     Converts a Uri string into an IPEndPoint.
        /// </summary>
        /// <param name="endPoint">The Uri string</param>
        /// <returns>An IPEndPoint representation of endPoint</returns>
        public static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            // Strip the protocol
            int i = endPoint.IndexOf("://");
            if (i >= 0) endPoint = endPoint.Substring(i + 3);

            string[] ep = endPoint.Split(':');
            if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;

            if (ep.Length > 2)
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    throw new FormatException("Invalid IP address");
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    throw new FormatException("Invalid IP address");
                }
            }
            int port;
            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                var portString = ep[ep.Length - 1];
                if (!int.TryParse(portString.Remove(portString.Length - 1, 1), NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
                {
                    throw new FormatException("Invalid port");
                }
            }
            return new IPEndPoint(ip, port);
        }

        /// <summary>
        ///     Converts a key string to a byte array for G2S endpoint security.
        /// </summary>
        /// <param name="key">The key represented as a comma separated string</param>
        /// <returns>The key represented as a byte array</returns>
        public static byte[] EncryptorKeyStringToArray(string key)
        {
            string[] bytes = key.Split(',');
            return bytes.Select(b => byte.Parse(b)).ToArray();
        }
    }
}