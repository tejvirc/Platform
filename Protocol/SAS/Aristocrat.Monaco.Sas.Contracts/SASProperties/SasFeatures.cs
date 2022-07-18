namespace Aristocrat.Monaco.Sas.Contracts.SASProperties
{
    using System;
    using Common.Storage;

    /// <summary>
    ///     The sas features
    /// </summary>
    public class SasFeatures : BaseEntity, ICloneable
    {
        /// <summary>
        ///     Gets or sets the fund transfer type which can be Aft or Eft.
        /// </summary>
        public FundTransferType FundTransferType { get; set; }

        /// <summary>
        ///     Gets whether or not Eft is allowed
        /// </summary>
        public bool EftAllowed => FundTransferType == FundTransferType.Eft && (TransferInAllowed || TransferOutAllowed);

        /// <summary>
        ///     Gets whether or not Aft is allowed
        /// </summary>
        public bool AftAllowed => FundTransferType == FundTransferType.Aft && (TransferInAllowed || TransferOutAllowed || AftBonusAllowed || WinTransferAllowed);

        /// <summary>
        ///     Gets or sets a value indicating the type of handpay reporting supported by the gaming machine
        /// </summary>
        public SasHandpayReportingType HandpayReportingType { get; set; }

        /// <summary>
        ///     Gets or sets the transfer limit
        /// </summary>
        public long TransferLimit { get; set; }

        /// <summary>
        ///     Gets or sets the max allowed transfer limits
        /// </summary>
        public long MaxAllowedTransferLimits { get; set; }

        /// <summary>
        ///     Gets or sets whether or not partial transfers are allowed
        /// </summary>
        public bool PartialTransferAllowed { get; set; }

        /// <summary>
        ///     Gets or sets whether or not transfer in is allowed
        /// </summary>
        public bool TransferInAllowed { get; set; }

        /// <summary>
        ///     Gets or sets whether or not debit transfers are allowed
        /// </summary>
        public bool DebitTransfersAllowed { get; set; }

        /// <summary>
        ///     Gets or sets whether or not transfers to tickets are allowed
        /// </summary>
        public bool TransferToTicketAllowed { get; set; }

        /// <summary>
        ///     Gets or sets whether or not transfer out is allowed
        /// </summary>
        public bool TransferOutAllowed { get; set; }

        /// <summary>
        ///     Gets or sets whether or not win transfer are allowed
        /// </summary>
        public bool WinTransferAllowed { get; set; }

        /// <summary>
        ///     Gets or sets whether or not bonus transfer are allowed
        /// </summary>
        public bool AftBonusAllowed { get; set; }

        /// <summary>
        ///     Gets or set whether or not legacy bonuses are allowed
        /// </summary>
        public bool LegacyBonusAllowed { get; set; }

        /// <summary>
        ///     Gets or sets the validation type
        /// </summary>
        public SasValidationType ValidationType { get; set; }

        /// <summary>
        ///     Gets or sets the overflow behavior
        /// </summary>
        public ExceptionOverflowBehavior OverflowBehavior { get; set; }

        /// <summary>
        ///     Gets or sets the configuration change notification
        /// </summary>
        public ConfigNotificationTypes ConfigNotification { get; set; }

        /// <summary>
        ///     Gets or sets whether or not we disable of host disconnect
        /// </summary>
        public bool DisableOnDisconnect { get; set; }

        /// <summary>
        ///     Gets or sets whether or not we disable on power up
        /// </summary>
        public bool DisabledOnPowerUp { get; set; }

        /// <summary>
        ///     Gets or sets whether or not disable on disconnect is configurable
        /// </summary>
        public bool DisableOnDisconnectConfigurable { get; set; }

        /// <summary>
        ///     Gets or sets whether or not Non Sas Progressive Hit Reporting
        /// </summary>
        public bool NonSasProgressiveHitReporting { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the general control port is editable
        /// </summary>
        public bool GeneralControlEditable { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the address is configuration only once
        /// </summary>
        public bool AddressConfigurableOnlyOnce { get; set; }

        /// <summary>
        ///     Gets or sets whether or not bonus transfers are editable
        /// </summary>
        public bool BonusTransferStatusEditable { get; set; }

        /// <summary>
        ///     Gets or sets the progressive group id
        /// </summary>
        public int ProgressiveGroupId { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}