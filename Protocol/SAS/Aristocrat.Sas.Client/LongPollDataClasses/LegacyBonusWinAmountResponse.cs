namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     Holds the data for Legacy Bonus Win Amount Response.
    /// </summary>
    public class LegacyBonusWinAmountResponse : LongPollResponse
    {
        /// <summary>Gets or sets the Multiplied Win.</summary>
        public ulong MultipliedWin { get; set; }

        /// <summary>Gets or sets the Multiplier.</summary>
        public byte Multiplier { get; set; }

        /// <summary>Gets or sets the BonusAmount of the legacy bonus award.</summary>
        public ulong BonusAmount { get; set; }

        /// <summary>Gets or sets the TaxStatus of the legacy bonus award.</summary>
        public TaxStatus TaxStatus { get; set; }
    }
}