namespace Aristocrat.Monaco.Hardware.Usb.Tests.ReelController
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.Reel;
    using Contracts.Reel.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Usb.ReelController.Relm;

    [TestClass]
    public class FaultAndStatusTests
    {
        private readonly Mock<IRelmCommunicator> _communicator = new();

        [TestInitialize]
        public void Initialize()
        {
            _communicator.Setup(x => x.IsOpen).Returns(true);
        }

        [DataTestMethod]
        [DataRow(ReelControllerFaults.CommunicationError)]
        [DataRow(ReelControllerFaults.HardwareError)]
        [DataRow(ReelControllerFaults.LightError)]
        public async Task ControllerFaultOccurredTest(ReelControllerFaults faults)
        {
            _communicator.Setup(x => x.HomeReel(1, 1, true))
                .Returns(Task.FromResult(true))
                .Raises(x => x.ControllerFaultOccurred += null, new ReelControllerFaultedEventArgs(faults));

            _communicator.Setup(x => x.HomeReel(2, 2, true))
                .Returns(Task.FromResult(true))
                .Raises(x => x.ControllerFaultCleared += null, new ReelControllerFaultedEventArgs(faults));

            var controller = new RelmReelController();
            await controller.Initialize(_communicator.Object);

            await controller.HomeReel(1, 1);
            Assert.AreEqual(faults, controller.ReelControllerFaults);

            await controller.HomeReel(2, 2);
            Assert.AreEqual(ReelControllerFaults.None, controller.ReelControllerFaults);
        }

        [TestMethod]
        public async Task ReelStatusConnectedTest()
        {
            var reelId = 1;
            var reelConnected = false;
            var inFault = false;
            _communicator.Setup(x => x.HomeReel(1, 1, true))
                .Returns(Task.FromResult(true))
                .Raises(x => x.ReelStatusReceived += null, new ReelStatusReceivedEventArgs(new ReelStatus { Connected = true, ReelId = reelId }));

            _communicator.Setup(x => x.HomeReel(2, 2, true))
                .Returns(Task.FromResult(true))
                .Raises(x => x.ReelStatusReceived += null, new ReelStatusReceivedEventArgs(new ReelStatus { Connected = false, IdleUnknown = true, ReelId = reelId }));

            var controller = new RelmReelController();
            controller.ReelConnected += delegate { reelConnected = true; };
            controller.ReelDisconnected += delegate { reelConnected = false; };
            controller.FaultOccurred += delegate { inFault = true; };
            controller.FaultCleared += delegate { inFault = false; };
            await controller.Initialize(_communicator.Object);

            await controller.HomeReel(1, 1);
            Assert.IsTrue(reelConnected);
            Assert.IsFalse(inFault);
            Assert.AreEqual(reelId, controller.ReelStatuses.First().Value.ReelId);
            Assert.AreEqual(reelConnected, controller.ReelStatuses.First().Value.Connected);
            Assert.IsFalse(controller.ReelStatuses.First().Value.IdleUnknown);

            await controller.HomeReel(2, 2);
            Assert.IsFalse(reelConnected);
            Assert.IsTrue(inFault);
            Assert.AreEqual(reelId, controller.ReelStatuses.First().Value.ReelId);
            Assert.AreEqual(reelConnected, controller.ReelStatuses.First().Value.Connected);
            Assert.IsTrue(controller.ReelStatuses.First().Value.IdleUnknown);

            await controller.HomeReel(1, 1);
            Assert.IsTrue(reelConnected);
            Assert.IsFalse(inFault);
            Assert.AreEqual(reelId, controller.ReelStatuses.First().Value.ReelId);
            Assert.AreEqual(reelConnected, controller.ReelStatuses.First().Value.Connected);
            Assert.IsFalse(controller.ReelStatuses.First().Value.IdleUnknown);
        }

        [TestMethod]
        public async Task LightStatusTest()
        {
            var inFault = false;
            _communicator.Setup(x => x.HomeReel(1, 1, true))
                .Returns(Task.FromResult(true))
                .Raises(x => x.LightStatusReceived += null, new LightEventArgs(new List<LightStatus> { new(1, true) }));

            _communicator.Setup(x => x.HomeReel(2, 2, true))
                .Returns(Task.FromResult(true))
                .Raises(x => x.LightStatusReceived += null, new LightEventArgs(new List<LightStatus> { new(1, false) }));

            var controller = new RelmReelController();
            controller.ControllerFaultOccurred += delegate { inFault = true; };
            controller.ControllerFaultCleared += delegate { inFault = false; };
            await controller.Initialize(_communicator.Object);

            await controller.HomeReel(1, 1);
            Assert.IsTrue(inFault);

            await controller.HomeReel(2, 2);
            Assert.IsFalse(inFault);
        }
    }
}