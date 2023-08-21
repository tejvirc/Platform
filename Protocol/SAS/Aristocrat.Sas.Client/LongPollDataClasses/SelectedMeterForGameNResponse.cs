namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using Metering;

    /// <inheritdoc />
    public class SelectedMeterForGameNResponse : LongPollResponse
    {
        /// <summary>
        ///     Gets the meter code for this result
        /// </summary>
        public SasMeterId MeterCode { get; }

        /// <summary>
        ///     Gets the meter value
        /// </summary>
        public ulong MeterValue { get; }

        /// <summary>
        ///     Gets the minimum meter length
        /// </summary>
        public int MinMeterLength { get; }

        /// <summary>
        ///     Gets the actual meter length
        /// </summary>
        public int MeterLength { get; }

        /// <summary>
        ///     Creates the SelectedMeterForGameNResponse
        /// </summary>
        /// <param name="meterCode">the meter code for this result</param>
        /// <param name="meterValue">the meter value</param>
        /// <param name="minMeterLength">the minimum meter length</param>
        /// <param name="meterLength">the actual meter length</param>
        public SelectedMeterForGameNResponse(
            SasMeterId meterCode,
            ulong meterValue,
            int minMeterLength,
            int meterLength)
        {
            MeterCode = meterCode;
            MeterValue = meterValue;
            MinMeterLength = minMeterLength;
            MeterLength = meterLength;
        }
    }
}