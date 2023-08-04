namespace Aristocrat.Monaco.Hardware.Usb.Tests.RelmUsbCommunicator
{
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Test.Common;
    using Contracts;
    using Contracts.Reel;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using RelmReels;
    using RelmReels.Communicator.Downloads;
    using RelmReels.Communicator.InterruptHandling;
    using RelmReels.Messages.Commands;
    using RelmReels.Messages.Interrupts;
    using RelmReels.Messages.Queries;
    using Usb.ReelController.Relm;

    [TestClass]
    public class AnimationEventsTests
    {
        private const string AnimationName = "anim";
        private const string Tag = "ALL";
        private readonly Mock<RelmReels.Communicator.IRelmCommunicator> _driver = new();
        private readonly Mock<IEventBus> _eventBus = new();
        private readonly RelmReelController _controller = new();
        private readonly AnimationFile _testLightShowFile = new("anim.lightshow", AnimationType.PlatformLightShow, AnimationName);
        private readonly StoredFile _storedFile = new("", 12345, 1);
        private readonly LightShowData _lightShowData = new(0, AnimationName, Tag, ReelConstants.RepeatOnce, -1);
        private readonly AnimationFile _testStepperCurveFile = new("anim.stepper", AnimationType.PlatformStepperCurve, AnimationName);
        private readonly ReelCurveData _curveData = new(0, AnimationName);
        private readonly uint _tagId = Tag.HashDjb2();
        private RelmUsbCommunicator _usbCommunicator;
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void Initialize()
        {
            _driver.Setup(x => x.IsOpen).Returns(true);
            _driver.Setup(x => x.Configuration).Returns(new DeviceConfiguration());
            _driver.Setup(x => x.SendQueryAsync<DeviceConfiguration>(default)).ReturnsAsync(new DeviceConfiguration());
            _driver.Setup(x => x.SendQueryAsync<FirmwareSize>(default)).ReturnsAsync(new FirmwareSize());
            _driver.Setup(x => x.SendQueryAsync<DeviceStatuses>(default)).ReturnsAsync(new DeviceStatuses());
            _driver.Setup(x => x.Download(_testStepperCurveFile.Path, RelmReels.Messages.BitmapVerification.CRC32, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedFile));
            _driver.Setup(x => x.Download(_testLightShowFile.Path, RelmReels.Messages.BitmapVerification.CRC32, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedFile));
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoNotResetRelmController, It.IsAny<bool>())).Returns(false);
            _usbCommunicator = new RelmUsbCommunicator(_driver.Object, _eventBus.Object, _propertiesManager.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public async Task PrepareLightShowInterruptPublishesCorrectEvent()
        {
            var interrupt = new LightShowAnimationsPrepared {
                Animations = new[]
                {
                    new LightShowAnimationPreparedData { AnimationId = _storedFile.FileId, TagId = _tagId }
                }
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareLightShowAnimations>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            InitializeAndLoadAnimation(_testLightShowFile);
            await _usbCommunicator.PrepareAnimation(_lightShowData);

            _eventBus.Verify(x => x.Publish(It.Is<LightShowAnimationUpdatedEvent>(
                evt => evt.AnimationName == _lightShowData.AnimationName &&
                        evt.Tag == _lightShowData.Tag &&
                        evt.State == AnimationState.Prepared)), Times.Once);
        }

        [TestMethod]
        public async Task PrepareLightShowInterruptNeverPublishesIfAnimationListIsNull()
        {
            var interrupt = new LightShowAnimationsPrepared();

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareLightShowAnimations>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            InitializeAndLoadAnimation(_testLightShowFile);
            await _usbCommunicator.PrepareAnimation(_lightShowData);

            _eventBus.Verify(x => x.Publish(It.Is<LightShowAnimationUpdatedEvent>(
                evt => evt.AnimationName == _lightShowData.AnimationName &&
                        evt.Tag == _lightShowData.Tag &&
                        evt.State == AnimationState.Prepared)), Times.Never);
        }

        [TestMethod]
        public async Task LightShowPlayingInterruptPublishesCorrectEvent()
        {
            var interrupt = new LightShowAnimationStarted {
                AnimationId = _storedFile.FileId,
                TagId = _tagId
            };

            SetupMocks();
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<StartAnimations>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            InitializeAndLoadAnimation(_testLightShowFile);
            await _usbCommunicator.PrepareAnimation(_lightShowData);
            await _usbCommunicator.PlayAnimations();

            _eventBus.Verify(x => x.Publish(It.Is<LightShowAnimationUpdatedEvent>(
                evt => evt.AnimationName == _lightShowData.AnimationName &&
                        evt.Tag == _lightShowData.Tag &&
                        evt.State == AnimationState.Started)), Times.Once);
        }

        [TestMethod]
        public async Task LightShowStoppedInterruptPublishesCorrectEvent()
        {
            var interrupt = new LightShowAnimationStopped() {
                AnimationId = _storedFile.FileId,
                TagId = _tagId
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<StopAllLightShowAnimations>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            InitializeAndLoadAnimation(_testLightShowFile);
            await _usbCommunicator.PrepareAnimation(_lightShowData);
            await _usbCommunicator.StopAllLightShows();

            _eventBus.Verify(x => x.Publish(It.Is<LightShowAnimationUpdatedEvent>(
                evt => evt.AnimationName == _lightShowData.AnimationName &&
                        evt.Tag == _lightShowData.Tag &&
                        evt.State == AnimationState.Stopped)), Times.Once);
        }

        [TestMethod]
        public async Task LightShowRemovedInterruptPublishesCorrectEvent()
        {
            var interrupt = new LightShowAnimationRemoved {
                AnimationId = _storedFile.FileId
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<StopAllLightShowAnimations>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            InitializeAndLoadAnimation(_testLightShowFile);
            await _usbCommunicator.PrepareAnimation(_lightShowData);
            await _usbCommunicator.StopAllLightShows();

            _eventBus.Verify(x => x.Publish(It.Is<LightShowAnimationUpdatedEvent>(
                evt => evt.AnimationName == _lightShowData.AnimationName &&
                        evt.Tag == string.Empty &&
                        evt.State == AnimationState.Removed)), Times.Once);
        }

        [TestMethod]
        public async Task PrepareStepperInterruptPublishesCorrectEvent()
        {
            var interrupt = new ReelAnimationsPrepared
            {
                Animations = new[]
                {
                    new ReelAnimationPreparedData { AnimationId = _storedFile.FileId }
                }
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperCurves>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            InitializeAndLoadAnimation(_testStepperCurveFile);
            await _usbCommunicator.PrepareAnimation(_curveData);

            _eventBus.Verify(x => x.Publish(It.Is<ReelAnimationUpdatedEvent>(
                evt => evt.AnimationName == _curveData.AnimationName &&
                        evt.ReelIndex == _curveData.ReelIndex &&
                        evt.State == AnimationState.Prepared)), Times.Once);
        }

        [TestMethod]
        public async Task PrepareStepperInterruptNeverPublishesIfAnimationListIsNull()
        {
            var interrupt = new ReelAnimationsPrepared();

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperCurves>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            InitializeAndLoadAnimation(_testStepperCurveFile);
            await _usbCommunicator.PrepareAnimation(_curveData);

            _eventBus.Verify(x => x.Publish(It.Is<ReelAnimationUpdatedEvent>(
                evt => evt.AnimationName == _curveData.AnimationName &&
                        evt.ReelIndex == _curveData.ReelIndex &&
                        evt.State == AnimationState.Prepared)), Times.Never);
        }

        [TestMethod]
        public async Task StepperPlayingInterruptPublishesCorrectEvent()
        {
            var interrupt = new ReelPlayingAnimation
            {
                AnimationId = _storedFile.FileId,
                ReelIndex = _curveData.ReelIndex
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<StartAnimations>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            InitializeAndLoadAnimation(_testStepperCurveFile);
            await _usbCommunicator.PrepareAnimation(_curveData);
            await _usbCommunicator.PlayAnimations();

            _eventBus.Verify(x => x.Publish(It.Is<ReelAnimationUpdatedEvent>(
                evt => evt.AnimationName == _curveData.AnimationName &&
                        evt.ReelIndex == _curveData.ReelIndex &&
                        evt.State == AnimationState.Started)), Times.Once);
        }

        [TestMethod]
        public async Task StepperFinishedInterruptPublishesCorrectEvent()
        {
            var interrupt = new ReelFinishedAnimation
            {
                AnimationId = _storedFile.FileId,
                ReelIndex = _curveData.ReelIndex
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<StartAnimations>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            InitializeAndLoadAnimation(_testStepperCurveFile);
            await _usbCommunicator.PlayAnimations();

            _eventBus.Verify(x => x.Publish(It.Is<ReelAnimationUpdatedEvent>(
                evt => evt.AnimationName == _curveData.AnimationName &&
                       evt.ReelIndex == _curveData.ReelIndex &&
                       evt.State == AnimationState.Stopped)), Times.Once);
        }

        [TestMethod]
        public async Task AllLightShowsClearedInterruptPublishesCorrectEvent()
        {
            var interrupt = new AllLightShowsCleared();

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<StopAllLightShowAnimations>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Raises(x => x.InterruptReceived += null, new RelmInterruptEventArgs(interrupt));

            InitializeAndLoadAnimation(_testStepperCurveFile);
            await _usbCommunicator.StopAllLightShows();

            _eventBus.Verify(x => x.Publish(It.IsAny<AllLightShowsClearedEvent>()), Times.Once);
        }

        private async void InitializeAndLoadAnimation(AnimationFile animationFile) {
            await _controller.Initialize(_usbCommunicator);
            await _usbCommunicator.LoadAnimationFile(animationFile);
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
