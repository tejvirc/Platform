namespace Aristocrat.Monaco.Kernel.Tests.Events
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SystemDisabledRemovedEventTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            var expectedPriority = SystemDisablePriority.Normal;
            var expectedGuid = Guid.Empty;
            var expectedReason = "test";
            var expectedSystemDisabled = true;
            var expectedSystemIdleStateAffected = false;
            var target = new SystemDisableRemovedEvent(
                expectedPriority,
                expectedGuid,
                expectedReason,
                expectedSystemDisabled,
                expectedSystemIdleStateAffected);

            Assert.IsNotNull(target);
            Assert.AreEqual(expectedReason, target.DisableReasons);
            Assert.AreEqual(expectedPriority, target.Priority);
            Assert.AreEqual(expectedGuid, target.DisableId);
            Assert.AreEqual(expectedSystemDisabled, target.SystemDisabled);
            Assert.AreEqual(expectedSystemIdleStateAffected, target.SystemIdleStateAffected);
        }
    }
}
