namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>Represents the tax status of a legacy bonus.</summary>
    public enum TaxStatus
    {
        /// <summary>A tax status of deductible.</summary>
        Deductible = 0x00,

        /// <summary>A tax status of nondeductible.</summary>
        Nondeductible = 0x01,

        /// <summary>A tax status of wager match.</summary>
        WagerMatch = 0x02,
    }

    /// <summary>
    ///     Holds the data for Legacy Bonus Awards Data.
    /// </summary>
    public class LegacyBonusAwardsData : LongPollData
    {
        /// <summary>Gets or sets the BonusAmount of the legacy bonus award.</summary>
        public long BonusAmount { get; set; }

        /// <summary>Gets or sets the TaxStatus of the legacy bonus award.</summary>
        public TaxStatus TaxStatus { get; set; }

        /// <summary>The accounting denom for this award</summary>
        public long AccountingDenom { get; set; }
    }
}