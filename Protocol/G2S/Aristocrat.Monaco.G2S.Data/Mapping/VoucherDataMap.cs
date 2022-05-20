namespace Aristocrat.Monaco.G2S.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Model;

    /// <summary>
    ///     Configuration for the <see cref="VoucherDataMap" /> entity
    /// </summary>
    public class VoucherDataMap : EntityTypeConfiguration<VoucherData>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherDataMap" /> class.
        /// </summary>
        public VoucherDataMap()
        {
            ToTable("VoucherData");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.ListId)
                .IsRequired();

            Property(t => t.ValidationId)
                .IsRequired();

            Property(t => t.ValidationSeed)
                .IsRequired();

            Property(t => t.ListTime)
                .IsRequired();
        }
    }
}