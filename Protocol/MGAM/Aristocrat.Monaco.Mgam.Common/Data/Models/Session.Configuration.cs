namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     Configuration for <see cref="Session"/> model.
    /// </summary>
    public class SessionConfiguration : EntityTypeConfiguration<Session>
    {
        /// <summary>
        ///     Initializes an instance of the <see cref="SessionConfiguration"/> class.
        /// </summary>
        public SessionConfiguration()
        {
            ToTable(nameof(Session));

            HasKey(t => t.Id);

            Property(t => t.SessionId)
                .IsRequired();

            Property(t => t.OfflineVoucherBarcode)
                .IsRequired();

            Property(t => t.OfflineVoucherPrinted)
                .IsRequired();
        }
    }
}
