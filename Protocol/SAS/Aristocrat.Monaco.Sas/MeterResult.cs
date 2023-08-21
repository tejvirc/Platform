namespace Aristocrat.Monaco.Sas
{
    /// <summary>
    ///     The meter manager query results
    /// </summary>
    public class MeterResult
    {
        /// <summary>
        ///     Gets the meter value read
        /// </summary>
        public long MeterValue { get; }

        /// <summary>
        ///     Gets the meter length
        /// </summary>
        public int MeterLength { get; }

        /// <summary>
        ///     Creates the MeterResult
        /// </summary>
        /// <param name="meterValue">The meter value read</param>
        /// <param name="meterLength">The meter length</param>
        public MeterResult(long meterValue, int meterLength)
        {
            MeterValue = meterValue;
            MeterLength = meterLength;
        }
    }
}