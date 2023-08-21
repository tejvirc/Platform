namespace Aristocrat.Monaco.Bingo.Tests.Monitors
{
    using System.Globalization;
    using Application.Contracts;

    public class TestClassification : MeterClassification
    {
        public TestClassification()
            : base("Currency", 1000)
        {
        }

        public override string CreateValueString(long meterValue, CultureInfo culture = null) => string.Empty;
    }
}