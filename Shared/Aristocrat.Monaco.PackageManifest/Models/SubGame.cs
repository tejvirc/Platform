namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     Contains data for a SubGame
    /// </summary>
    public class SubGame
    {
        /// <summary>
        ///     Gets or sets the game TitleId.
        /// </summary>
        public long TitleId { get; set; }

        /// <summary>
        ///     Gets or sets the Unique game id
        /// </summary>
        public long UniqueGameId { get; set; }

        /// <summary>
        ///     Gets or sets the denominations.
        /// </summary>
        public IEnumerable<long> Denominations { get; set; }
    }
}
