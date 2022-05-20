namespace Aristocrat.Monaco.Sas
{
    using Contracts.Metering;

    /// <summary>
    ///     The Sas Meter configuration
    /// </summary>
    public class SasMeterConfiguration
    {
        /// <summary>
        ///     Creates a SasMeterConfiguration
        /// </summary>
        /// <param name="meterName">The name of the meter</param>
        /// <param name="meterCategory">The meter category</param>
        public SasMeterConfiguration(string meterName, MeterCategory meterCategory)
        {
            MeterName = meterName;
            MeterCategory = meterCategory;
        }

        /// <summary>
        ///     Gets the meter name
        /// </summary>
        public string MeterName { get; }

        /// <summary>
        ///     Gets the meter category
        /// </summary>
        public MeterCategory MeterCategory { get; }
    }
}