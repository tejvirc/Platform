namespace Aristocrat.Monaco.Hardware.Usb.Tests.RelmUsbCommunicator
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Reel;
    using Contracts.Reel.ControlData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using RelmReels.Communicator.Downloads;
    using RelmReels.Messages;
    using RelmReels.Messages.Commands;
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

        private readonly LightShowData _lightShow1 = new() { AnimationName = FriendlyName, Tag = Tag };
        private readonly LightShowData _lightShow2 = new() { AnimationName = DefaultFriendlyName, Tag = Tag };
        private readonly ReelCurveData _curve1 = new(1, FriendlyName);
        private readonly ReelCurveData _curve2 = new(2, DefaultFriendlyName);

        [TestMethod]
        public async Task LoadedAnimationsShouldBeInCollectionTest()
        {
            var driver = new Mock<RelmReels.Communicator.IRelmCommunicator>();
            driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength)));
            driver.Setup(x => x.SendCommandAsync(It.IsAny<RelmCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            var usbCommunicator = new RelmUsbCommunicator(driver.Object, null);

            await usbCommunicator.LoadAnimationFile(_namedAnimationFile);

            Assert.AreEqual(usbCommunicator.AnimationFiles.Count, 1);
            Assert.AreEqual(usbCommunicator.AnimationFiles.First().FriendlyName, FriendlyName);
            Assert.AreEqual(usbCommunicator.AnimationFiles.First().AnimationId, AnimationId);
        }

        [TestMethod]
        public async Task DuplicateAnimationsShouldNotBeInCollectionTest()
        {
            var driver = new Mock<RelmReels.Communicator.IRelmCommunicator>();
            driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength)));
            driver.Setup(x => x.SendCommandAsync(It.IsAny<RelmCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            var usbCommunicator = new RelmUsbCommunicator(driver.Object, null);

            await usbCommunicator.LoadAnimationFiles(new[] {_namedAnimationFile, _namedAnimationFile});

            Assert.AreEqual(usbCommunicator.AnimationFiles.Count, 1);
            Assert.AreEqual(usbCommunicator.AnimationFiles.First().FriendlyName, FriendlyName);
            Assert.AreEqual(usbCommunicator.AnimationFiles.First().AnimationId, AnimationId);
        }

        [TestMethod]
        public async Task MissingFriendlyNameShouldBeSetToFileNameTest()
        {
            var driver = new Mock<RelmReels.Communicator.IRelmCommunicator>();
            driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(string.Empty, AnimationId, FileLength)));
            driver.Setup(x => x.SendCommandAsync(It.IsAny<RelmCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            var usbCommunicator = new RelmUsbCommunicator(driver.Object, null);

            await usbCommunicator.LoadAnimationFile(_unnamedAnimationFile);

            Assert.AreEqual(usbCommunicator.AnimationFiles.Count, 1);
            Assert.AreEqual(usbCommunicator.AnimationFiles.First().FriendlyName, DefaultFriendlyName);
            Assert.AreEqual(usbCommunicator.AnimationFiles.First().AnimationId, AnimationId);
        }

        [TestMethod]
        public async Task AnimationCollectionShouldBeEmptyAfterRemoveTest()
        {
            var driver = new Mock<RelmReels.Communicator.IRelmCommunicator>();
            driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(string.Empty, AnimationId, FileLength)));
            driver.Setup(x => x.SendCommandAsync(It.IsAny<RelmCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            var usbCommunicator = new RelmUsbCommunicator(driver.Object, null);

            await usbCommunicator.LoadAnimationFile(_namedAnimationFile);
            await usbCommunicator.RemoveAllControllerAnimations();

            Assert.AreEqual(usbCommunicator.AnimationFiles.Count, 0);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task StopAllAnimationTagsReturnsControllerResultWhenShowExists(bool controllerResult)
        {
            var driver = new Mock<RelmReels.Communicator.IRelmCommunicator>();
            driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength)));
            driver.Setup(x => x.SendCommandAsync(It.IsAny<StopAllAnimationTags>(), default))
                .Returns(Task.FromResult(controllerResult));
            var usbCommunicator = new RelmUsbCommunicator(driver.Object, null);
            
            await usbCommunicator.LoadAnimationFile(_namedAnimationFile);
            var result = await usbCommunicator.StopAllAnimationTags(FriendlyName);
            
            driver.Verify(x => x.SendCommandAsync(It.IsAny<StopAllAnimationTags>(), default), Times.Once);
            Assert.AreEqual(controllerResult, result);
        }

        [TestMethod]
        public async Task StopAllAnimationTagsReturnsFalseWhenShowNotExist()
        {
            var driver = new Mock<RelmReels.Communicator.IRelmCommunicator>();
            var usbCommunicator = new RelmUsbCommunicator(driver.Object, null);

            var result = await usbCommunicator.StopAllAnimationTags(FriendlyName);
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        public async Task LightShowPrepareAnimationsFailsWhenOneNotLoaded()
        {
            var driver = new Mock<RelmReels.Communicator.IRelmCommunicator>();
            driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength)));
            driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareLightShowAnimations>(), default))
                .Returns(Task.FromResult(true));

            var usbCommunicator = new RelmUsbCommunicator(driver.Object, null);
            var lightShows = new List<LightShowData> { _lightShow1, _lightShow2 };

            await usbCommunicator.LoadAnimationFile(_namedAnimationFile);
            var result = await usbCommunicator.PrepareAnimations(lightShows);

            Assert.IsFalse(result);
        }
        
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task LightShowPrepareAnimationReturnsControllerResult(bool controllerResult)
        {
            var driver = new Mock<RelmReels.Communicator.IRelmCommunicator>();
            driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength)));
            driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareLightShowAnimations>(), default))
                .Returns(Task.FromResult(controllerResult));

            var usbCommunicator = new RelmUsbCommunicator(driver.Object, null);

            await usbCommunicator.LoadAnimationFile(_namedAnimationFile);
            var result = await usbCommunicator.PrepareAnimation(_lightShow1);

            Assert.AreEqual(controllerResult, result);
        }
        
        [TestMethod]
        public async Task CurvePrepareAnimationsFailsWhenOneNotLoaded()
        {
            var driver = new Mock<RelmReels.Communicator.IRelmCommunicator>();
            driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength)));
            driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperCurves>(), default))
                .Returns(Task.FromResult(true));

            var usbCommunicator = new RelmUsbCommunicator(driver.Object, null);
            var curves = new List<ReelCurveData> { _curve1, _curve2 };

            await usbCommunicator.LoadAnimationFile(_namedAnimationFile);
            var result = await usbCommunicator.PrepareAnimations(curves);

            Assert.IsFalse(result);
        }
        
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CurvePrepareAnimationReturnsControllerResult(bool controllerResult)
        {
            var driver = new Mock<RelmReels.Communicator.IRelmCommunicator>();
            driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength)));
            driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperCurves>(), default))
                .Returns(Task.FromResult(controllerResult));

            var usbCommunicator = new RelmUsbCommunicator(driver.Object, null);

            await usbCommunicator.LoadAnimationFile(_namedAnimationFile);
            var result = await usbCommunicator.PrepareAnimation(_curve1);

            Assert.AreEqual(controllerResult, result);
        }
    }
}
