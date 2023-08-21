namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Models
{
    using Monaco.Common.Storage;

    /// <summary>
    ///     Implementation of certificate configuration entity.
    /// </summary>
    public class PkiConfiguration : BaseEntity
    {
        private const int DefaultScepPollingIntervalSec = 60;
        private const short DefaultOcspPeriodForOfflineMin = 240;
        private const short DefaultOcspReauthPeriodMin = 600;
        private const short DefaultOcspAcceptPrevCertPeriodMin = 720;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PkiConfiguration" /> class.
        /// </summary>
        public PkiConfiguration()
            : this(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PkiConfiguration" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public PkiConfiguration(long id)
        {
            Id = id;

            ScepManualPollingInterval = DefaultScepPollingIntervalSec;
            OcspMinimumPeriodForOffline = DefaultOcspPeriodForOfflineMin;
            OcspReAuthenticationPeriod = DefaultOcspReauthPeriodMin;
            OcspAcceptPreviouslyGoodCertificatePeriod = DefaultOcspAcceptPrevCertPeriodMin;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether SCEP is enabled.
        /// </summary>
        public bool ScepEnabled { get; set; }

        /// <summary>
        ///     Gets or sets gets/sets Certificate Mgr Location
        /// </summary>
        public string CertificateManagerLocation { get; set; }

        /// <summary>
        ///     Gets or sets gets/sets Scep CaIdent
        /// </summary>
        public string ScepCaIdent { get; set; }

        /// <summary>
        ///     Gets or sets gets/sets Scep Username
        /// </summary>
        public string ScepUsername { get; set; }

        /// <summary>
        ///     Gets or sets the key size
        /// </summary>
        public int KeySize { get; set; }

        /// <summary>
        ///     Gets or sets the scep manual polling interval in seconds.
        /// </summary>
        public int ScepManualPollingInterval { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether OCSP is enabled.
        /// </summary>
        public bool OcspEnabled { get; set; }

        /// <summary>
        ///     Gets or sets gets/sets Certificate Status Location
        /// </summary>
        public string CertificateStatusLocation { get; set; }

        /// <summary>
        ///     Gets or sets gets/sets Ocsp Minimum Period For Offline (gsaOO)
        /// </summary>
        public short OcspMinimumPeriodForOffline { get; set; }

        /// <summary>
        ///     Gets or sets gets/sets OCSP: Re-Authenticate Certificate Period (gsaOR)
        /// </summary>
        public short OcspReAuthenticationPeriod { get; set; }

        /// <summary>
        ///     Gets or sets gets/sets OCSP: Accept Previously Good Certificate Period (gsaOA)
        /// </summary>
        public short OcspAcceptPreviouslyGoodCertificatePeriod { get; set; }

        /// <summary>
        ///     Gets or sets gets/sets Ocsp Next Update
        /// </summary>
        public int? OcspNextUpdate { get; set; }

        /// <summary>
        ///     Gets or sets gets/sets Offline method type
        /// </summary>
        public OfflineMethodType OfflineMethod { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether nonces are enabled
        /// </summary>
        public bool NoncesEnabled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the domain should be validated
        /// </summary>
        public bool ValidateDomain { get; set; }

        /// <summary>
        ///     Gets or sets the name of the common.
        /// </summary>
        public string CommonName { get; set; }

        /// <summary>
        ///     Gets or sets the organization unit.
        /// </summary>
        public string OrganizationUnit { get; set; }
    }
}