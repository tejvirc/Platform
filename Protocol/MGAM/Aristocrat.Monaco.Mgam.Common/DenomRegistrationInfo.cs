namespace Aristocrat.Monaco.Mgam.Common
{
    /// <summary>
    ///     Denomination registration information.
    /// </summary>
    public struct DenomRegistrationInfo
    {
        /// <summary>
        ///     Gets or sets the game ID.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        ///     Gets or sets the denomination in millicents.
        /// </summary>
        public long Denomination { get; set; }
    }
}
