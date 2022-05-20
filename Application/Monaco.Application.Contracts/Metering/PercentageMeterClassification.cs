namespace Aristocrat.Monaco.Application.Contracts
{
    using System.Globalization;

    /// <summary>
    ///     A classification of meters that represent a percentage,
    /// </summary>
    /// <remarks>
    ///     A meter value with this classification is currently on the XSpin
    ///     calculated at runtime. The upper bound passed to the base class
    ///     is not applied in calculating.
    /// </remarks>
    public class PercentageMeterClassification : MeterClassification
    {
        /// <summary>
        ///     The multiplier for converting the decimal to long.
        /// </summary>
        public const double Multiplier = 10000.00;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PercentageMeterClassification" /> class.
        /// </summary>
        public PercentageMeterClassification()
            : base("Percentage", 99999999)
        {
        }

        /// <summary>
        ///     Creates and returns a string representation of the value
        /// </summary>
        /// <param name="meterValue">The value to convert to a string</param>
        /// <returns>A string representation of the value</returns>
        public override string CreateValueString(long meterValue)
        {
            var percentage = meterValue / Multiplier;
            return string.Format(NumberFormatInfo.CurrentInfo, "{0:P2}", percentage);
        }
    }
}