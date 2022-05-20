namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using G2S.Consumers;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CashOutButtonPressedConsumerTest
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
            var consumer = new CashOutButtonPressedConsumer(null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventLiftIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var consumer = new CashOutButtonPressedConsumer(egm.Object, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPropertiesManagerIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var lift = new Mock<IEventLift>();

            var consumer = new CashOutButtonPressedConsumer(egm.Object, lift.Object, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var lift = new Mock<IEventLift>();
            var pm = new Mock<IPropertiesManager>();

            var consumer = new CashOutButtonPressedConsumer(egm.Object, lift.Object, pm.Object);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeExpectRaiseEvent()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<ICabinetDevice>();
            var lift = new Mock<IEventLift>();
            var pm = new Mock<IPropertiesManager>();

            egm.Setup(e => e.GetDevice<ICabinetDevice>()).Returns(device.Object);
            device.SetupGet(d => d.DeviceClass).Returns("cabinet");

            pm.Setup(p => p.GetProperty("Automation.HandleCashOut", It.IsAny<object>()))
                .Returns(true);

            var consumer = new CashOutButtonPressedConsumer(egm.Object, lift.Object, pm.Object);

            consumer.Consume(new CashOutButtonPressedEvent());

            lift.Verify(
                l => l.Report(
                    It.Is<IDevice>(d => device.Object == d),
                    It.Is<string>(e => e == EventCode.G2S_CBE316)));
        }
    }
}