namespace Aristocrat.Monaco.Hardware.Usb.Tests.RelmUsbCommunicator
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Common.Storage;
    using Aristocrat.Monaco.Test.Common;
    using Contracts;
    using Contracts.Reel;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using RelmReels;
    using RelmReels.Communicator;
    using RelmReels.Communicator.Downloads;
    using RelmReels.Communicator.InterruptHandling;
    using RelmReels.Messages;
    using RelmReels.Messages.Commands;
    using RelmReels.Messages.Interrupts;
    using RelmReels.Messages.Queries;
    using Usb.ReelController.Relm;
    using MonacoAnimationPreparedStatus = Contracts.Reel.AnimationPreparedStatus;
    using RelmAnimationPreparedStatus = RelmReels.Messages.AnimationPreparedStatus;

    [TestClass]
    public class InterruptTests
    {
        private const string AnimationName = "anim";
        private const string Tag = "ALL";
        private readonly Mock<IRelmCommunicator> _driver = new();
        private readonly AnimationFile _testLightShowFile = new("anim.lightshow", AnimationType.PlatformLightShow, AnimationName);
        private readonly StoredFile _storedFile = new("", 12345, 1, true);
        private readonly LightShowData _lightShowData = new(3, AnimationName, Tag, ReelConstants.RepeatOnce, -1);
        private readonly AnimationFile _testStepperCurveFile = new("anim.stepper", AnimationType.PlatformStepperCurve, AnimationName);
        private readonly ReelCurveData _curveData = new(3, AnimationName);
        private readonly uint _tagId = Tag.HashDjb2();

        private RelmUsbCommunicator _target;
        private Mock<IFileSystemProvider> _fileSystem;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoNotResetRelmController, It.IsAny<bool>())).Returns(false);

            _driver.Setup(x => x.IsOpen).Returns(true);
            _driver.Setup(x => x.Configuration).Returns(new DeviceConfiguration());
            _driver.Setup(x => x.SendQueryAsync<StoredAnimationIds>(default)).ReturnsAsync(new RelmResponse<StoredAnimationIds>(true, new StoredAnimationIds()));
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
            _driver.Setup(x => x.Download(_testStepperCurveFile.Path, BitmapVerification.CRC32, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedFile));
            _driver.Setup(x => x.Download(_testLightShowFile.Path, BitmapVerification.CRC32, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedFile));

            _fileSystem = new Mock<IFileSystemProvider>();
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Loose);
            _target = new RelmUsbCommunicator(_driver.Object, _propertiesManager.Object, _fileSystem.Object, _eventBus.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }
        
        [DataTestMethod]
        [DataRow(RelmAnimationPreparedStatus.AnimationIsIncompatibleWithTheCurrentReelControllerState, MonacoAnimationPreparedStatus.IncompatibleState)]
        [DataRow(RelmAnimationPreparedStatus.AnimationQueueFull, MonacoAnimationPreparedStatus.QueueFull)]
        [DataRow(RelmAnimationPreparedStatus.AnimationSuccessfullyPrepared, MonacoAnimationPreparedStatus.Prepared)]
        [DataRow(RelmAnimationPreparedStatus.FileCorrupt, MonacoAnimationPreparedStatus.FileCorrupt)]
        [DataRow(RelmAnimationPreparedStatus.ShowDoesNotExist, MonacoAnimationPreparedStatus.DoesNotExist)]
        public async Task LightShowAnimationsPreparedInterruptTest(RelmAnimationPreparedStatus actualStatus, MonacoAnimationPreparedStatus expectedStatus)
        {
            var interrupt = new LightShowAnimationsPrepared {
                Animations = new[]
                {
                    new LightShowAnimationPreparedData { AnimationId = _storedFile.FileId, TagId = _tagId, PreparedStatus = actualStatus }
                }
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareLightShowAnimations>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            LightAnimationEventArgs actualArgs = null; 
            _target.LightAnimationPrepared += (_, args) =>
            {
                actualArgs = args;
            };

            await InitializeAndLoadAnimation(_testLightShowFile);
            await _target.PrepareAnimation(_lightShowData);

            Assert.AreEqual(actualArgs?.AnimationName, AnimationName);
            Assert.AreEqual(actualArgs?.Tag, Tag);
            Assert.AreEqual(actualArgs?.PreparedStatus, expectedStatus);
            Assert.AreEqual(actualArgs?.QueueType, AnimationQueueType.Unknown);
        }
        
        [TestMethod]
        public async Task LightShowAnimationStartedInterruptTest()
        {
            var interrupt = new LightShowAnimationStarted
            {
                AnimationId = _storedFile.FileId,
                TagId = _tagId
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareLightShowAnimations>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            LightAnimationEventArgs actualArgs = null; 
            _target.LightAnimationStarted += (_, args) =>
            {
                actualArgs = args;
            };
            
            await InitializeAndLoadAnimation(_testLightShowFile);
            await _target.PrepareAnimation(_lightShowData);

            Assert.AreEqual(actualArgs?.AnimationName, AnimationName);
            Assert.AreEqual(actualArgs?.Tag, Tag);
            Assert.AreEqual(actualArgs?.PreparedStatus, MonacoAnimationPreparedStatus.Unknown);
            Assert.AreEqual(actualArgs?.QueueType, AnimationQueueType.Unknown);
        }

        [DataTestMethod]
        [DataRow(LightShowAnimationQueueLocation.NotInTheQueues, AnimationQueueType.NotInQueues)]
        [DataRow(LightShowAnimationQueueLocation.RemovedFromPlayingQueue, AnimationQueueType.PlayingQueue)]
        [DataRow(LightShowAnimationQueueLocation.RemovedFromPlayAndWaitQueues, AnimationQueueType.PlayAndWaitQueues)]
        [DataRow(LightShowAnimationQueueLocation.RemovedFromPlayBecauseAnimationEnded, AnimationQueueType.AnimationEnded)]
        [DataRow(LightShowAnimationQueueLocation.RemovedFromWaitingQueue, AnimationQueueType.WaitingQueue)]
        public async Task LightShowAnimationStoppedInterruptTest(LightShowAnimationQueueLocation actualQueueType, AnimationQueueType expectedQueueType)
        {
            var interrupt = new LightShowAnimationStopped
            {
                AnimationId = _storedFile.FileId,
                TagId = _tagId,
                LightShowAnimationQueueLocation = actualQueueType
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareLightShowAnimations>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            LightAnimationEventArgs actualArgs = null; 
            _target.LightAnimationStopped += (_, args) =>
            {
                actualArgs = args;
            };

            await InitializeAndLoadAnimation(_testLightShowFile);
            await _target.PrepareAnimation(_lightShowData);

            Assert.AreEqual(actualArgs?.AnimationName, AnimationName);
            Assert.AreEqual(actualArgs?.Tag, Tag);
            Assert.AreEqual(actualArgs?.PreparedStatus, MonacoAnimationPreparedStatus.Unknown);
            Assert.AreEqual(actualArgs?.QueueType, expectedQueueType);
        }

        [DataTestMethod]
        [DataRow(LightShowAnimationQueueLocation.NotInTheQueues, AnimationQueueType.NotInQueues)]
        [DataRow(LightShowAnimationQueueLocation.RemovedFromPlayingQueue, AnimationQueueType.PlayingQueue)]
        [DataRow(LightShowAnimationQueueLocation.RemovedFromPlayAndWaitQueues, AnimationQueueType.PlayAndWaitQueues)]
        [DataRow(LightShowAnimationQueueLocation.RemovedFromPlayBecauseAnimationEnded, AnimationQueueType.AnimationEnded)]
        [DataRow(LightShowAnimationQueueLocation.RemovedFromWaitingQueue, AnimationQueueType.WaitingQueue)]
        public async Task LightShowAnimationRemovedInterruptTest(LightShowAnimationQueueLocation actualQueueType, AnimationQueueType expectedQueueType)
        {
            var interrupt = new LightShowAnimationRemoved {
                AnimationId = _storedFile.FileId,
                QueueLocation = actualQueueType
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareLightShowAnimations>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            LightAnimationEventArgs actualArgs = null;
            _target.LightAnimationRemoved += (_, args) =>
            {
                actualArgs = args;
            };

            await InitializeAndLoadAnimation(_testLightShowFile);
            await _target.PrepareAnimation(_lightShowData);

            Assert.AreEqual(actualArgs?.AnimationName, AnimationName);
            Assert.IsTrue(string.IsNullOrEmpty(actualArgs?.Tag));
            Assert.AreEqual(actualArgs?.PreparedStatus, MonacoAnimationPreparedStatus.Unknown);
            Assert.AreEqual(actualArgs?.QueueType, expectedQueueType);
        }

        [TestMethod]
        public async Task AllLightShowsClearedInterruptTest()
        {
            var interrupt = new AllLightShowsCleared();

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareLightShowAnimations>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            var eventFired = false;
            _target.AllLightAnimationsCleared += (_, _) =>
            {
                eventFired = true;
            };

            await InitializeAndLoadAnimation(_testLightShowFile);
            await _target.PrepareAnimation(_lightShowData);

            Assert.IsTrue(eventFired);
        }

        [DataTestMethod]
        [DataRow(ReelPreparedStatus.AnimationIsIncompatibleWithTheCurrentControllerState, MonacoAnimationPreparedStatus.IncompatibleState)]
        [DataRow(ReelPreparedStatus.AnimationQueueFull, MonacoAnimationPreparedStatus.QueueFull)]
        [DataRow(ReelPreparedStatus.AnimationSuccessfullyPrepared, MonacoAnimationPreparedStatus.Prepared)]
        [DataRow(ReelPreparedStatus.FileCorrupt, MonacoAnimationPreparedStatus.FileCorrupt)]
        [DataRow(ReelPreparedStatus.ShowDoesNotExist, MonacoAnimationPreparedStatus.DoesNotExist)]
        public async Task ReelAnimationsPreparedInterruptTest(ReelPreparedStatus actualStatus, MonacoAnimationPreparedStatus expectedStatus)
        {
            var interrupt = new ReelAnimationsPrepared
            {
                Animations = new[]
                {
                    new ReelAnimationPreparedData { AnimationId = _storedFile.FileId, PreparedStatus = actualStatus }
                }
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperCurves>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            ReelAnimationEventArgs actualArgs = null;
            _target.ReelAnimationPrepared += (_, args) =>
            {
                actualArgs = args;
            };

            await InitializeAndLoadAnimation(_testStepperCurveFile);
            await _target.PrepareAnimation(_curveData);

            Assert.AreEqual(actualArgs?.ReelId, 0);
            Assert.AreEqual(actualArgs?.AnimationName, AnimationName);
            Assert.AreEqual(actualArgs?.PreparedStatus, expectedStatus);
        }

        [TestMethod]
        public async Task ReelPlayingAnimationInterruptTest()
        {
            var interrupt = new ReelPlayingAnimation
            {
                AnimationId = _storedFile.FileId,
                ReelIndex = _curveData.ReelIndex
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperCurves>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            ReelAnimationEventArgs actualArgs = null;
            _target.ReelAnimationStarted += (_, args) =>
            {
                actualArgs = args;
            };

            await InitializeAndLoadAnimation(_testStepperCurveFile);
            await _target.PrepareAnimation(_curveData);

            Assert.AreEqual(actualArgs?.ReelId, _curveData.ReelIndex);
            Assert.AreEqual(actualArgs?.AnimationName, AnimationName);
            Assert.AreEqual(actualArgs?.PreparedStatus, MonacoAnimationPreparedStatus.Unknown);
        }

        [TestMethod]
        public async Task ReelFinishedAnimationInterruptTest()
        {
            var interrupt = new ReelFinishedAnimation
            {
                AnimationId = _storedFile.FileId,
                ReelIndex = _curveData.ReelIndex
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperCurves>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            ReelAnimationEventArgs actualArgs = null;
            _target.ReelAnimationStopped += (_, args) =>
            {
                actualArgs = args;
            };

            await InitializeAndLoadAnimation(_testStepperCurveFile);
            await _target.PrepareAnimation(_curveData);

            Assert.AreEqual(actualArgs?.ReelId, _curveData.ReelIndex);
            Assert.AreEqual(actualArgs?.AnimationName, AnimationName);
            Assert.AreEqual(actualArgs?.PreparedStatus, MonacoAnimationPreparedStatus.Unknown);
        }

        [TestMethod]
        public async Task ReelIdleTimeCalculatedInterruptTest()
        {
            const int reelId = 3;
            const int stopTime = 100;

            var interrupt = new ReelIdleTimeCalculated
            {
                ReelIndex = reelId,
                StopTime = stopTime
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperCurves>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            ReelStoppingEventArgs actualArgs = null;
            _target.ReelStopping += (_, args) =>
            {
                actualArgs = args;
            };

            await InitializeAndLoadAnimation(_testStepperCurveFile);
            await _target.PrepareAnimation(_curveData);

            Assert.AreEqual(actualArgs?.ReelId, reelId);
            Assert.AreEqual(actualArgs?.TimeToStop, stopTime);
        }

        [TestMethod]
        public async Task UserSpecifiedInterruptTest()
        {
            const int reelId = 3;
            const int eventId = 100;

            var interrupt = new UserSpecifiedInterrupt
            {
                ReelIndex = reelId,
                EventId = eventId
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperCurves>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            StepperRuleTriggeredEventArgs actualArgs = null;
            _target.StepperRuleTriggered += (_, args) =>
            {
                actualArgs = args;
            };

            await InitializeAndLoadAnimation(_testStepperCurveFile);
            await _target.PrepareAnimation(_curveData);

            Assert.AreEqual(actualArgs?.ReelId, reelId);
            Assert.AreEqual(actualArgs?.EventId, eventId);
        }

        [TestMethod]
        public async Task ReelSyncStartedInterruptTest()
        {
            const int reelId = 3;

            var interrupt = new ReelSyncStarted
            {
                ReelIndex = reelId
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperCurves>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            ReelSynchronizationEventArgs actualArgs = null;
            _target.SynchronizationStarted += (_, args) =>
            {
                actualArgs = args;
            };

            await InitializeAndLoadAnimation(_testStepperCurveFile);
            await _target.PrepareAnimation(_curveData);

            Assert.AreEqual(actualArgs?.ReelId, reelId);
        }

        [TestMethod]
        public async Task ReelSynchronizedInterruptTest()
        {
            const int reelId = 3;

            var interrupt = new ReelSynchronized
            {
                ReelIndex = reelId
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperCurves>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            ReelSynchronizationEventArgs actualArgs = null;
            _target.SynchronizationCompleted += (_, args) =>
            {
                actualArgs = args;
            };

            await InitializeAndLoadAnimation(_testStepperCurveFile);
            await _target.PrepareAnimation(_curveData);

            Assert.AreEqual(actualArgs?.ReelId, reelId);
        }

        private async Task InitializeAndLoadAnimation(AnimationFile animationFile) {
            await _target.Initialize();
            await _target.LoadAnimationFile(animationFile);
        }
    }
}
