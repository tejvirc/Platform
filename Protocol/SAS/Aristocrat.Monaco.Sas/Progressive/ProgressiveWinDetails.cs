namespace Aristocrat.Monaco.Sas.Progressive
{
    /// <summary>
    ///     The details for a progressive win for a game
    /// </summary>
    public class ProgressiveWinDetails
    {
        /// <summary>
        ///     The total win amount for progressive in that game
        /// </summary>
        public long WinAmount { get; set; }

        /// <summary>
        ///     The highest level hit
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        ///     The group Id for the highest level hit
        /// </summary>
        public int GroupId { get; set; }
    }
}