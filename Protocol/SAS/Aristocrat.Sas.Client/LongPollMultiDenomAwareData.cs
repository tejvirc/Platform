namespace Aristocrat.Sas.Client
{
    /// <summary>
    ///     Error codes for the multi-denom preamble long poll. See SAS 6.03 Table 16.1c.
    /// </summary>
    public enum MultiDenomAwareErrorCode
    {
        NoError = 0,
        LongPollNotSupported,
        ImproperlyFormatted,
        NotMultiDenomAware,
        SpecificDenomNotSupported,
        NotValidPlayerDenom
    }

    public class LongPollMultiDenomAwareData : LongPollData
    {
        /// <summary>
        ///     Target denomination for command, generally in cents to match SAS spec
        /// </summary>
        public long TargetDenomination { get; set; }

        /// <summary>
        ///     Whether or not this data should be treated as multi-denom.
        /// </summary>
        public bool MultiDenomPoll { get; set; }
    }

    public class LongPollMultiDenomAwareResponse : LongPollResponse
    {
        public MultiDenomAwareErrorCode ErrorCode { get; set; }
    }
}
