namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using System.Data.Entity.ModelConfiguration;

    public class FailedRequestEntityConfiguration : EntityTypeConfiguration<PendingRequestEntity>
    {
	    public FailedRequestEntityConfiguration()
	    {
		    ToTable(nameof(PendingRequestEntity));

		    HasKey(t => t.Id);
	    }
    }
}
