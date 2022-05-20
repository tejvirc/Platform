namespace Aristocrat.Monaco.G2S.Common.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using GAT.Storage;

    /// <summary>
    ///     Configuration for the <see cref="GatComponentVerification" /> entity
    /// </summary>
    public class GatComponentVerificationMap : EntityTypeConfiguration<GatComponentVerification>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GatComponentVerificationMap" /> class.
        /// </summary>
        public GatComponentVerificationMap()
        {
            ToTable("ComponentVerification");

            // Primary Key
            HasKey(t => t.Id);

            Property(t => t.AlgorithmType)
                .IsRequired();

            Property(t => t.Seed)
                .IsRequired();

            Property(t => t.Salt)
                .IsRequired();

            Property(t => t.StartOffset)
                .IsRequired();

            Property(t => t.EndOffset)
                .IsRequired();

            Property(t => t.State)
                .IsRequired();

            Property(t => t.Result)
                .IsRequired();

            Property(t => t.GatExec)
                .IsRequired();
        }
    }
}
