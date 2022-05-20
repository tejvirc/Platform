namespace Aristocrat.Monaco.Sas.Contracts.Metering
{
    using Aristocrat.Sas.Client.Metering;

    /// <summary>Different meter categories for SAS meter requests</summary>
    public enum MeterCategory
    {
        /// <summary>This is for an occurrence meter type</summary>
        Occurrence,

        /// <summary>This is for meters that are percentages</summary>
        Percentage,

        /// <summary>This is for all meters being reported as credits</summary>
        Credit,

        /// <summary>This is for all meters being reported as cents</summary>
        Cents,

        /// <summary>This is for all meters being reported as dollars</summary>
        Dollars
    }

    /// <summary>SasMeter provides additional information about a specific Sas Meter.</summary>
    public class SasMeter
    {
        private const int DefaultMeterLength = 4;

        /// <summary>Initializes a new instance of the SasMeter class.</summary>
        /// <param name="meterId">The Sas meter id.</param>
        /// <param name="mappedMeterName">The name of the client 12 meter mapped to this meter,
        /// or null if there is no direct mapping.</param>
        /// <param name="isPeriodMeter">Indicates if this meter is period or lifetime.</param>
        /// <param name="isGameMeter">Indicates if this meter is a per game meter.</param>
        /// <param name="category">The meter category for this meter</param>
        /// <param name="meterFieldLength">Specifies the field length of the meter</param>
        public SasMeter(
            SasMeterId meterId,
            string mappedMeterName,
            bool isPeriodMeter,
            bool isGameMeter,
            MeterCategory category,
            int meterFieldLength = DefaultMeterLength)
        {
            MeterId = meterId;
            MappedMeterName = mappedMeterName;
            PeriodMeter = isPeriodMeter;
            GameMeter = isGameMeter;
            Category = category;
            MeterFieldLength = meterFieldLength;
        }

        /// <summary>Gets the Sas meter id.</summary>
        public SasMeterId MeterId { get; }

        /// <summary>
        /// Gets the name of the Client 12 meter that is directly mapped to the Sas meter.
        /// If this value is null, then the meter is retrieved through additional means.
        /// </summary>
        public string MappedMeterName { get; }

        /// <summary>Gets a value indicating whether or not this meter is a period meter.</summary>
        public bool PeriodMeter { get; }

        /// <summary>Gets a value indicating whether or not this meter is a per game meter.</summary>
        public bool GameMeter { get; }

        /// <summary>Gets the length of the meter field.</summary>
        public int MeterFieldLength { get; }

        /// <summary>The type of SAS meter</summary>
        public MeterCategory Category { get; }
    }
}