namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Models
{
    using System;

    /// <summary>
    ///     Get certificate status result
    /// </summary>
    public class GetCertificateStatusResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCertificateStatusResult" /> class.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="verified">The last date/time the certificate was validated</param>
        /// <param name="offline">The date/time the OSCP server was noted as offline</param>
        /// <param name="nextUpDateTime">The next up date time.</param>
        public GetCertificateStatusResult(CertificateStatus status, DateTime verified, DateTime? offline, DateTime? nextUpDateTime)
        {
            Status = status;
            Verified = verified;
            Offline = offline;
            NextUpDateTime = nextUpDateTime;
        }

        /// <summary>
        ///     Gets the status.
        /// </summary>
        /// <value>
        ///     The status.
        /// </value>
        public CertificateStatus Status { get; }

        /// <summary>
        ///     Gets the last time the certificate was validated
        /// </summary>
        public DateTime Verified { get; }

        /// <summary>
        ///     The last time the OCSP server was noted as offline
        /// </summary>
        public DateTime? Offline { get; }

        /// <summary>
        ///     Gets the next up date time.
        /// </summary>
        /// <value>
        ///     The next up date time.
        /// </value>
        public DateTime? NextUpDateTime { get; }
    }
}