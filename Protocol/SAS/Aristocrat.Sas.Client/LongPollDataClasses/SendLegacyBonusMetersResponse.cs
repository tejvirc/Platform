namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     Holds the data for Send Legacy Bonus Meters Response.
    /// </summary>
    public class LongPollSendLegacyBonusMetersResponse : LongPollResponse
    {
        /// <summary> Gets or sets a value indicating Deductible bonus meter </summary>
        public ulong Deductible { get; set; }

        /// <summary> Gets or sets a value indicating Non-Deductible bonus meter </summary>
        public ulong NonDeductible { get; set; }

        /// <summary> Gets or sets a value indicating WagerMatch bonus meter </summary>
        public ulong WagerMatch { get; set; }
    }

    public class LongPollSendLegacyBonusMetersData : LongPollData
    {
        public int GameId { get; set; }

        public long AccountingDenom { get; set; }
    }
}