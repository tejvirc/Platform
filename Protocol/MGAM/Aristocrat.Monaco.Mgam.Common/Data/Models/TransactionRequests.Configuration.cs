namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System.Data.Entity.ModelConfiguration;

    /// <summary>
    ///     Configuration for <see cref="TransactionRequests"/> model.
    /// </summary>
    public class TransactionRequestsConfiguration : EntityTypeConfiguration<TransactionRequests>
    {
        /// <summary>
        ///     Initializes an instance of the <see cref="TransactionRequestsConfiguration"/> class.
        /// </summary>
        public TransactionRequestsConfiguration()
        {
            ToTable(nameof(TransactionRequests));

            HasKey(t => t.Id);

            Property(t => t.Requests)
                .IsRequired();
        }
    }
}
