namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     The database configuration for <see cref="Host"/>
    /// </summary>
    public class HostConfiguration : IEntityTypeConfiguration<Host>
    {
        /// <summary>
        ///     Creates an instance of <see cref="HostConfiguration"/>
        /// </summary>
        public void Configure(EntityTypeBuilder<Host> builder)
        {
            builder.ToTable(nameof(Host));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ComPort)
                .IsRequired();
            builder.Property(x => x.SasAddress)
                .IsRequired();
            builder.Property(x => x.AccountingDenom)
                .IsRequired();
        }
    }
}