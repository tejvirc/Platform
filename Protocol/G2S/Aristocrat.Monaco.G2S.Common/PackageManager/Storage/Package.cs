namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using System.Globalization;
    using System.Text;
    using Monaco.Common.Storage;
    using Data.Model;

    /// <summary>
    ///     Implementation of package entity.
    /// </summary>
    public class Package : BaseEntity
    {
        /// <summary>
        ///     Gets or sets Package Id
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        ///     Gets or sets get/sets package size.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        ///     Gets or sets package state.
        /// </summary>
        public PackageState State { get; set; }

        /// <summary>
        ///     Gets or sets the hash of the package (256 bytes)
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        ///     Returns a human-readable representation of a Package.
        /// </summary>
        /// <returns>A human-readable string.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} [PackageId={1}, State={2}, Size={3}, Hash={4}",
                GetType(),
                PackageId,
                State,
                Size,                
                Hash);

            return builder.ToString();
        }
    }
}