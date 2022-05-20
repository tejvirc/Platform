namespace TestKernelInterfaces
{
    using System;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PlatformBootedEventTest
    {
        [TestMethod]
        public void PlatformBootedEventConstructorTest()
        {
            // Just need to make sure the Time value of the event was initialized
            var target = new PlatformBootedEvent();

            Assert.IsTrue(target.Time != DateTime.MinValue);
        }

        [TestMethod]
        public void PlatformBootedEventConstructorWithTime()
        {
            var time = default(DateTime);
            var target = new PlatformBootedEvent(time);

            Assert.IsTrue(time == target.Time);
        }
    }
}