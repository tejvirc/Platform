namespace Aristocrat.Monaco.G2S.Tests.Meters
{
    using G2S.Meters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Scheduler;

    [TestClass]
    public class MeterReportJobTest
    {
        [TestMethod]
        public void SerializationTest()
        {
            var meterReportJob = MeterReportJob.Create(10);

            Assert.AreEqual(
                meterReportJob.SubscriptionId.ToString(),
                meterReportJob.SerializeJobData());
        }

        [TestMethod]
        public void DeserealizationTest()
        {
            var meterReportJob = MeterReportJob.Create(10);

            var value = 15;
            meterReportJob.DeserializeJobData(value.ToString());

            Assert.AreEqual(value, meterReportJob.SubscriptionId);
        }

        [TestMethod]
        public void WhenExecuteExpectNoException()
        {
            var meterReportJob = MeterReportJob.Create(10);
            meterReportJob.Execute(new TaskSchedulerContext());
        }

        private void HandleMeterReport(long subId)
        {
        }
    }
}