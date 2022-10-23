namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Contracts.SASProperties;

    /// <summary>
    ///     The database configuration for <see cref="SasFeatures"/> entity
    /// </summary>
    public class SasFeaturesConfiguration : IEntityTypeConfiguration<SasFeatures>
    {
        /// <summary>
        ///     Creates an instance of <see cref="SasFeaturesConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<SasFeatures> builder)
        {
            builder.ToTable(nameof(SasFeatures));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.HandpayReportingType)
                .IsRequired();
            builder.Property(x => x.TransferLimit)
                .IsRequired();
            builder.Property(x => x.MaxAllowedTransferLimits)
                .IsRequired();
            builder.Property(x => x.PartialTransferAllowed)
                .IsRequired();
            builder.Property(x => x.TransferInAllowed)
                .IsRequired();
            builder.Property(x => x.TransferOutAllowed)
                .IsRequired();
            builder.Property(x => x.AftBonusAllowed)
                .IsRequired();
            builder.Property(x => x.WinTransferAllowed)
                .IsRequired();
            builder.Property(x => x.LegacyBonusAllowed)
                .IsRequired();
            builder.Property(x => x.ValidationType)
                .IsRequired();
            builder.Property(x => x.ConfigNotification)
                .IsRequired();
            builder.Property(x => x.ConfigNotification)
                .IsRequired();
            builder.Property(x => x.DisableOnDisconnect)
                .IsRequired();
            builder.Property(x => x.NonSasProgressiveHitReporting)
                .IsRequired();
            builder.Property(x => x.DisabledOnPowerUp)
                .IsRequired();
            builder.Property(x => x.DisableOnDisconnectConfigurable)
                .IsRequired();
            builder.Property(x => x.GeneralControlEditable)
                .IsRequired();
            builder.Property(x => x.AddressConfigurableOnlyOnce)
                .IsRequired();
            builder.Property(x => x.BonusTransferStatusEditable)
                .IsRequired();
            builder.Property(x => x.ProgressiveGroupId)
                .IsRequired();
            builder.Property(x => x.DebitTransfersAllowed)
                .IsRequired();
            builder.Property(x => x.TransferToTicketAllowed)
                .IsRequired();
        }
    }
}