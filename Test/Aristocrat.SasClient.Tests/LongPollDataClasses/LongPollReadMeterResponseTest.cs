namespace Aristocrat.SasClient.Tests.LongPollDataClasses
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Contains unit tests for the LongPollReadMeterResponse class
    /// </summary>
    [TestClass]
    public class LongPollReadMeterResponseTest
    {
        [TestMethod]
        public void ConstructorWithParametersTest()
        {
            var meterValue = 1234UL;
            var target = new LongPollReadMeterResponse(SasMeters.CurrentCredits, meterValue);

            Assert.AreEqual(SasMeters.CurrentCredits, target.Meter);
            Assert.AreEqual(meterValue, target.MeterValue);
        }
    }
}
