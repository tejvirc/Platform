namespace Aristocrat.Monaco.Hardware.Tests.ContractsTests
{
    using Contracts.HardMeter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This file contains the tests for LogicalHardMeterContractsTest
    /// </summary>
    [TestClass]
    public class LogicalHardMeterContractsTest
    {
        [TestMethod]
        public void ConstructorTest1()
        {
            var target = new LogicalHardMeter();

            Assert.IsNotNull(target);
            Assert.AreEqual(0, target.PhysicalId);
            Assert.AreEqual(HardMeterState.Uninitialized, target.State);
            Assert.AreEqual(HardMeterAction.Off, target.Action);
            Assert.AreEqual(string.Empty, target.Name);
            Assert.AreEqual(string.Empty, target.LocalizedName);
            Assert.AreEqual(1, target.TickValue);
            Assert.AreEqual(0, target.Count);
            Assert.IsFalse(target.Ready);
            Assert.IsFalse(target.Suspended);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            int pysicalId = 123;
            int logicalId = 1;
            var hardMeterState = HardMeterState.Disabled;
            var hardMeterAction = HardMeterAction.On;
            string name = "test hard meter";
            string localizedName = "?? test hard meter??";
            int tickValue = 456;
            var target = new LogicalHardMeter(
                pysicalId,
                logicalId,
                name,
                localizedName,
                tickValue,
                true)
            {
                State = HardMeterState.Disabled,
                Action = HardMeterAction.On
            };

            Assert.IsNotNull(target);
            Assert.AreEqual(pysicalId, target.PhysicalId);
            Assert.AreEqual(logicalId, target.LogicalId);
            Assert.AreEqual(hardMeterState, target.State);
            Assert.AreEqual(hardMeterAction, target.Action);
            Assert.AreEqual(name, target.Name);
            Assert.AreEqual(localizedName, target.LocalizedName);
            Assert.AreEqual(tickValue, target.TickValue);
            Assert.AreEqual(0, target.Count);
            Assert.IsTrue(target.Ready);
            Assert.IsFalse(target.Suspended);
        }
    }
}