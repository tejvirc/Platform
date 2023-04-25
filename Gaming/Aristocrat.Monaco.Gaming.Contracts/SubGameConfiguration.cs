namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Server configuration data of a given sub game
    /// </summary>
    public class SubGameConfiguration
    {
        /// <summary>
        ///     The title id of the sub game 
        /// </summary>
        public long GameTitleId { get; set; }

        /// <summary>
        ///     The Denomination of the associated sub game
        /// </summary>
        public long Denomination { get; set; }
    }
}
