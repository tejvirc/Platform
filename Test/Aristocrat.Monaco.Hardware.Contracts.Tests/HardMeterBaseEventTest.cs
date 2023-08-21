namespace Aristocrat.Monaco.Hardware.Tests.ContractsTests
{
    using Contracts.HardMeter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This file contains the tests for HardMeterBaseEvent
    /// </summary>
    [TestClass]
    public class HardMeterBaseEventTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            // constructor with no parameters test
            var hardMeterBaseEvent = new TestHardMeterEvent();
            Assert.IsNotNull(hardMeterBaseEvent);

            // constructor with logicalId test
            int logicalId = 1;
            hardMeterBaseEvent = new TestHardMeterEvent(logicalId);
            Assert.IsNotNull(hardMeterBaseEvent);
            Assert.AreEqual(logicalId, hardMeterBaseEvent.LogicalId);
        }

        /// <summary>
        ///     Since HardMeterBaseEvent is abstract, create a test subclass to test it
        /// </summary>
        public class TestHardMeterEvent : HardMeterBaseEvent
        {
            public TestHardMeterEvent()
            {
            }

            public TestHardMeterEvent(int logicalId)
                : base(logicalId)
            {
            }
        }
    }
}