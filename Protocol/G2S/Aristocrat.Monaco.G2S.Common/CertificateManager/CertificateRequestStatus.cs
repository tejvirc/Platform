namespace Aristocrat.Monaco.G2S.Common.CertificateManager
{
    /// <summary>
    ///     Certificate request statuses
    /// </summary>
    public enum CertificateRequestStatus
    {
        /// <summary>
        ///     The request is pending
        /// </summary>
        Pending,

        /// <summary>
        ///     Enrollment has completed successfully
        /// </summary>
        Enrolled,

        /// <summary>
        ///     The request was denied
        /// </summary>
        Denied,

        /// <summary>
        ///     Indicates an error occurred during enrollment
        /// </summary>
        Error
    }
}