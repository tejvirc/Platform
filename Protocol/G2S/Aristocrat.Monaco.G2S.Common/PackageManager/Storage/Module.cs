namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using System.Globalization;
    using System.Text;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Entity that represents module.
    /// </summary>
    public class Module : BaseEntity
    {
        /// <summary>
        ///     Gets or sets Module Id
        /// </summary>
        public string ModuleId { get; set; }

        /// <summary>
        ///     Gets or sets Package Id
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        ///     Gets or sets package status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///     Returns a human-readable representation of a Module.
        /// </summary>
        /// <returns>A human-readable string.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} [ModuleId={1}, PackageId={2}, Status={3}",
                GetType(),
                ModuleId,
                PackageId,
                Status);

            return builder.ToString();
        }
    }
}