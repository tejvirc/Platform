namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Defines information about a central determinant template
    /// </summary>
    public class CentralInfo
    {
        /// <summary>
        ///     Gets or sets the template Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Gets or sets the denomination
        /// </summary>
        public long Denomination { get; set; }

        /// <summary>
        ///     Gets or sets the name of the template
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the bet line preset Id
        /// </summary>
        public int BetLinePresetId { get; set; }

        /// <summary>
        ///     Gets or set the bet multiplier
        /// </summary>
        public int BetMultiplier { get; set; }

        /// <summary>
        ///     Gets or sets the wager amount
        /// </summary>
        public int Bet { get; set; }

        /// <summary>
        ///     Universal Product Code for the Game Title
        /// </summary>
        public long Upc { get; set; }
    }
}