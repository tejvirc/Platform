namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     The database configuration for <see cref="Host"/>
    /// </summary>
    public class HostConfiguration : EntityTypeConfiguration<Host>
    {
        /// <summary>
        ///     Creates an instance of <see cref="HostConfiguration"/>
        /// </summary>
        public HostConfiguration()
        {
            ToTable(nameof(Host));
            HasKey(x => x.Id);

            Property(x => x.ComPort)
                .IsRequired();
            Property(x => x.SasAddress)
                .IsRequired();
            Property(x => x.AccountingDenom)
                .IsRequired();
        }
    }
}