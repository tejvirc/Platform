namespace Aristocrat.Monaco.Sas.Contracts.Metering
{
    using Application.Contracts;

    /// <summary>
    /// Class for passing meter information to the SasBaseMeterProvider base class.
    /// </summary>
    public class MeterInfo
    {
        /// <summary>
        /// Initializes a new instance of the MeterInfo class.
        /// </summary>
        /// <param name="name">Name of the meter.</param>
        /// <param name="meterClassification">Meter classification of this meter.</param>
        public MeterInfo(string name, MeterClassification meterClassification)
        {
            Name = name;
            MeterClassification = meterClassification;
        }

        /// <summary>
        /// Gets or sets the name of this meter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the meter classification of this meter.
        /// </summary>
        public MeterClassification MeterClassification { get; set; }
    }
}
