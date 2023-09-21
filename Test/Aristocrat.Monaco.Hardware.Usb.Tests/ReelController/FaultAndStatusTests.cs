namespace Aristocrat.Monaco.Hardware.Usb.Tests.ReelController
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Common;
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
            SetupMocks();
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
            var connectedCalled = false;
            var disconnectedCalled = false;
            var faultCalled = false;

            var reelStatus = new ReelStatus()
            {
                Connected = true,
                ReelId = 1
            };

            _communicator
                .Setup(x => x.HomeReel(1, 1, true))
                .Returns(Task.FromResult(true))
                .Raises(x => x.ReelStatusReceived += null, () => new ReelStatusReceivedEventArgs(reelStatus));

            _communicator.Setup(x => x.HomeReel(2, 2, true))
                .Returns(Task.FromResult(true))
                .Raises(x => x.ReelStatusReceived += null, () => new ReelStatusReceivedEventArgs(reelStatus));

            var controller = new RelmReelController();
            controller.ReelConnected += delegate { connectedCalled = true; };
            controller.ReelDisconnected += delegate { disconnectedCalled = true; };
            controller.FaultOccurred += delegate { faultCalled = true; };
            await controller.Initialize(_communicator.Object);

            ResetFlags();
            await controller.HomeReel(1, 1);
            Assert.IsTrue(connectedCalled);
            Assert.IsFalse(disconnectedCalled);
            Assert.IsFalse(faultCalled);
            Assert.AreEqual(reelStatus.ReelId, controller.ReelStatuses[reelStatus.ReelId].ReelId);
            Assert.AreEqual(reelStatus.Connected, controller.ReelStatuses[reelStatus.ReelId].Connected);
            Assert.AreEqual(reelStatus.IdleUnknown, controller.ReelStatuses[reelStatus.ReelId].IdleUnknown);

            reelStatus = new ReelStatus
            {
                Connected = false,
                IdleUnknown = true,
                ReelId = 2
            };

            ResetFlags();
            await controller.HomeReel(2, 2);
            Assert.IsFalse(connectedCalled);
            Assert.IsTrue(disconnectedCalled);
            Assert.IsTrue(faultCalled);
            Assert.AreEqual(reelStatus.ReelId, controller.ReelStatuses[reelStatus.ReelId].ReelId);
            Assert.AreEqual(reelStatus.Connected, controller.ReelStatuses[reelStatus.ReelId].Connected);
            Assert.AreEqual(reelStatus.IdleUnknown, controller.ReelStatuses[reelStatus.ReelId].IdleUnknown);

            reelStatus = new ReelStatus
            {
                Connected = false,
                ReelId = 1
            };

            ResetFlags();
            await controller.HomeReel(1, 1);
            Assert.IsFalse(connectedCalled);
            Assert.IsTrue(disconnectedCalled);
            Assert.IsFalse(faultCalled);
            Assert.AreEqual(reelStatus.ReelId, controller.ReelStatuses[reelStatus.ReelId].ReelId);
            Assert.AreEqual(reelStatus.Connected, controller.ReelStatuses[reelStatus.ReelId].Connected);
            Assert.AreEqual(reelStatus.IdleUnknown, controller.ReelStatuses[reelStatus.ReelId].IdleUnknown);

            reelStatus = new ReelStatus
            {
                Connected = true,
                ReelTampered = true,
                ReelId = 1
            };

            ResetFlags();
            await controller.HomeReel(1, 1);
            Assert.IsTrue(connectedCalled);
            Assert.IsFalse(disconnectedCalled);
            Assert.IsTrue(faultCalled);
            Assert.AreEqual(reelStatus.ReelId, controller.ReelStatuses[reelStatus.ReelId].ReelId);
            Assert.AreEqual(reelStatus.Connected, controller.ReelStatuses[reelStatus.ReelId].Connected);
            Assert.AreEqual(reelStatus.IdleUnknown, controller.ReelStatuses[reelStatus.ReelId].IdleUnknown);

            reelStatus = new ReelStatus
            {
                Connected = true,
                ReelTampered = false,
                ReelId = 1
            };

            ResetFlags();
            await controller.HomeReel(1, 1);
            Assert.IsFalse(connectedCalled);
            Assert.IsFalse(disconnectedCalled);
            Assert.IsFalse(faultCalled);
            Assert.AreEqual(reelStatus.ReelId, controller.ReelStatuses[reelStatus.ReelId].ReelId);
            Assert.AreEqual(reelStatus.Connected, controller.ReelStatuses[reelStatus.ReelId].Connected);
            Assert.AreEqual(reelStatus.IdleUnknown, controller.ReelStatuses[reelStatus.ReelId].IdleUnknown);


            void ResetFlags()
            {
                connectedCalled = false;
                disconnectedCalled = false;
                faultCalled = false;
            }
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

        private void SetupMocks()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            var disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Default);
            var localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Default);
            localizerFactory.Setup(x => x.For(It.IsAny<string>())).Returns(new Mock<ILocalizer>().Object);
            var localizer = new Mock<ILocalizer>();
            localizer.Setup(x => x.GetString(It.IsAny<string>())).Returns("empty");
            localizerFactory.Setup(x => x.For(It.IsAny<string>())).Returns(localizer.Object);
        }
    }
}