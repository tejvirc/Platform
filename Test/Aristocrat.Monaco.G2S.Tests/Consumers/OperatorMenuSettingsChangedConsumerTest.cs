namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.G2S.Services;
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
    public class OperatorMenuSettingsChangedConsumerTest
    {
        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IProgressiveService>(MockBehavior.Default);
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
            var consumer = new OperatorMenuSettingsChangedConsumer(null, null, null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCabinetCommandBuilderIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var consumer = new OperatorMenuSettingsChangedConsumer(egm.Object, null, null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenOptionConfigCommandBuilderIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var builder1 = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();

            var consumer = new OperatorMenuSettingsChangedConsumer(egm.Object, builder1.Object, null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenProgressiveBuilderIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var builder1 = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var builder2 = new Mock<ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus>>();

            var consumer = new OperatorMenuSettingsChangedConsumer(egm.Object, builder1.Object, builder2.Object, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventLiftIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var builder1 = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var builder2 = new Mock<ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus>>();
            var builder3 = new Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>>();

            var consumer = new OperatorMenuSettingsChangedConsumer(egm.Object, builder1.Object, builder2.Object, builder3.Object, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var builder1 = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var builder2 = new Mock<ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus>>();
            var builder3 = new Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>>();
            var lift = new Mock<IEventLift>();

            var consumer = new OperatorMenuSettingsChangedConsumer(egm.Object, builder1.Object, builder2.Object, builder3.Object, lift.Object);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeExpectRaiseEvent()
        {
            var egm = new Mock<IG2SEgm>();
            var builder1 = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var builder2 = new Mock<ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus>>();
            var builder3 = new Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>>();
            var lift = new Mock<IEventLift>();
            var cabinetDevice = new Mock<ICabinetDevice>();
            var optionConfigDevice = new Mock<IOptionConfigDevice>();
            var progressiveDevice = new Mock<IProgressiveDevice>();

            egm.Setup(e => e.GetDevices<ICabinetDevice>()).Returns( new List<ICabinetDevice> { cabinetDevice.Object });
            cabinetDevice.SetupGet(d => d.DeviceClass).Returns("G2S_cabinet");

            egm.Setup(e => e.GetDevices<IOptionConfigDevice>()).Returns( new List<IOptionConfigDevice> { optionConfigDevice.Object });
            optionConfigDevice.SetupGet(d => d.DeviceClass).Returns("G2S_optionConfig");

            egm.Setup(e => e.GetDevices<IProgressiveDevice>()).Returns(new List<IProgressiveDevice> { progressiveDevice.Object });
            progressiveDevice.SetupGet(d => d.DeviceClass).Returns("G2S_progressive");

            var consumer = new OperatorMenuSettingsChangedConsumer(egm.Object, builder1.Object, builder2.Object, builder3.Object, lift.Object);

            consumer.Consume(new OperatorMenuSettingsChangedEvent());

            lift.Verify(
                l => l.Report(
                    It.Is<IDevice>(d => cabinetDevice.Object == d),
                    It.Is<string>(e => e == EventCode.G2S_CBE006),
                    It.IsAny<deviceList1>()));

            lift.Verify(
                l => l.Report(
                    It.Is<IDevice>(d => optionConfigDevice.Object == d),
                    It.Is<string>(e => e == EventCode.G2S_OCE004),
                    It.IsAny<deviceList1>()));

            lift.Verify(
                l => l.Report(
                    It.Is<IDevice>(d => progressiveDevice.Object == d),
                    It.Is<string>(e => e == EventCode.G2S_PGE006),
                    It.IsAny<deviceList1>()));
        }

    }
}
