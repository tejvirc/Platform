namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Defines an upgrade action
    /// </summary>
    public class UpgradeAction
    {
        /// <summary>
        ///     Gets or sets the paytable identifier to be upgraded
        /// </summary>
        public string FromPaytableId { get; set; }

        /// <summary>
        ///     Gets or sets the target/new paytable identifier
        /// </summary>
        public string ToPaytableId { get; set; }

        /// <summary>
        ///     Gets or sets the denomination identifier
        /// </summary>
        public long DenomId { get; set; }

        /// <summary>
        ///     Gets or sets the version to be upgraded
        /// </summary>
        public string FromVersion { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not jackpot data will be migrated
        /// </summary>
        public bool MigrateJackpots { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the position in the lobby will be preserved
        /// </summary>
        public bool MaintainPosition { get; set; }
    }
}
