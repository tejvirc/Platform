namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Consumers;
    using G2S.Handlers;
    using Hardware.Contracts.HardMeter;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using IDevice = Aristocrat.G2S.Client.Devices.IDevice;

    [TestClass]
    public class HardMeterEnabledConsumerTest
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
            var consumer = new HardMeterEnabledConsumer(null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCommandBuilderIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var consumer = new HardMeterEnabledConsumer(egm.Object, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventLiftIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();

            var consumer = new HardMeterEnabledConsumer(egm.Object, builder.Object, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();

            var consumer = new HardMeterEnabledConsumer(egm.Object, builder.Object, lift.Object);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeExpectRaiseEvent()
        {
            var egm = new Mock<IG2SEgm>();
            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var lift = new Mock<IEventLift>();
            var device = new Mock<ICabinetDevice>();

            egm.Setup(e => e.GetDevice<ICabinetDevice>()).Returns(device.Object);
            device.SetupGet(d => d.DeviceClass).Returns("cabinet");

            var consumer = new HardMeterEnabledConsumer(egm.Object, builder.Object, lift.Object);

            consumer.Consume(new EnabledEvent(EnabledReasons.System));

            lift.Verify(
                l => l.Report(
                    It.Is<IDevice>(d => device.Object == d),
                    It.Is<string>(e => e == EventCode.G2S_CBE327),
                    It.IsAny<deviceList1>(),
                    It.IsAny<IEvent>()));
        }
    }
}