namespace Aristocrat.Monaco.Sas.Tests
{
    using System.Globalization;
    using Application.Contracts;

    /// <inheritdoc />
    public class TestMeterClassification : MeterClassification
    {
        /// <inheritdoc />
        public TestMeterClassification()
            : base("TestClassification", long.MaxValue)
        {
        }

        /// <inheritdoc />
        public TestMeterClassification(string name, long upperBounds)
            : base(name, upperBounds)
        {
        }

        /// <inheritdoc />
        public override string CreateValueString(long meterValue, CultureInfo culture = null)
        {
            return meterValue.ToString(CultureInfo.InvariantCulture);
        }
    }
}