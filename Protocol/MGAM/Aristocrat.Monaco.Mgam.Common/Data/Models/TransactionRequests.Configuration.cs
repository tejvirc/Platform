namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    ///     Configuration for <see cref="TransactionRequests"/> model.
    /// </summary>
    public class TransactionRequestsConfiguration : IEntityTypeConfiguration<TransactionRequests>
    {
        /// <summary>
        ///     Initializes an instance of the <see cref="TransactionRequestsConfiguration"/> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<TransactionRequests> builder)
        {
            builder.ToTable(nameof(TransactionRequests));

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Requests)
                .IsRequired();
        }
    }
}
