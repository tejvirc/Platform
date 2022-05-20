namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using G2S.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class SystemEnabledEventConsumerTest
    {
        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEgmIsNullExpectException()
        {
            var consumer = new SystemEnabledEventConsumer(null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();

            var consumer = new SystemEnabledEventConsumer(egm.Object);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeExpectRemoveState()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<ICabinetDevice>();

            egm.Setup(e => e.GetDevice<ICabinetDevice>()).Returns(device.Object);
            device.SetupGet(d => d.DeviceClass).Returns("cabinet");

            var consumer = new SystemEnabledEventConsumer(egm.Object);

            consumer.Consume(new SystemEnabledEvent());

            device.Verify(d => d.RemoveAllConditions());
        }
    }
}