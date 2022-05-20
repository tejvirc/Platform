namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Handlers;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using G2S.Consumers;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ReelConsumerTest
    {
        Mock<IG2SEgm> _egm = new Mock<IG2SEgm>();
        Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>> _builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
        Mock<IEventLift>  _lift = new Mock<IEventLift>();
        Mock<ICabinetDevice> _device = new Mock<ICabinetDevice>();

        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _device.SetupGet(d => d.DeviceClass).Returns("cabinet");
            _egm.Setup(e => e.GetDevice<ICabinetDevice>()).Returns(_device.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void WhenReelControllerDisconnectAndConnectExpectReconnect()
        {
            var addConsumer = new ReelControllerConnectedConsumer(_egm.Object, _builder.Object, _lift.Object);
            var removeConsumer = new ReelControllerDisconnectedConsumer(_egm.Object, _builder.Object, _lift.Object);

            addConsumer.Consume(new ConnectedEvent());
            removeConsumer.Consume(new DisconnectedEvent());

            _device.Verify(d => d.AddCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelControllerDisconnected), Times.Once);
            _device.Verify(d => d.RemoveCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelControllerDisconnected), Times.Once);
        }

        [TestMethod]
        public void WhenReelControllerDisabledAndEnabledExpectReenable()
        {
            var addConsumer = new ReelControllerDisabledConsumer(_egm.Object, _builder.Object, _lift.Object);
            var removeConsumer = new ReelControllerEnabledConsumer(_egm.Object, _builder.Object, _lift.Object);

            addConsumer.Consume(new DisabledEvent(DisabledReasons.Operator));
            removeConsumer.Consume(new EnabledEvent(EnabledReasons.Operator));

            _device.Verify(d => d.AddCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelControllerDisabled), Times.Once);
            _device.Verify(d => d.RemoveCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelControllerDisabled), Times.Once);
        }

        [TestMethod]
        public void WhenReelControllerFaultAndClearExpectClear()
        {
            var addConsumer = new ReelControllerHardwareFaultConsumer(_egm.Object, _builder.Object, _lift.Object);
            var removeConsumer = new ReelControllerHardwareFaultClearConsumer(_egm.Object, _builder.Object, _lift.Object);

            addConsumer.Consume(new HardwareFaultEvent(ReelControllerFaults.LightError));
            removeConsumer.Consume(new HardwareFaultClearEvent(ReelControllerFaults.LightError));

            _device.Verify(d => d.AddCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelControllerFault), Times.Once);
            _device.Verify(d => d.RemoveCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelControllerFault), Times.Once);
        }

        [TestMethod]
        public void WhenReelControllerInspectFailAndSucceedExpectInspected()
        {
            var addConsumer = new ReelControllerInspectionFailedConsumer(_egm.Object, _builder.Object, _lift.Object);
            var removeConsumer = new ReelControllerInspectedConsumer(_egm.Object, _builder.Object, _lift.Object);

            addConsumer.Consume(new InspectionFailedEvent());
            removeConsumer.Consume(new InspectedEvent());

            _device.Verify(d => d.AddCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelControllerInspectionFailed), Times.Once);
            _device.Verify(d => d.RemoveCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelControllerInspectionFailed), Times.Once);
        }

        [TestMethod]
        public void WhenReelDisconnectAndConnectExpectReconnect()
        {
            int firstEventReel = 0;
            int secondEventReel = 0;

            var addConsumer = new ReelDisconnectedConsumer(_egm.Object, _builder.Object, _lift.Object);
            var removeConsumer = new ReelConnectedConsumer(_egm.Object, _builder.Object, _lift.Object);

            addConsumer.Consume(new ReelDisconnectedEvent(firstEventReel));
            removeConsumer.Consume(new ReelConnectedEvent(secondEventReel));

            _device.Verify(d => d.AddCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelDisconnected - firstEventReel), Times.Once);
            _device.Verify(d => d.RemoveCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelDisconnected - firstEventReel), Times.Once);
        }

        [TestMethod]
        public void WhenReelDisconnectAndConnectDifferExpectOnlyDisconnect()
        {
            int firstEventReel = 0;
            int secondEventReel = 1;

            var addConsumer = new ReelDisconnectedConsumer(_egm.Object, _builder.Object, _lift.Object);
            var removeConsumer = new ReelConnectedConsumer(_egm.Object, _builder.Object, _lift.Object);

            addConsumer.Consume(new ReelDisconnectedEvent(firstEventReel));
            removeConsumer.Consume(new ReelConnectedEvent(secondEventReel));

            _device.Verify(d => d.AddCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelDisconnected - firstEventReel), Times.Once);
            _device.Verify(d => d.RemoveCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelDisconnected - firstEventReel), Times.Never);
        }

        [TestMethod]
        public void WhenReelFaultAndClearExpectClear()
        {
            var firstFault = ReelFaults.ReelStall;
            var secondFault = ReelFaults.ReelStall;

            var addConsumer = new ReelFaultConsumer(_egm.Object, _builder.Object, _lift.Object);
            var removeConsumer = new ReelFaultClearConsumer(_egm.Object, _builder.Object, _lift.Object);

            addConsumer.Consume(new HardwareReelFaultEvent(firstFault));
            removeConsumer.Consume(new HardwareReelFaultClearEvent(secondFault));

            _device.Verify(d => d.AddCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelFault - (int)firstFault), Times.Once);
            _device.Verify(d => d.RemoveCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelFault - (int)firstFault), Times.Once);
        }

        [TestMethod]
        public void WhenReelFaultAndClearDifferExpectOnlyFault()
        {
            var firstFault = ReelFaults.ReelStall;
            var secondFault = ReelFaults.ReelTamper;

            var addConsumer = new ReelFaultConsumer(_egm.Object, _builder.Object, _lift.Object);
            var removeConsumer = new ReelFaultClearConsumer(_egm.Object, _builder.Object, _lift.Object);

            addConsumer.Consume(new HardwareReelFaultEvent(firstFault));
            removeConsumer.Consume(new HardwareReelFaultClearEvent(secondFault));

            _device.Verify(d => d.AddCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelFault - (int)firstFault), Times.Once);
            _device.Verify(d => d.RemoveCondition(_device.Object, EgmState.EgmDisabled, (int)CabinetFaults.ReelFault - (int)firstFault), Times.Never);
        }
    }
}