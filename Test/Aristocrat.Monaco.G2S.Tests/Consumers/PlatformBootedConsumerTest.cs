namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Profile;
    using G2S.Consumers;
    using G2S.Handlers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class PlatformBootedConsumerTest
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
            var consumer = new PlatformBootedConsumer(null, null, null, null);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCommandBuilderIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var consumer = new PlatformBootedConsumer(egm.Object, null, null, null);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventLiftIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var consumer = new PlatformBootedConsumer(egm.Object, command.Object, null, null);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();
            var profiles = new Mock<IProfileService>();
            var consumer = new PlatformBootedConsumer(egm.Object, command.Object, lift.Object, profiles.Object);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeWithCriticalMemoryClearedExpectEvents()
        {
            var egm = new Mock<IG2SEgm>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();
            var profiles = new Mock<IProfileService>();

            var consumer = new PlatformBootedConsumer(egm.Object, command.Object, lift.Object, profiles.Object);

            var cabinet = new Mock<ICabinetDevice>();
            egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(cabinet.Object);
            cabinet.SetupGet(m => m.DeviceClass).Returns("G2S_cabinet");

            consumer.Consume(new PlatformBootedEvent(DateTime.UtcNow, true));

            lift.Verify(e => e.Report(cabinet.Object, EventCode.G2S_CBE325, It.IsAny<deviceList1>()));
            lift.Verify(e => e.Report(cabinet.Object, EventCode.G2S_CBE321));
            lift.Verify(e => e.Report(cabinet.Object, EventCode.G2S_CBE322));
        }

        [TestMethod]
        public void WhenConsumeWithoutCriticialMemoryClearedExpectOnlyBootedEvent()
        {
            var egm = new Mock<IG2SEgm>();
            var command = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();
            var profiles = new Mock<IProfileService>();

            var consumer = new PlatformBootedConsumer(egm.Object, command.Object, lift.Object, profiles.Object);

            var cabinet = new Mock<ICabinetDevice>();
            egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(cabinet.Object);
            cabinet.SetupGet(m => m.DeviceClass).Returns("G2S_cabinet");

            consumer.Consume(new PlatformBootedEvent(DateTime.UtcNow, false));

            lift.Verify(e => e.Report(cabinet.Object, EventCode.G2S_CBE325, It.IsAny<deviceList1>()));
            lift.Verify(e => e.Report(cabinet.Object, EventCode.G2S_CBE321), Times.Never);
            lift.Verify(e => e.Report(cabinet.Object, EventCode.G2S_CBE322), Times.Never);
        }
    }
}