namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The aft transfer options persistence configuration
    /// </summary>
    public class AftTransferOptionsConfiguration : EntityTypeConfiguration<AftTransferOptions>
    {
        /// <summary>
        ///     Creates an instance of <see cref="AftTransferOptionsConfiguration"/>
        /// </summary>
        public AftTransferOptionsConfiguration()
        {
            ToTable(nameof(AftTransferOptions));
            HasKey(x => x.Id);
            Property(x => x.IsTransferAcknowledgedByHost)
                .IsRequired();
            Property(x => x.CurrentTransfer)
                .IsRequired();
            Property(x => x.CurrentTransferFlags)
                .IsRequired();
        }
    }
}