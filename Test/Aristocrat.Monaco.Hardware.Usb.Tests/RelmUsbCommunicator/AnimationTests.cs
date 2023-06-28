namespace Aristocrat.Monaco.Hardware.Usb.Tests.RelmUsbCommunicator
{
    using System.Linq;
    using Contracts.Reel;
    using Contracts.Reel.ControlData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using RelmReels.Communicator.Downloads;
    using RelmReels.Messages;
    using System.Threading;
    using System.Threading.Tasks;
    using Usb.ReelController.Relm;

    [TestClass]
    public class AnimationTests
    {
        private const string AnimationPath = @"\TestDirectory\TestFile.show";
        private const string DefaultFriendlyName = "TestFile.show";
        private const string FriendlyName = "TestShow";
        private const uint AnimationId = 1234;
        private const int FileLength = 4321;

        private readonly AnimationFile _namedAnimationFile = new(
            AnimationPath,
            AnimationType.PlatformLightShow,
            FriendlyName);

        private readonly AnimationFile _unnamedAnimationFile = new(
            AnimationPath,
            AnimationType.PlatformLightShow);

        [TestMethod]
        public async Task LoadedAnimationsShouldBeInCollectionTest()
        {
            var driver = new Mock<RelmReels.Communicator.IRelmCommunicator>();
            driver.Setup(x => x.Download(It.IsAny<string>(), It.IsAny<BitmapVerification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new StoredFile(FriendlyName, AnimationId, FileLength)));
            driver.Setup(x => x.SendCommandAsync(It.IsAny<RelmCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            var usbCommunicator = new RelmUsbCommunicator(driver.Object);

            await usbCommunicator.LoadAnimationFile(_namedAnimationFile, default);

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
            var usbCommunicator = new RelmUsbCommunicator(driver.Object);

            await usbCommunicator.LoadAnimationFiles(new[] {_namedAnimationFile, _namedAnimationFile}, default);

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
            var usbCommunicator = new RelmUsbCommunicator(driver.Object);

            await usbCommunicator.LoadAnimationFile(_unnamedAnimationFile, default);

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
            var usbCommunicator = new RelmUsbCommunicator(driver.Object);

            await usbCommunicator.LoadAnimationFile(_namedAnimationFile, default);
            await usbCommunicator.RemoveAllControllerAnimations(default);

            Assert.AreEqual(usbCommunicator.AnimationFiles.Count, 0);
        }
    }
}
