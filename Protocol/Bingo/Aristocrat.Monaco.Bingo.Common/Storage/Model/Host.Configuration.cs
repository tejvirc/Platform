namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.Data.Entity.ModelConfiguration;

    public class HostConfiguration : EntityTypeConfiguration<Host>
    {
        public HostConfiguration()
        {
            ToTable(nameof(Host));
            HasKey(x => x.Id);
            Property(x => x.HostName)
                .IsRequired();
            Property(x => x.Port)
                .IsRequired();
        }
    }
}