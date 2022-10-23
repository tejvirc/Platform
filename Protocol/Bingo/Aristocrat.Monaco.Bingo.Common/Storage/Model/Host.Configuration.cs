namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class HostConfiguration : IEntityTypeConfiguration<Host>
    {
        public void Configure(EntityTypeBuilder<Host> builder)
        {
            builder.ToTable(nameof(Host));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.HostName).IsRequired();
            builder.Property(x => x.Port).IsRequired();
        }
    }
}