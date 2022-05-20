namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     Data class to hold the request to read a single meter
    /// </summary>
    public class LongPollReadMeterData : LongPollMultiDenomAwareData
    {
        /// <summary>
        ///     Creates an instance of the LongPollReadMeterData class
        /// </summary>
        /// <param name="meter">The meter to get the value of</param>
        /// <param name="type">The type of the meter, either Lifetime or Period</param>
        public LongPollReadMeterData(SasMeters meter, MeterType type)
        {
            MeterType = type;
            Meter = meter;
        }

        /// <summary>
        ///     Creates an instance of the LongPollReadMeterData class
        /// </summary>
        public LongPollReadMeterData()
        {
        }

        /// <summary>
        ///     Gets or sets the meter being requested
        /// </summary>
        public SasMeters Meter { get; set; }

        /// <summary>
        ///     Gets or sets the type of meter being requested
        /// </summary>
        public MeterType MeterType { get; set; }

        /// <summary>
        ///     Gets or sets the accounting denom being requested to be used
        /// </summary>
        public long AccountingDenom { get; set; }
    }

    /// <summary>
    ///     Data class to hold the response of a single meter read
    /// </summary>
    public class LongPollReadMeterResponse : LongPollMultiDenomAwareResponse
    {
        /// <summary>
        ///     Creates an instance of the LongPollReadMeterResponse class
        /// </summary>
        /// <param name="meter">The meter associated with this value</param>
        /// <param name="value">The value of the meter</param>
        public LongPollReadMeterResponse(SasMeters meter, ulong value)
        {
            Meter = meter;
            MeterValue = value;
        }

        /// <summary>
        ///     Gets the meter being requested
        /// </summary>
        public SasMeters Meter { get; }

        /// <summary>
        ///     Gets the value of the meter
        /// </summary>
        public ulong MeterValue { get; }

        /// <summary>
        ///     Gets or sets the accounting denom being requested to be used
        /// </summary>
        public long AccountingDenom { get; set; }
    }
}