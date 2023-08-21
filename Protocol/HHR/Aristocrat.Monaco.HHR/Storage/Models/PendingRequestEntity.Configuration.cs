namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class FailedRequestEntityConfiguration : IEntityTypeConfiguration<PendingRequestEntity>
    {
        public void Configure(EntityTypeBuilder<PendingRequestEntity> builder)
        {
            builder.ToTable(nameof(PendingRequestEntity));
            builder.HasKey(t => t.Id);
	    }
    }
}
