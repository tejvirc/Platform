namespace Aristocrat.Monaco.G2S.Common.DHCP
{
    /// <summary>
    ///     Contains DHCP service and parameter name constants.
    /// </summary>
    public static class DhcpConstants
    {
        /// <summary>
        ///     Communication config service name.
        /// </summary>
        public const string CommConfigServiceName = "g2sCC";

        /// <summary>
        ///     Certificate manager service name.
        /// </summary>
        public const string CertificateManagerServiceName = "gsaCM";

        /// <summary>
        ///     Certificate status service name.
        /// </summary>
        public const string CertificateStatusServiceName = "gsaCS";

        /// <summary>
        ///     OCSP minimum period for offline parameter name.
        /// </summary>
        public const string OcspMinimumPeriodForOfflineMinParameterName = "gsaOO";

        /// <summary>
        ///     OCSP re-authenticate certificate period parameter name.
        /// </summary>
        public const string OcspReauthPeriodMinParameterName = "gsaOR";

        /// <summary>
        ///     OCSP accept previously good certificate period name.
        /// </summary>
        public const string OcspAcceptPrevGoodPeriodMinParameterName = "gsaOA";

        /// <summary>
        ///     SCEP CA-IDENT parameter name.
        /// </summary>
        public const string CaIdent = "c";

        internal const int DhcpClientPort = 68;

        internal const int DhcpServerPort = 67;
    }
}