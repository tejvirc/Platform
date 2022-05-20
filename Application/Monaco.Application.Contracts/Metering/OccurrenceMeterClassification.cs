namespace Aristocrat.Monaco.Application.Contracts
{
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     A classification of meters that represent a count of something,
    ///     and rolls over at 99,999,999 (8 digits)
    /// </summary>
    public class OccurrenceMeterClassification : MeterClassification
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OccurrenceMeterClassification" /> class.
        /// </summary>
        public OccurrenceMeterClassification()
            : base(
                "Occurrence",
                (long)ServiceManager.GetInstance().GetService<IPropertiesManager>().GetProperty(
                    ApplicationConstants.OccurrenceMeterRolloverText,
                    100000000))
        {
        }

        /// <summary>
        ///     Creates and returns a string representation of the value
        /// </summary>
        /// <param name="meterValue">The value to convert to a string</param>
        /// <returns>A string representation of the value</returns>
        public override string CreateValueString(long meterValue)
        {
            return string.Format(NumberFormatInfo.CurrentInfo, "{0:N0}", meterValue);
        }
    }
}