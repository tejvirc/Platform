namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Model;
    using Protocol.Common.Storage;

    /// <summary>
    ///     Configuration for the <see cref="VoucherDataMap" /> entity
    /// </summary>
    public class VoucherDataMap : IEntityTypeConfiguration<VoucherData>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherDataMap" /> class.
        /// </summary>
        public void Configure(EntityTypeBuilder<VoucherData> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ToTable(nameof(VoucherData));
            builder.HasKey(t => t.Id);
            builder.Property(t => t.ListId)
                .IsRequired();
            builder.Property(t => t.ValidationId)
                .IsRequired();
            builder.Property(t => t.ValidationSeed)
                .IsRequired();
            builder.Property(t => t.ListTime)
                .IsRequired();
        }
    }
}