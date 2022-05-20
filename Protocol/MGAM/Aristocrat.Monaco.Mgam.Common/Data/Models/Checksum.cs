namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Monaco.Common.Storage;

    /// <summary>
    ///     Stores data about the Checksum.
    /// </summary>
    public class Checksum : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the Seed value.
        /// </summary>
        public int Seed { get; set; }
    }
}
