namespace Aristocrat.Monaco.Common.Tests.Scheduler
{
    using System;
    using Common.Scheduler;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Quartz;
    using Quartz.Spi;
    using SimpleInjector;

    [TestClass]
    public class SimpleInjectorJobFactoryTest
    {
        [TestMethod]
        public void WhenNewJobExpectJobAdapter()
        {
            var jobFactory = new SimpleInjectorJobFactory(new Container());

            var triggerFiredBundle = new TriggerFiredBundle(
                new Mock<IJobDetail>().Object,
                new Mock<IOperableTrigger>().Object,
                new Mock<ICalendar>().Object,
                true,
                DateTimeOffset.MaxValue,
                DateTimeOffset.MaxValue,
                DateTimeOffset.MaxValue,
                DateTimeOffset.MaxValue);

            var job = jobFactory.NewJob(triggerFiredBundle, new Mock<IScheduler>().Object);

            Assert.IsNotNull(job);
            Assert.IsInstanceOfType(job, typeof(JobAdapter));
        }

        [TestMethod]
        public void WhenReturnJobExpectNoException()
        {
            var jobFactory = new SimpleInjectorJobFactory(new Container());
            jobFactory.ReturnJob(new Mock<IJob>().Object);
        }
    }
}