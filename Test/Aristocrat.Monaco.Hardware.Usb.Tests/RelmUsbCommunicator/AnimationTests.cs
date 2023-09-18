namespace Aristocrat.Monaco.Hardware.Usb.Tests.RelmUsbCommunicator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Common.Cryptography;
    using Aristocrat.Monaco.Common.Storage;
    using Aristocrat.Monaco.Test.Common;
    using Contracts;
    using Contracts.Reel;
    using Contracts.Reel.ControlData;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using RelmReels;
    using RelmReels.Communicator;
    using RelmReels.Communicator.Downloads;
    using RelmReels.Messages;
    using RelmReels.Messages.Commands;
    using RelmReels.Messages.Interrupts;
    using RelmReels.Messages.Queries;
    using Usb.ReelController.Relm;

    [TestClass]
    public class AnimationTests
    {
        private const string AnimationPath = @"\\TestDirectory\TestFile.show";
        private const string DefaultFriendlyName = "TestFile";
        private const string FriendlyName = "TestShow";
        private const string Tag = "TestTag";
        private const uint AnimationId = 1234;
        private const int FileLength = 4321;

        private readonly AnimationFile _namedAnimationFile = new(
            AnimationPath,
            AnimationType.PlatformLightShow,
            FriendlyName);

        private readonly AnimationFile _unnamedAnimationFile = new(
            AnimationPath,
            AnimationType.PlatformLightShow);

        private readonly LightShowData _lightShow1 = new(0, FriendlyName, Tag, ReelConstants.RepeatOnce, -1);
        private readonly LightShowData _lightShow2 = new(0, DefaultFriendlyName, Tag, ReelConstants.RepeatOnce, -1);
        private readonly ReelCurveData _curve1 = new(1, FriendlyName);
        private readonly ReelCurveData _curve2 = new(2, DefaultFriendlyName);
        private readonly MemoryStream _fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        private StoredAnimationIds _storedAnimationIds;
        private RelmResponse<StoredAnimationIds> _storedAnimationIdsResponse;
        private AnimationHashCompleted _animationHashCompleted;
        private RelmResponse<AnimationHashCompleted> _animationHashCompletedResponse;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IRelmCommunicator> _driver;
        private Mock<IFileSystemProvider> _fileSystem;
        private RelmUsbCommunicator _target;
        private Mock<IEventBus> _eventBus;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoNotResetRelmController, It.IsAny<bool>())).Returns(false);

            _storedAnimationIds = new();
            _storedAnimationIdsResponse = new(true, _storedAnimationIds);

            using var crc = new Crc32();
            var hash = crc.ComputeHash(_fileStream).Reverse().ToArray();
            _fileStream.Position = 0;

            _animationHashCompleted = new()
            {
                Hash = hash
            };
            _animationHashCompletedResponse = new(true, _animationHashCompleted);
            _driver = new Mock<IRelmCommunicator>();

            var localizer = new Mock<ILocalizer>();
            localizer
                .Setup(x => x.GetString(It.IsAny<string>()))
                .Returns("empty");

            var localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Default);
            localizerFactory
                .Setup(x => x.For(It.IsAny<string>()))
                .Returns(new Mock<ILocalizer>().Object);
            localizerFactory
                .Setup(x => x.For(It.IsAny<string>()))
                .Returns(localizer.Object);

            _fileSystem = new Mock<IFileSystemProvider>();
            _fileSystem
                .Setup(x => x.GetFileReadStream(It.IsAny<string>()))
                .Returns(_fileStream);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Loose);
            _target = new RelmUsbCommunicator(_driver.Object, _propertiesManager.Object, _fileSystem.Object, _eventBus.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _target.Dispose();
            _fileStream.Dispose();
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public async Task LoadedAnimationsShouldBeInCollectionTest()
        {
            _driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength, true)));
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<RelmCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _driver.Setup(x => x.SendQueryAsync<StoredAnimationIds>(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedAnimationIdsResponse));
            _driver.Setup(x => x.SendCommandWithResponseAsync(It.IsAny<CalculateAnimationHash>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_animationHashCompletedResponse));

            await _target.LoadAnimationFile(_namedAnimationFile);

            Assert.AreEqual(_target.AnimationFiles.Count, 1);
            Assert.AreEqual(_target.AnimationFiles.First().FriendlyName, FriendlyName);
            Assert.AreEqual(_target.AnimationFiles.First().AnimationId, AnimationId);
        }

        [TestMethod]
        public async Task DuplicateAnimationsShouldNotBeInCollectionTest()
        {
            var fileNameHash = Path.GetFileName(_namedAnimationFile.Path).HashDjb2();
            _driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    _storedAnimationIds.AnimationIds = new[] { fileNameHash };
                })
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength, true)));
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<RelmCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _driver.Setup(x => x.SendQueryAsync<StoredAnimationIds>(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedAnimationIdsResponse));
            _driver.Setup(x => x.SendCommandWithResponseAsync(It.IsAny<CalculateAnimationHash>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_animationHashCompletedResponse));

            await _target.LoadAnimationFiles(new[] { _namedAnimationFile }, new Progress<LoadingAnimationFileModel>());
            await _target.LoadAnimationFiles(new[] { _namedAnimationFile }, new Progress<LoadingAnimationFileModel>());

            Assert.AreEqual(_target.AnimationFiles.Count, 1);
            Assert.AreEqual(_target.AnimationFiles.First().FriendlyName, FriendlyName);
            Assert.AreEqual(_target.AnimationFiles.First().AnimationId, AnimationId);
            _driver.Verify(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ModifiedAnimationsShouldBeLoadedTest()
        {
            _storedAnimationIdsResponse = new(true, new StoredAnimationIds { AnimationIds = new[] { FriendlyName.HashDjb2() } });

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<RelmCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _driver.Setup(x => x.SendCommandWithResponseAsync(It.IsAny<CalculateAnimationHash>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_animationHashCompletedResponse));
            _driver.Setup(x => x.SendQueryAsync<StoredAnimationIds>(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedAnimationIdsResponse));

            await _target.LoadAnimationFiles(new[] { _namedAnimationFile }, new Progress<LoadingAnimationFileModel>());
            _driver.Verify(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task MissingFriendlyNameShouldBeSetToFileNameTest()
        {
            _driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(string.Empty, AnimationId, FileLength, true)));
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<RelmCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _driver.Setup(x => x.SendQueryAsync<StoredAnimationIds>(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedAnimationIdsResponse));

            await _target.LoadAnimationFile(_unnamedAnimationFile);

            Assert.AreEqual(_target.AnimationFiles.Count, 1);
            Assert.AreEqual(_target.AnimationFiles.First().FriendlyName, DefaultFriendlyName);
            Assert.AreEqual(_target.AnimationFiles.First().AnimationId, AnimationId);
        }

        [TestMethod]
        public async Task AnimationCollectionShouldBeEmptyAfterRemoveTest()
        {
            _driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(string.Empty, AnimationId, FileLength, true)));
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<RelmCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _driver.Setup(x => x.SendQueryAsync<StoredAnimationIds>(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedAnimationIdsResponse));

            await _target.LoadAnimationFile(_namedAnimationFile);
            await _target.RemoveAllControllerAnimations();

            Assert.AreEqual(_target.AnimationFiles.Count, 0);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task StopLightShowAnimationsReturnsControllerResult(bool controllerResult)
        {
            _driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength, true)));
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<StopLightShowAnimation>(), default))
                .Returns(Task.FromResult(controllerResult));
            _driver.Setup(x => x.SendQueryAsync<StoredAnimationIds>(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedAnimationIdsResponse));

            await _target.LoadAnimationFile(_namedAnimationFile);
            var result = await _target.StopLightShowAnimations(new List<LightShowData> { _lightShow1 });

            _driver.Verify(x => x.SendCommandAsync(It.IsAny<StopLightShowAnimation>(), default), Times.Once);

            Assert.AreEqual(controllerResult, result);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task StopAllAnimationTagsReturnsControllerResultWhenShowExists(bool controllerResult)
        {
            _driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength, true)));
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<StopAllAnimationTags>(), default))
                .Returns(Task.FromResult(controllerResult));
            _driver.Setup(x => x.SendQueryAsync<StoredAnimationIds>(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedAnimationIdsResponse));

            await _target.LoadAnimationFile(_namedAnimationFile);
            var result = await _target.StopAllAnimationTags(FriendlyName);

            _driver.Verify(x => x.SendCommandAsync(It.IsAny<StopAllAnimationTags>(), default), Times.Once);
            Assert.AreEqual(controllerResult, result);
        }

        [TestMethod]
        public async Task StopAllAnimationTagsReturnsFalseWhenShowNotExist()
        {
            var result = await _target.StopAllAnimationTags(FriendlyName);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task LightShowPrepareAnimationsFailsWhenOneNotLoaded()
        {
            _driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength, true)));
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareLightShowAnimations>(), default))
                .Returns(Task.FromResult(true));
            _driver.Setup(x => x.SendQueryAsync<StoredAnimationIds>(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedAnimationIdsResponse));

            await _target.LoadAnimationFile(_namedAnimationFile);
            var result = await _target.PrepareAnimations(new List<LightShowData> { _lightShow1, _lightShow2 });
            Assert.IsFalse(result);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task LightShowPrepareAnimationReturnsControllerResult(bool controllerResult)
        {
            _driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength, true)));
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareLightShowAnimations>(), default))
                .Returns(Task.FromResult(controllerResult));
            _driver.Setup(x => x.SendQueryAsync<StoredAnimationIds>(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedAnimationIdsResponse));

            await _target.LoadAnimationFile(_namedAnimationFile);
            var result = await _target.PrepareAnimation(_lightShow1);
            Assert.AreEqual(controllerResult, result);
        }

        [TestMethod]
        public async Task CurvePrepareAnimationsFailsWhenOneNotLoaded()
        {
            _driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength, true)));
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperCurves>(), default))
                .Returns(Task.FromResult(true));
            _driver.Setup(x => x.SendQueryAsync<StoredAnimationIds>(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedAnimationIdsResponse));

            var curves = new List<ReelCurveData> { _curve1, _curve2 };

            await _target.LoadAnimationFile(_namedAnimationFile);
            var result = await _target.PrepareAnimations(curves);

            Assert.IsFalse(result);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CurvePrepareAnimationReturnsControllerResult(bool controllerResult)
        {
            _driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength, true)));
            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperCurves>(), default))
                .Returns(Task.FromResult(controllerResult));
            _driver.Setup(x => x.SendQueryAsync<StoredAnimationIds>(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_storedAnimationIdsResponse));

            await _target.LoadAnimationFile(_namedAnimationFile);
            var result = await _target.PrepareAnimation(_curve1);

            Assert.AreEqual(controllerResult, result);
        }
    }
}
