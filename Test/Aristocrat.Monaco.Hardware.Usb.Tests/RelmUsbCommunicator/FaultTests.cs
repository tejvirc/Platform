namespace Aristocrat.Monaco.Hardware.Usb.Tests.RelmUsbCommunicator
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts.Reel;
    using Contracts.Reel.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using RelmReels.Communicator;
    using RelmReels.Communicator.InterruptHandling;
    using RelmReels.Messages;
    using RelmReels.Messages.Commands;
    using RelmReels.Messages.Interrupts;
    using RelmReels.Messages.Queries;
    using Usb.ReelController.Relm;
    using MonacoReelStatus = Contracts.Reel.ReelStatus;
    using MonacoLightStatus = Contracts.Reel.LightStatus;

    [TestClass]
    public class FaultTests
    {
        private readonly Mock<IRelmCommunicator> _driver = new();

        [TestInitialize]
        public void Initialize()
        {
            _driver.Setup(x => x.IsOpen).Returns(true);
            _driver.Setup(x => x.Configuration).Returns(new DeviceConfiguration());
            _driver.Setup(x => x.SendQueryAsync<DeviceConfiguration>(default)).ReturnsAsync(new DeviceConfiguration());
            _driver.Setup(x => x.SendQueryAsync<FirmwareSize>(default)).ReturnsAsync(new FirmwareSize());
            _driver.Setup(x => x.SendQueryAsync<DeviceStatuses>(default)).ReturnsAsync(new DeviceStatuses());
        }

        [TestMethod]
        public async Task PingTimeoutReceivedTest()
        {
            var controllerFaultOccurred = false;
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<HomeReels>(), default))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(new PingTimeout()));

            var usbCommunicator = new RelmUsbCommunicator(_driver.Object);
            usbCommunicator.ControllerFaultOccurred += delegate(object _, ReelControllerFaultedEventArgs e)
            {
                controllerFaultOccurred = e.Faults == ReelControllerFaults.CommunicationError;
            };

            await usbCommunicator.Initialize();

            Assert.IsTrue(controllerFaultOccurred);
        }

        [TestMethod]
        public async Task PingTimeoutClearedTest()
        {
            var controllerFaultCleared = false;
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<HomeReels>(), default))
                .Returns(Task.FromResult(true))
                .Raises(x => x.PingTimeoutCleared += null, EventArgs.Empty);

            var usbCommunicator = new RelmUsbCommunicator(_driver.Object);
            usbCommunicator.ControllerFaultCleared += delegate(object _, ReelControllerFaultedEventArgs e)
            {
                controllerFaultCleared = e.Faults == ReelControllerFaults.CommunicationError;
            };

            await usbCommunicator.Initialize();

            Assert.IsTrue(controllerFaultCleared);
        }

        [TestMethod]
        [DataRow(typeof(ReelDisconnected), false, false, false, false, false, false)]
        [DataRow(typeof(ReelTamperingDetected), true, true, false, false, false, false)]
        [DataRow(typeof(ReelStalled), true, false, true, false, false, false)]
        [DataRow(typeof(ReelOpticSequenceError), true, false, false, true, false, false)]
        [DataRow(typeof(ReelIdleUnknown), true, false, false, false, true, false)]
        [DataRow(typeof(ReelUnknownStopReceived), true, false, false, false, false, true)]
        public async Task OnInterruptReceivedTest(
            Type interruptType,
            bool connected,
            bool tampered,
            bool stalled,
            bool opticError,
            bool idleUnknown,
            bool unknownStop)
        {
            var actualReelStatus = new MonacoReelStatus();
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<HomeReels>(), default))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(Activator.CreateInstance(interruptType)));

            var usbCommunicator = new RelmUsbCommunicator(_driver.Object);
            usbCommunicator.ReelStatusReceived += delegate(object _, ReelStatusReceivedEventArgs e)
            {
                actualReelStatus = e.Statuses.First();
            };

            await usbCommunicator.Initialize();

            Assert.AreEqual(connected, actualReelStatus.Connected);
            Assert.AreEqual(tampered, actualReelStatus.ReelTampered);
            Assert.AreEqual(stalled, actualReelStatus.ReelStall);
            Assert.AreEqual(opticError, actualReelStatus.OpticSequenceError);
            Assert.AreEqual(idleUnknown, actualReelStatus.IdleUnknown);
            Assert.AreEqual(unknownStop, actualReelStatus.UnknownStop);
        }

        [TestMethod]
        [DataRow(LightId.InBetween, true)]
        [DataRow(LightId.Mood, true)]
        [DataRow(LightId.Reel1, true)]
        [DataRow(LightId.Reel2, true)]
        [DataRow(LightId.Reel3, true)]
        [DataRow(LightId.Reel4, true)]
        [DataRow(LightId.Reel5, true)]
        [DataRow(LightId.InBetween, false)]
        [DataRow(LightId.Mood, false)]
        [DataRow(LightId.Reel1, false)]
        [DataRow(LightId.Reel2, false)]
        [DataRow(LightId.Reel3, false)]
        [DataRow(LightId.Reel4, false)]
        [DataRow(LightId.Reel5, false)]
        public async Task OnLightInterruptReceived(LightId lightId, bool isFaulted)
        {
            MonacoLightStatus actualLightStatus = null;
            object interrupt = isFaulted
                ? new LightMalfunction { LightId = lightId }
                : new LightFunctioning { LightId = lightId };

            if (isFaulted)
            {
                _driver.Setup(x => x.SendCommandAsync(It.IsAny<HomeReels>(), default))
                    .Returns(Task.FromResult(true))
                    .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));
            }
            else
            {
                _driver.Setup(x => x.SendCommandAsync(It.IsAny<HomeReels>(), default))
                    .Returns(Task.FromResult(true))
                    .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));
            }

            var usbCommunicator = new RelmUsbCommunicator(_driver.Object);
            usbCommunicator.LightStatusReceived += delegate(object _, LightEventArgs e)
            {
                actualLightStatus = e.Statuses.First();
            };
            
            await usbCommunicator.Initialize();

            Assert.IsNotNull(actualLightStatus);
            Assert.AreEqual((int)lightId, actualLightStatus.LightId);
            Assert.AreEqual(isFaulted, actualLightStatus.Faulted);
        }
    }
}
