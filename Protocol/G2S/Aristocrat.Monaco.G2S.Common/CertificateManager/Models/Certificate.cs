namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Models
{
    using System;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Implementation of certificate entity.
    /// </summary>
    public class Certificate : BaseEntity
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Certificate" /> class.
        /// </summary>
        public Certificate()
            : this(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Certificate" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public Certificate(long id)
        {
            Id = id;
        }

        /// <summary>
        ///     Gets or sets get/sets the thumbprint for the certificate.
        /// </summary>
        public string Thumbprint { get; set; }

        /// <summary>
        ///     Gets or sets the serialized certificate.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        ///     Gets or sets the password for the certificate.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets get/sets verification date.
        /// </summary>
        public DateTime VerificationDate { get; set; }

        /// <summary>
        ///     Gets or sets get/sets OcspOfflineDate.
        /// </summary>
        public DateTime? OcspOfflineDate { get; set; }

        /// <summary>
        ///     Gets or sets the certificate status.
        /// </summary>
        public CertificateStatus Status { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this is the default certificate.
        /// </summary>
        public bool Default { get; set; }
    }
}