namespace Aristocrat.Monaco.G2S.Security
{
    using Common.CertificateManager;
    using Kernel;

    /// <summary>
    ///     Published when a certificate status has been updated.
    /// </summary>
    public class CertificateStatusUpdatedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateStatusUpdatedEvent" /> class.
        /// </summary>
        /// <param name="status">Certificate request status.</param>
        public CertificateStatusUpdatedEvent(CertificateRequestStatus status)
        {
            Status = status;
        }

        /// <summary>
        ///     Gets the certificate status
        /// </summary>
        public CertificateRequestStatus Status { get; }
    }
}