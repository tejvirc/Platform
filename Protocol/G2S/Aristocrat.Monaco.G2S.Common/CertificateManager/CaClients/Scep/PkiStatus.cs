namespace Aristocrat.Monaco.G2S.Common.CertificateManager.CaClients.Scep
{
    /// <summary>
    ///     Pki statuses list
    /// </summary>
    public enum PkiStatus
    {
        /// <summary>
        ///     Unable to parse Pki Status.
        /// </summary>
        ParseError = -1,

        /// <summary>
        ///     Means that request granted (response in pkcsPKIEnvelope)
        /// </summary>
        Success = 0,

        /// <summary>
        ///     Means that request rejected (details in failInfo attribute)
        /// </summary>
        Failure = 2,

        /// <summary>
        ///     Means that request awaits manual approval.
        /// </summary>
        Pending = 3
    }
}