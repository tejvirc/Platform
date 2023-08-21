namespace Aristocrat.Monaco.Application.Contracts.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Tests for DoorOpenAlarmExtensionNode
    /// </summary>
    [TestClass]
    public class DoorOpenAlarmExtensionNodeTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            var target = new DoorOpenAlarmExtensionNode();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            var target = new DoorOpenAlarmExtensionNode { RepeatSeconds = "1", LoopCount = "1", OperatorCanCancel = "true" };
            Assert.IsNotNull(target);
            Assert.AreEqual("1", target.RepeatSeconds);
            Assert.AreEqual("1", target.LoopCount);
            Assert.AreEqual("true", target.OperatorCanCancel);
        }
    }
}