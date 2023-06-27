namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using G2S.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class TimeUpdateConsumerTest
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
            var consumer = new TimeUpdatedConsumer(null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventLiftIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var consumer = new TimeUpdatedConsumer(egm.Object, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var lift = new Mock<IEventLift>();

            var consumer = new TimeUpdatedConsumer(egm.Object, lift.Object);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeWithSpanBelowThresholdExpectNoEvent()
        {
            var egm = new Mock<IG2SEgm>();
            var lift = new Mock<IEventLift>();
            var device = new Mock<ICabinetDevice>();

            egm.Setup(e => e.GetDevice<ICabinetDevice>()).Returns(device.Object);

            var consumer = new TimeUpdatedConsumer(egm.Object, lift.Object);

            consumer.Consume(new TimeUpdatedEvent(TimeSpan.Zero));

            lift.Verify(
                l => l.Report(
                    It.Is<IDevice>(d => device.Object == d),
                    It.Is<string>(e => e == EventCode.G2S_CBE315)),
                Times.Never);
        }

        [TestMethod]
        public void WhenConsumeExpectEvent()
        {
            var egm = new Mock<IG2SEgm>();
            var lift = new Mock<IEventLift>();
            var device = new Mock<ICabinetDevice>();

            egm.Setup(e => e.GetDevice<ICabinetDevice>()).Returns(device.Object);

            var consumer = new TimeUpdatedConsumer(egm.Object, lift.Object);

            consumer.Consume(new TimeUpdatedEvent(TimeSpan.FromMinutes(1)));

            lift.Verify(
                l => l.Report(
                    It.Is<IDevice>(d => device.Object == d),
                    It.Is<string>(e => e == EventCode.G2S_CBE315),
                    It.IsAny<IEvent>()));
        }
    }
}