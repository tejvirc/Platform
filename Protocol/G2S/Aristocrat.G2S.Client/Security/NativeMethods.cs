namespace Aristocrat.G2S.Client.Security
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Wraps the httpapi.dll
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        ///     Determines how client certificates are checked.
        /// </summary>
        [Flags]
        public enum CertCheckModes : uint
        {
            /// <summary>
            ///     Enables the client certificate revocation check.
            /// </summary>
            None = 0,

            /// <summary>
            ///     Client certificate is not to be verified for revocation.
            /// </summary>
            DoNotVerifyCertificateRevocation = 1,

            /// <summary>
            ///     Only cached certificate is to be used the revocation check.
            /// </summary>
            VerifyRevocationWithCachedCertificateOnly = 2,

            /// <summary>
            ///     The RevocationFreshnessTime setting is enabled.
            /// </summary>
            EnableRevocationFreshnessTime = 4,

            /// <summary>
            ///     No usage check is to be performed.
            /// </summary>
            NoUsageCheck = 0x10000
        }

        /// <summary>
        ///     The HTTP_SERVICE_CONFIG_ID enumeration type defines service configuration options.
        /// </summary>
        public enum HttpServiceConfigId
        {
            /// <summary>
            ///     Specifies the IP Listen List used to register IP addresses on which to listen for SSL connections.
            /// </summary>
            HttpServiceConfigIpListenList = 0,

            /// <summary>
            ///     Specifies the SSL certificate store.
            /// </summary>
            HttpServiceConfigSslCertInfo,

            /// <summary>
            ///     Specifies the URL reservation store.
            /// </summary>
            HttpServiceConfigUrlAclInfo,

            /// <summary>
            ///     Terminates the enumeration; is not used to define a service configuration option.
            /// </summary>
            HttpServiceConfigMax
        }

        /// <summary>
        ///     The HTTP_SERVICE_CONFIG_QUERY_TYPE enumeration type defines various types of queries to make. It is used in the
        ///     HTTP_SERVICE_CONFIG_SSL_QUERY and HTTP_SERVICE_CONFIG_URLACL_QUERY structures.
        /// </summary>
        public enum HttpServiceConfigQueryType
        {
            /// <summary>
            ///     The query returns a single record that matches the specified key value.
            /// </summary>
            HttpServiceConfigQueryExact = 0,

            /// <summary>
            ///     The query iterates through the store and returns all records in sequence, using an index value that the calling
            ///     process increments between query calls.
            /// </summary>
            HttpServiceConfigQueryNext,

            /// <summary>
            ///     Terminates the enumeration; is not used to define a query type.
            /// </summary>
            HttpServiceConfigQueryMax
        }

        /// <summary>
        ///     Service config ssl flags
        /// </summary>
        [Flags]
        public enum HttpServiceConfigSslFlag : uint
        {
            /// <summary>
            ///     None
            /// </summary>
            None = 0,

            /// <summary>
            ///     Client certificates are mapped where possible to corresponding operating-system user accounts based on the
            ///     certificate mapping rules stored in Active Directory.
            /// </summary>
            UseDsMapper = 0x00000001,

            /// <summary>
            ///     Enables a client certificate to be cached locally for subsequent use.
            /// </summary>
            NegotiateClientCert = 0x00000002,

            /// <summary>
            ///     Prevents SSL requests from being passed to low-level ISAPI filters.
            /// </summary>
            NoRawFilter = 0x00000004
        }

        /// <summary>
        ///     Default intitialization value
        /// </summary>
        public const uint HttpInitializeConfig = 0x00000002;

        /// <summary>
        ///     No error
        /// </summary>
        public const uint NoError = 0;

        /// <summary>
        ///     The buffer size specified in the ConfigInformationLength parameter is insufficient.
        /// </summary>
        public const uint ErrorInsufficientBuffer = 122;

        /// <summary>
        ///     The specified record already exists, and must be deleted in order for its value to be re-set.
        /// </summary>
        public const uint ErrorAlreadyExists = 183;

        /// <summary>
        ///     ErrorFileNotFound
        /// </summary>
        public const uint ErrorFileNotFound = 2;

        /// <summary>
        ///     ErrorNoMoreItems
        /// </summary>
        public const int ErrorNoMoreItems = 259;

        private static readonly HttpApiVersion ApiVersion = new HttpApiVersion(1, 0);

        /// <summary>
        ///     Executes the defined action
        /// </summary>
        /// <param name="body">The action to invoke.</param>
        public static void CallHttpApi(Action body)
        {
            var retVal = HttpInitialize(ApiVersion, HttpInitializeConfig, IntPtr.Zero);
            ThrowWin32ExceptionIfError(retVal);

            try
            {
                body();
            }
            finally
            {
                retVal = HttpTerminate(HttpInitializeConfig, IntPtr.Zero);
                ThrowWin32ExceptionIfError(retVal);
            }
        }

        /// <summary>
        ///     Throws if the return value is an error
        /// </summary>
        /// <param name="retVal">The HttpApi return value.</param>
        public static void ThrowWin32ExceptionIfError(uint retVal)
        {
            if (retVal != NoError)
            {
                throw new Win32Exception(Convert.ToInt32(retVal));
            }
        }

        /// <summary>
        ///     The HttpInitialize function initializes the HTTP Server API driver, starts it, if it has not already been started,
        ///     and allocates data structures for the calling application to support response-queue creation and other operations.
        ///     Call this function before calling any other functions in the HTTP Server API.
        /// </summary>
        /// <param name="version">
        ///     HTTP version. This parameter is an HTTPAPI_VERSION structure. For the current version, declare an
        ///     instance of the structure and set it to the pre-defined value HTTPAPI_VERSION_1 before passing it to
        ///     HttpInitialize.
        /// </param>
        /// <param name="flags">Initialization options</param>
        /// <param name="pReserved">This parameter is reserved and must be NULL.</param>
        /// <returns>If the function succeeds, the return value is NO_ERROR.</returns>
        [DllImport("httpapi.dll", SetLastError = true)]
        public static extern uint HttpInitialize(HttpApiVersion version, uint flags, IntPtr pReserved);

        /// <summary>
        ///     The HttpSetServiceConfiguration function creates and sets a configuration record for the HTTP Server API
        ///     configuration store. The call fails if the specified record already exists. To change a given configuration record,
        ///     delete it and then recreate it with a different value.
        /// </summary>
        /// <param name="serviceIntPtr">Reserved. Must be zero.</param>
        /// <param name="configId">
        ///     ype of configuration record to be set. This parameter can be one of the following values from
        ///     the HTTP_SERVICE_CONFIG_ID enumeration.
        /// </param>
        /// <param name="pConfigInformation">
        ///     Type of configuration record to be set. This parameter can be one of the following
        ///     values from the HTTP_SERVICE_CONFIG_ID enumeration.
        /// </param>
        /// <param name="configInformationLength">Size, in bytes, of the pConfigInformation buffer.</param>
        /// <param name="pOverlapped">This parameter is reserved and must be NULL.</param>
        /// <returns>If the function succeeds, the return value is NO_ERROR.</returns>
        [DllImport("httpapi.dll", SetLastError = true)]
        public static extern uint HttpSetServiceConfiguration(
            IntPtr serviceIntPtr,
            HttpServiceConfigId configId,
            IntPtr pConfigInformation,
            int configInformationLength,
            IntPtr pOverlapped);

        /// <summary>
        ///     The HttpDeleteServiceConfiguration function deletes specified data, such as IP addresses or SSL Certificates, from
        ///     the HTTP Server API configuration store, one record at a time.
        /// </summary>
        /// <param name="serviceIntPtr">This parameter is reserved and must be zero.</param>
        /// <param name="configId">
        ///     ype of configuration record to be set. This parameter can be one of the following values from
        ///     the HTTP_SERVICE_CONFIG_ID enumeration.
        /// </param>
        /// <param name="pConfigInformation">
        ///     Type of configuration record to be set. This parameter can be one of the following
        ///     values from the HTTP_SERVICE_CONFIG_ID enumeration.
        /// </param>
        /// <param name="configInformationLength">Size, in bytes, of the pConfigInformation buffer.</param>
        /// <param name="pOverlapped">This parameter is reserved and must be NULL.</param>
        /// <returns>0 for success</returns>
        [DllImport("httpapi.dll", SetLastError = true)]
        public static extern uint HttpDeleteServiceConfiguration(
            IntPtr serviceIntPtr,
            HttpServiceConfigId configId,
            IntPtr pConfigInformation,
            int configInformationLength,
            IntPtr pOverlapped);

        /// <summary>
        ///     The HttpTerminate function cleans up resources used by the HTTP Server API to process calls by an application. An
        ///     application should call HttpTerminate once for every time it called HttpInitialize, with matching flag settings.
        /// </summary>
        /// <param name="flags">Termination options.</param>
        /// <param name="pReserved">This parameter is reserved and must be NULL.</param>
        /// <returns>0 for success</returns>
        [DllImport("httpapi.dll", SetLastError = true)]
        public static extern uint HttpTerminate(uint flags, IntPtr pReserved);

        /// <summary>
        ///     Queries the http service
        /// </summary>
        /// <param name="serviceIntPtr">Reserved. Must be zero.</param>
        /// <param name="configId">
        ///     The configuration record query type. This parameter is one of the following values from the
        ///     HTTP_SERVICE_CONFIG_ID enumeration.
        /// </param>
        /// <param name="pInputConfigInfo">
        ///     A pointer to a structure whose contents further define the query and of the type that
        ///     correlates with ConfigId in the following table.
        /// </param>
        /// <param name="inputConfigInfoLength">Size, in bytes, of the pInputConfigInfo buffer.</param>
        /// <param name="pOutputConfigInfo">
        ///     A pointer to a buffer in which the query results are returned. The type of this buffer
        ///     correlates with ConfigId.
        /// </param>
        /// <param name="outputConfigInfoLength">Size, in bytes, of the pOutputConfigInfo buffer.</param>
        /// <param name="pReturnLength">
        ///     A pointer to a variable that receives the number of bytes to be written in the output
        ///     buffer. If the output buffer is too small, the call fails with a return value of ERROR_INSUFFICIENT_BUFFER. The
        ///     value pointed to by pReturnLength can be used to determine the minimum length the buffer requires for the call to
        ///     succeed.
        /// </param>
        /// <param name="pOverlapped">Reserved for asynchronous operation and must be set to NULL.</param>
        /// <returns>If the function succeeds, the return value is NO_ERROR.</returns>
        [DllImport("httpapi.dll", SetLastError = true)]
        public static extern uint HttpQueryServiceConfiguration(
            IntPtr serviceIntPtr,
            HttpServiceConfigId configId,
            IntPtr pInputConfigInfo,
            int inputConfigInfoLength,
            IntPtr pOutputConfigInfo,
            int outputConfigInfoLength,
            out int pReturnLength,
            IntPtr pOverlapped);

        /// <summary>
        ///     The HTTP_SERVICE_CONFIG_SSL_SET structure is used to add a new record to the SSL store or retrieve an existing
        ///     record from it. An instance of the structure is used to pass data in to the HTTPSetServiceConfiguration function
        ///     through the pConfigInformation parameter or to retrieve data from the HTTPQueryServiceConfiguration function
        ///     through the pOutputConfigInformation parameter when the ConfigId parameter of either function is equal to
        ///     HTTPServiceConfigSSLCertInfo.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct HttpServiceConfigSslSet
        {
            /// <summary>
            ///     An HTTP_SERVICE_CONFIG_SSL_KEY structure that identifies the SSL certificate record.
            /// </summary>
            public HttpServiceConfigSslKey KeyDesc;

            /// <summary>
            ///     An HTTP_SERVICE_CONFIG_SSL_PARAM structure that holds the contents of the specified SSL certificate record.
            /// </summary>
            public HttpServiceConfigSslParam ParamDesc;
        }

        /// <summary>
        ///     The HTTP_SERVICE_CONFIG_SSL_KEY structure serves as the key by which a given Secure Sockets Layer (SSL) certificate
        ///     record is identified. It appears in the HTTP_SERVICE_CONFIG_SSL_SET and the HTTP_SERVICE_CONFIG_SSL_QUERY
        ///     structures, and is passed as the pConfigInformation parameter to HTTPDeleteServiceConfiguration,
        ///     HttpQueryServiceConfiguration, and HttpSetServiceConfiguration when the ConfigId parameter is set to
        ///     HttpServiceConfigSSLCertInfo.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct HttpServiceConfigSslKey
        {
            /// <summary>
            ///     Pointer to a sockaddr structure that contains the Internet Protocol (IP) address with which this SSL certificate is
            ///     associated.
            /// </summary>
            public IntPtr IpPort;

            /// <summary>
            ///     Initializes a new instance of the <see cref="HttpServiceConfigSslKey" /> struct.
            /// </summary>
            /// <param name="pIpPort">Ip port</param>
            public HttpServiceConfigSslKey(IntPtr pIpPort)
            {
                IpPort = pIpPort;
            }
        }

        /// <summary>
        ///     The HTTP_SERVICE_CONFIG_SSL_PARAM structure defines a record in the SSL configuration store. Together with a
        ///     HTTP_SERVICE_CONFIG_SSL_KEY structure, it makes up the HTTP_SERVICE_CONFIG_SSL_SET structure passed to
        ///     HttpSetServiceConfiguration function in the pConfigInformation parameter when the ConfigId parameter is set to
        ///     HttpServiceConfigSSLCertInfo.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct HttpServiceConfigSslParam
        {
            /// <summary>
            ///     The size, in bytes, of the SSL hash.
            /// </summary>
            public int SslHashLength;

            /// <summary>
            ///     A pointer to the SSL certificate hash.
            /// </summary>
            public IntPtr SslHash;

            /// <summary>
            ///     A unique identifier of the application setting this record.
            /// </summary>
            public Guid AppId;

            /// <summary>
            ///     A pointer to a wide-character string that contains the name of the store from which the server certificate is to be
            ///     read. If set to NULL, "MY" is assumed as the default name. The specified certificate store name must be present in
            ///     the Local System store location.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)] public string SslCertStoreName;

            /// <summary>
            ///     Determines how client certificates are checked.
            /// </summary>
            public CertCheckModes DefaultCertCheckMode;

            /// <summary>
            ///     The number of seconds after which to check for an updated certificate revocation list (CRL). If this value is zero,
            ///     the new CRL is updated only when the previous one expires.
            /// </summary>
            public int DefaultRevocationFreshnessTime;

            /// <summary>
            ///     The timeout interval, in milliseconds, for an attempt to retrieve a certificate revocation list from the remote
            ///     URL.
            /// </summary>
            public int DefaultRevocationUrlRetrievalTimeout;

            /// <summary>
            ///     A pointer to an SSL control identifier, which enables an application to restrict the group of certificate issuers
            ///     to be trusted. This group must be a subset of the certificate issuers trusted by the machine on which the
            ///     application is running.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)] public string DefaultSslCtlIdentifier;

            /// <summary>
            ///     The name of the store where the control identifier pointed to by pDefaultSslCtlIdentifier is stored.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)] public string DefaultSslCtlStoreName;

            /// <summary>
            ///     A combination of zero or more of the following flag values can be combined with OR as appropriate.
            /// </summary>
            public HttpServiceConfigSslFlag DefaultFlags;
        }

        /// <summary>
        ///     The HTTPAPI_VERSION structure defines the version of the HTTP Server API. This is not to be confused with the
        ///     version of the HTTP protocol used, which is stored in an HTTP_VERSION structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct HttpApiVersion
        {
            /// <summary>
            ///     Major version of the HTTP Server API.
            /// </summary>
            public ushort HttpApiMajorVersion;

            /// <summary>
            ///     Minor version of the HTTP Server API.
            /// </summary>
            public ushort HttpApiMinorVersion;

            /// <summary>
            ///     Initializes a new instance of the <see cref="HttpApiVersion" /> struct.
            /// </summary>
            /// <param name="majorVersion">The major version</param>
            /// <param name="minorVersion">The minor version</param>
            public HttpApiVersion(ushort majorVersion, ushort minorVersion)
            {
                HttpApiMajorVersion = majorVersion;
                HttpApiMinorVersion = minorVersion;
            }
        }

        /// <summary>
        ///     The HTTP_SERVICE_CONFIG_SSL_QUERY structure is used to specify a particular record to query in the SSL
        ///     configuration store. It is passed to the HttpQueryServiceConfiguration function using the pInputConfigInfo
        ///     parameter when the ConfigId parameter is set to HttpServiceConfigSSLCertInfo.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct HttpServiceConfigSslQuery
        {
            /// <summary>
            ///     One of the following values from the HTTP_SERVICE_CONFIG_QUERY_TYPE enumeration.
            /// </summary>
            public HttpServiceConfigQueryType QueryDesc;

            /// <summary>
            ///     If the QueryDesc parameter is equal to HttpServiceConfigQueryExact, then KeyDesc should contain an
            ///     HTTP_SERVICE_CONFIG_SSL_KEY structure that identifies the SSL certificate record queried. If the QueryDesc
            ///     parameter is equal to HTTPServiceConfigQueryNext, then KeyDesc is ignored.
            /// </summary>
            public HttpServiceConfigSslKey KeyDesc;

            /// <summary>
            ///     If the QueryDesc parameter is equal to HTTPServiceConfigQueryNext, then dwToken must be equal to zero on the first
            ///     call to the HttpQueryServiceConfiguration function, one on the second call, two on the third call, and so forth
            ///     until all SSL certificate records are returned, at which point HttpQueryServiceConfiguration returns
            ///     ERROR_NO_MORE_ITEMS.
            /// </summary>
            public uint Token;
        }
    }
}