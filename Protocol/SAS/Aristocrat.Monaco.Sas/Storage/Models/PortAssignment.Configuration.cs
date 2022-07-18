namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The database configuration for <see cref="PortAssignment"/>
    /// </summary>
    public class PortAssignmentConfiguration : EntityTypeConfiguration<PortAssignment>
    {
        /// <summary>
        ///     Creates an instance of <see cref="PortAssignmentConfiguration"/>
        /// </summary>
        public PortAssignmentConfiguration()
        {
            ToTable(nameof(PortAssignment));
            HasKey(x => x.Id);

            Property(x => x.IsDualHost)
                .IsRequired();
            Property(x => x.FundTransferPort)
                .IsRequired();
            Property(x => x.FundTransferType)
                .IsRequired();
            Property(x => x.GeneralControlPort)
                .IsRequired();
            Property(x => x.LegacyBonusPort)
                .IsRequired();
            Property(x => x.ProgressivePort)
                .IsRequired();
            Property(x => x.ValidationPort)
                .IsRequired();
            Property(x => x.GameStartEndHosts)
                .IsRequired();
            Property(x => x.Host1NonSasProgressiveHitReporting)
                .IsRequired();
            Property(x => x.Host2NonSasProgressiveHitReporting)
                .IsRequired();
        }
    }
}