namespace Aristocrat.Monaco.Application.Tests.Monitors
{
    using Application.Monitors;
    using Kernel.Contracts.MessageDisplay;
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.IO;
    using Test.Common;

    [TestClass]
    public class ReadOnlyMediaMonitorTest
    {
        private const string TempFile = "test.tmp";

        private Mock<IEventBus> _bus;
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IPathMapper> _pathMapper;
        private Mock<IPropertiesManager> _properties;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);

            _bus = new Mock<IEventBus>();
            _disableManager = new Mock<ISystemDisableManager>();
            _pathMapper = new Mock<IPathMapper>();
            _properties = new Mock<IPropertiesManager>();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            var monitor = new ReadOnlyMediaMonitor(
                _properties.Object,
                _pathMapper.Object,
                _disableManager.Object,
                _bus.Object);

            Assert.IsNotNull(monitor);
        }

        [TestMethod]
        public void WhenInitializeAndNotRequiredExpectNothing()
        {
            _properties.Setup(m => m.GetProperty(ApplicationConstants.ReadOnlyMediaRequired, false)).Returns(false);

            var monitor = new ReadOnlyMediaMonitor(
                _properties.Object,
                _pathMapper.Object,
                _disableManager.Object,
                _bus.Object);

            monitor.Initialize();

            _pathMapper.Verify(m => m.GetDirectory(ApplicationConstants.ManifestPath), Times.Never);
        }

        [TestMethod]
        public void WhenInitializeAndOptionalExpectNothing()
        {
            _properties.Setup(m => m.GetProperty(ApplicationConstants.ReadOnlyMediaRequired, false)).Returns(true);
            _properties.Setup(m => m.GetProperty("readonlymediaoptional", "false")).Returns("true");

            var monitor = new ReadOnlyMediaMonitor(
                _properties.Object,
                _pathMapper.Object,
                _disableManager.Object,
                _bus.Object);

            monitor.Initialize();

            _pathMapper.Verify(m => m.GetDirectory(ApplicationConstants.ManifestPath), Times.Never);
        }

        [TestMethod]
        public void WhenInitializeAndRequiredWithWritableMediaExpectDisabled()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            _properties.Setup(m => m.GetProperty(ApplicationConstants.ReadOnlyMediaRequired, false)).Returns(true);
            _pathMapper.Setup(m => m.GetDirectory(ApplicationConstants.ManifestPath))
                .Returns(new DirectoryInfo(currentDirectory));

            var monitor = new ReadOnlyMediaMonitor(
                _properties.Object,
                _pathMapper.Object,
                _disableManager.Object,
                _bus.Object);

            monitor.Initialize();

            Assert.IsFalse(File.Exists(Path.Combine(currentDirectory, TempFile)));
            _disableManager.Verify(m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<string>(), It.IsAny<CultureProviderType>(), It.IsAny<object[]>()));
            _bus.Verify(m => m.Publish(It.IsAny<ReadOnlyMediaErrorEvent>()));
        }

        [TestMethod]
        public void WhenInitializeAndRequiredWithNonWritableMediaExpectSuccess()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var tempFile = Path.Combine(currentDirectory, TempFile);

            SafeDelete(tempFile);

            // This is will drive a different (unhandled) exception than what is expected when the media is read-only,
            //  but it would be a potential security risk if we handled the UnauthorizedAccessException in the monitor.
            //  This will get us close to simulating the behavior of the partition being marked read-only 
            File.WriteAllText(tempFile, string.Empty);
            File.SetAttributes(tempFile, File.GetAttributes(tempFile) | FileAttributes.ReadOnly);

            _properties.Setup(m => m.GetProperty(ApplicationConstants.ReadOnlyMediaRequired, false)).Returns(true);
            _pathMapper.Setup(m => m.GetDirectory(ApplicationConstants.ManifestPath))
                .Returns(new DirectoryInfo(currentDirectory));

            var monitor = new ReadOnlyMediaMonitor(
                _properties.Object,
                _pathMapper.Object,
                _disableManager.Object,
                _bus.Object);

            try
            {
                monitor.Initialize();
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch
            {
                Assert.Fail("Unexpected exception");
            }

            SafeDelete(tempFile);

            _disableManager.Verify(m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null), Times.Never);
            _bus.Verify(m => m.Publish(It.IsAny<ReadOnlyMediaErrorEvent>()), Times.Never);
        }

        private static void SafeDelete(string tempFile)
        {
            if (File.Exists(tempFile))
            {
                try
                {
                    File.SetAttributes(tempFile, File.GetAttributes(tempFile) & ~FileAttributes.ReadOnly);
                    File.Delete(tempFile);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}