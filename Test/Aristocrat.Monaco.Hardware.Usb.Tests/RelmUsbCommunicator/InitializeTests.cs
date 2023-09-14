namespace Aristocrat.Monaco.Hardware.Usb.Tests.RelmUsbCommunicator
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Test.Common;
    using Contracts;
    using Contracts.Reel.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using RelmReels.Communicator;
    using RelmReels.Messages;
    using RelmReels.Messages.Queries;
    using Usb.ReelController.Relm;
    using MonacoLightStatus = Contracts.Reel.LightStatus;
    using MonacoReelStatus = Contracts.Reel.ReelStatus;

    [TestClass]
    public class InitializeTests
    {
        private readonly Mock<IRelmCommunicator> _driver = new();
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;

        [TestInitialize]
        public void Initialize()
        {
            _driver.Setup(x => x.IsOpen).Returns(true);
            _driver.Setup(x => x.Configuration).Returns(new DeviceConfiguration());
            _driver.Setup(x => x.SendQueryAsync<DeviceConfiguration>(default)).ReturnsAsync(new RelmResponse<DeviceConfiguration>(true, new DeviceConfiguration()));
            _driver.Setup(x => x.SendQueryAsync<FirmwareSize>(default)).ReturnsAsync(new RelmResponse<FirmwareSize>(true, new FirmwareSize()));
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoNotResetRelmController, It.IsAny<bool>())).Returns(false);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Loose);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public async Task InitializeTest()
        {
            var lightStatusesReceived = false;
            var reelStatusesReceived = false;

            var usbCommunicator = new RelmUsbCommunicator(_driver.Object, _propertiesManager.Object, _eventBus.Object);
            usbCommunicator.LightStatusReceived += delegate { lightStatusesReceived = true; };
            usbCommunicator.ReelStatusReceived += delegate { reelStatusesReceived = true; };

            _driver.Setup(x => x.SendQueryAsync<DeviceStatuses>(default)).ReturnsAsync(new RelmResponse<DeviceStatuses>(true, new DeviceStatuses()
            {
                LightStatuses = Enumerable
                    .Range(0, 6)
                    .Select(x => new DeviceStatus<LightStatus>()
                    {
                        Id = (byte)x,
                        Status = LightStatus.Functioning
                    })
                    .ToArray(),
                ReelStatuses = Enumerable
                    .Range(0, 5)
                    .Select(x => new DeviceStatus<ReelStatus>()
                    {
                        Id = (byte)x,
                        Status = ReelStatus.Idle
                    })
                    .ToArray()
            }));

            await usbCommunicator.Initialize();

            Assert.IsTrue(lightStatusesReceived);
            Assert.IsTrue(reelStatusesReceived);
        }

        [DataTestMethod]
        [DataRow(2, LightStatus.Failure, true)]
        [DataRow(2, LightStatus.Functioning, false)]
        [DataRow(2, LightStatus.None, false)]
        public async Task InitializeLightStatusesTest(int statusCount, LightStatus lightStatus, bool expectedFaulted)
        {
            var actualLightStatuses = new List<MonacoLightStatus>();
            var lightStatuses = new List<DeviceStatus<LightStatus>>();
            for (byte i = 0; i < statusCount; i++)
            {
                lightStatuses.Add(new DeviceStatus<LightStatus> { Id = i, Status = lightStatus });
            }

            var expectedDeviceStatuses = new DeviceStatuses
            {
                LightStatuses = lightStatuses.ToArray(),
                LightCount = (byte)statusCount
            };

            var usbCommunicator = new RelmUsbCommunicator(_driver.Object, _propertiesManager.Object, _eventBus.Object);
            usbCommunicator.LightStatusReceived += delegate (object _, LightEventArgs e) { actualLightStatuses = e.Statuses.ToList(); };
            _driver.Setup(x => x.SendQueryAsync<DeviceStatuses>(default)).ReturnsAsync(new RelmResponse<DeviceStatuses>(true, expectedDeviceStatuses));

            await usbCommunicator.Initialize();

            for (var i = 0; i < expectedDeviceStatuses.LightStatuses.Length; i++)
            {
                Assert.AreEqual(expectedDeviceStatuses.LightStatuses[i].Id, actualLightStatuses[i].LightId);
                Assert.AreEqual(expectedFaulted, actualLightStatuses[i].Faulted);
            } 
        }

        [DataTestMethod]
        [DataRow(2, ReelStatus.Disconnected, false, false, false, false, false, false)]
        [DataRow(1, ReelStatus.TamperingDetected, true, true, false, false, false, false)]
        [DataRow(1, ReelStatus.Stalled, true, false, true, false, false, false)]
        [DataRow(1, ReelStatus.OpticSequenceError, true, false, false, true, false, false)]
        [DataRow(1, ReelStatus.IdleUnknown, true, false, false, false, true, false)]
        [DataRow(1, ReelStatus.UnknownStopReceived, true, false, false, false, false, true)]
        public async Task InitializeReelStatusesTest(
            int statusCount,
            ReelStatus reelStatus,
            bool connected,
            bool tampered,
            bool stalled,
            bool opticError,
            bool idleUnknown,
            bool unknownStop)
        {
            var actualReelStatuses = new List<MonacoReelStatus>();
            var reelStatuses = new List<DeviceStatus<ReelStatus>>();
            for (byte i = 0; i < statusCount; i++)
            {
                reelStatuses.Add(new DeviceStatus<ReelStatus> { Id = i, Status = reelStatus });
            }

            var expectedDeviceStatuses = new DeviceStatuses
            {
                ReelStatuses = reelStatuses.ToArray(),
                ReelCount = (byte)statusCount
            };

            var expectedReelStatus = new MonacoReelStatus
            {
                Connected = connected,
                ReelTampered = tampered,
                ReelStall = stalled,
                OpticSequenceError = opticError,
                IdleUnknown = idleUnknown,
                UnknownStop = unknownStop
            };

            var usbCommunicator = new RelmUsbCommunicator(_driver.Object, _propertiesManager.Object, _eventBus.Object);
            usbCommunicator.ReelStatusReceived += delegate(object _, ReelStatusReceivedEventArgs e) { actualReelStatuses = e.Statuses.ToList(); };
            _driver.Setup(x => x.SendQueryAsync<DeviceStatuses>(default)).ReturnsAsync(new RelmResponse<DeviceStatuses>(true, expectedDeviceStatuses));

            await usbCommunicator.Initialize();

            for (var i = 0; i < expectedDeviceStatuses.ReelStatuses.Length; i++)
            {
                Assert.AreEqual(expectedDeviceStatuses.ReelStatuses[i].Id + 1, actualReelStatuses[i].ReelId);
                Assert.AreEqual(expectedReelStatus.Connected, actualReelStatuses[i].Connected);
                Assert.AreEqual(expectedReelStatus.ReelTampered, actualReelStatuses[i].ReelTampered);
                Assert.AreEqual(expectedReelStatus.OpticSequenceError, actualReelStatuses[i].OpticSequenceError);
                Assert.AreEqual(expectedReelStatus.IdleUnknown, actualReelStatuses[i].IdleUnknown);
                Assert.AreEqual(expectedReelStatus.UnknownStop, actualReelStatuses[i].UnknownStop);
            }
        }
    }
}
