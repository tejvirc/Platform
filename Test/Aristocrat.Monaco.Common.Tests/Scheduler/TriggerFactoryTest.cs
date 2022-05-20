namespace Aristocrat.Monaco.Common.Tests.Scheduler
{
    using System;
    using Common.Scheduler;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TriggerFactoryTest
    {
        [TestMethod]
        public void WhenGetStartTriggerExpectSuccess()
        {
            var trigger = TriggerFactory.GetStartTrigger(DateTime.Now);
            Assert.IsNotNull(trigger);
        }

        [TestMethod]
        public void WhenGetTriggerExpectSuccess()
        {
            var trigger = TriggerFactory.GetTrigger("Monaco.Triggers", DateTime.Now);
            Assert.IsNotNull(trigger);
        }
    }
}