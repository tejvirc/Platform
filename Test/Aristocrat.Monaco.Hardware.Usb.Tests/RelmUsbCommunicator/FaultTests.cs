namespace Aristocrat.Monaco.Hardware.Usb.Tests.RelmUsbCommunicator
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Common.Storage;
    using Aristocrat.Monaco.Test.Common;
    using Contracts;
    using Contracts.Reel;
    using Contracts.Reel.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using RelmReels.Communicator;
    using RelmReels.Communicator.InterruptHandling;
    using RelmReels.Messages;
    using RelmReels.Messages.Commands;
    using RelmReels.Messages.Interrupts;
    using RelmReels.Messages.Queries;
    using Usb.ReelController.Relm;
    using MonacoLightStatus = Contracts.Reel.LightStatus;
    using MonacoReelStatus = Contracts.Reel.ReelStatus;

    [TestClass]
    public class FaultTests
    {
        private readonly Mock<IRelmCommunicator> _driver = new();
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IFileSystemProvider> _fileSystem;
        private RelmUsbCommunicator _target;
        private Mock<IEventBus> _eventBus;

        [TestInitialize]
        public void Initialize()
        {
            _driver.Setup(x => x.IsOpen).Returns(true);
            _driver.Setup(x => x.Configuration).Returns(new DeviceConfiguration());
            _driver.Setup(x => x.SendQueryAsync<DeviceConfiguration>(default)).ReturnsAsync(new RelmResponse<DeviceConfiguration>(true, new DeviceConfiguration()));
            _driver.Setup(x => x.SendQueryAsync<FirmwareSize>(default)).ReturnsAsync(new RelmResponse<FirmwareSize>(true, new FirmwareSize()));
            _driver.Setup(x => x.SendQueryAsync<DeviceStatuses>(default)).ReturnsAsync(new RelmResponse<DeviceStatuses>(true, new DeviceStatuses()
            {
                LightStatuses = Enumerable
                    .Range(0, 6)
                    .Select(x => new DeviceStatus<RelmReels.Messages.LightStatus>()
                    {
                        Id = (byte)x,
                        Status = RelmReels.Messages.LightStatus.Functioning
                    })
                    .ToArray(),
                ReelStatuses = Enumerable
                    .Range(0, 5)
                    .Select(x => new DeviceStatus<RelmReels.Messages.ReelStatus>()
                    {
                        Id = (byte)x,
                        Status = RelmReels.Messages.ReelStatus.Idle
                    })
                    .ToArray()
            }));
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoNotResetRelmController, It.IsAny<bool>())).Returns(false);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Loose);
            _fileSystem = new Mock<IFileSystemProvider>();
            _target = new RelmUsbCommunicator(_driver.Object, _propertiesManager.Object, _fileSystem.Object, _eventBus.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public async Task PingTimeoutReceivedTest()
        {
            var controllerFaultOccurred = false;
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<HomeReels>(), default))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(new PingTimeout()));

            _target.ControllerFaultOccurred += delegate (object _, ReelControllerFaultedEventArgs e)
            {
                controllerFaultOccurred = e.Faults == ReelControllerFaults.CommunicationError;
            };

            await _target.Initialize();
            await _target.HomeReel(1, 0);

            Assert.IsTrue(controllerFaultOccurred);
        }

        [TestMethod]
        public async Task PingTimeoutClearedTest()
        {
            var controllerFaultCleared = false;
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<HomeReels>(), default))
                .Returns(Task.FromResult(true))
                .Raises(x => x.PingTimeoutCleared += null, EventArgs.Empty);

            _target.ControllerFaultCleared += delegate (object _, ReelControllerFaultedEventArgs e)
            {
                controllerFaultCleared = e.Faults == ReelControllerFaults.CommunicationError;
            };

            await _target.Initialize();
            await _target.HomeReel(1, 0);

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

            _target.ReelStatusReceived += delegate (object _, ReelStatusReceivedEventArgs e)
            {
                actualReelStatus = e.Statuses.First();
            };

            await _target.Initialize();
            await _target.HomeReel(1, 0);

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

            _target.LightStatusReceived += delegate (object _, LightEventArgs e)
            {
                actualLightStatus = e.Statuses.First();
            };

            await _target.Initialize();
            await _target.HomeReel(1, 0);

            Assert.IsNotNull(actualLightStatus);
            Assert.AreEqual((int)lightId, actualLightStatus.LightId);
            Assert.AreEqual(isFaulted, actualLightStatus.Faulted);
        }
    }
}
