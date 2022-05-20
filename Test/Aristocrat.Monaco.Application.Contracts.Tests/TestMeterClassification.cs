namespace Aristocrat.Monaco.Application.Contracts.Tests
{
    using System.Globalization;

    /// <summary>
    ///     Definition of the TestMeterClassification class.
    /// </summary>
    public class TestMeterClassification : MeterClassification
    {
        /// <summary>
        ///     Initializes a new instance of the TestMeterClassification class
        /// </summary>
        public TestMeterClassification()
            : base("TestClassification", long.MaxValue)
        {
        }

        /// <summary>
        ///     Creates and returns a string representation of the value
        /// </summary>
        /// <param name="meterValue">The value to convert to a string</param>
        /// <returns>A string representation of the value</returns>
        public override string CreateValueString(long meterValue)
        {
            return meterValue.ToString(CultureInfo.InvariantCulture);
        }
    }
}