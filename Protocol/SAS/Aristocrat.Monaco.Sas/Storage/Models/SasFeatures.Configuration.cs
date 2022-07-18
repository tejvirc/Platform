namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;
    using Contracts.SASProperties;

    /// <summary>
    ///     The database configuration for <see cref="SasFeatures"/> entity
    /// </summary>
    public class SasFeaturesConfiguration : EntityTypeConfiguration<SasFeatures>
    {
        /// <summary>
        ///     Creates an instance of <see cref="SasFeaturesConfiguration"/>
        /// </summary>
        public SasFeaturesConfiguration()
        {
            ToTable(nameof(SasFeatures));
            HasKey(x => x.Id);

            Property(x => x.HandpayReportingType)
                .IsRequired();
            Property(x => x.FundTransferType)
                .IsRequired();
            Property(x => x.TransferLimit)
                .IsRequired();
            Property(x => x.MaxAllowedTransferLimits)
                .IsRequired();
            Property(x => x.PartialTransferAllowed)
                .IsRequired();
            Property(x => x.TransferInAllowed)
                .IsRequired();
            Property(x => x.TransferOutAllowed)
                .IsRequired();
            Property(x => x.AftBonusAllowed)
                .IsRequired();
            Property(x => x.WinTransferAllowed)
                .IsRequired();
            Property(x => x.LegacyBonusAllowed)
                .IsRequired();
            Property(x => x.ValidationType)
                .IsRequired();
            Property(x => x.ConfigNotification)
                .IsRequired();
            Property(x => x.ConfigNotification)
                .IsRequired();
            Property(x => x.DisableOnDisconnect)
                .IsRequired();
            Property(x => x.NonSasProgressiveHitReporting)
                .IsRequired();
            Property(x => x.DisabledOnPowerUp)
                .IsRequired();
            Property(x => x.DisableOnDisconnectConfigurable)
                .IsRequired();
            Property(x => x.GeneralControlEditable)
                .IsRequired();
            Property(x => x.AddressConfigurableOnlyOnce)
                .IsRequired();
            Property(x => x.BonusTransferStatusEditable)
                .IsRequired();
            Property(x => x.ProgressiveGroupId)
                .IsRequired();
            Property(x => x.DebitTransfersAllowed)
                .IsRequired();
            Property(x => x.TransferToTicketAllowed)
                .IsRequired();
        }
    }
}