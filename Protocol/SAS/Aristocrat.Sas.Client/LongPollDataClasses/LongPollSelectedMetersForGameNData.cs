namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System.Collections.Generic;
    using Metering;

    /// <summary>
    ///     Holds the values for sending selected game meters
    /// </summary>
    public class LongPollSelectedMetersForGameNData : LongPollMultiDenomAwareData
    {
        /// <summary>
        ///     Gets or sets the game number
        /// </summary>
        public ulong GameNumber { get; set; }

        /// <summary>
        ///     Gets or sets the accounting denom
        /// </summary>
        public long AccountingDenom { get; set; }

        /// <summary>
        ///     Gets or sets the requested meters list
        /// </summary>
        public List<SasMeterId> RequestedMeters { get; } = new List<SasMeterId>();
    }
}