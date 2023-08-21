namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ManualHandicapEntityConfiguration : IEntityTypeConfiguration<ManualHandicapEntity>
    {
        public void Configure(EntityTypeBuilder<ManualHandicapEntity> builder)
        {
            builder.ToTable(nameof(ManualHandicapEntity));

            builder.HasKey(t => t.Id);

            builder.Property(t => t.IsCompleted).IsRequired();
        }
    }
}