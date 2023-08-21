namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.Package
{
    using System;
    using System.IO;
    using Protocol.Common.Installer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class TarArchivatorTests
    {
        private TarArchive _tarArchive;

        private string _testDirectoryPath;

        private string _testOutputDirectoryPath;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            //_testDirectoryPath = Path.Combine(AppContext.BaseDirectory, "TestData\\TestTar").Replace("\\", "/").TrimEnd('/');
            //_testOutputDirectoryPath = Path.Combine(AppContext.BaseDirectory, "TestData/TestTarOut").Replace("\\", "/").TrimEnd('/');

            _testDirectoryPath = Path.Combine(AppContext.BaseDirectory, "TestData\\TestTar");
            _testOutputDirectoryPath = Path.Combine(AppContext.BaseDirectory, "TestData/TestTarOut");

            _tarArchive = new TarArchive();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDirectoryPath))
            {
                Directory.Delete(_testDirectoryPath, true);
            }

            if (Directory.Exists(_testDirectoryPath))
            {
                Directory.Delete(_testDirectoryPath, true);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPackSourceDirectoryIsNullExpectException()
        {
            _tarArchive.Pack(null, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPackTargetArchiveStreamIsNullExpectException()
        {
            _tarArchive.Pack(_testDirectoryPath, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenUnpackSourceDirectoryIsNullExpectException()
        {
            _tarArchive.Unpack(null, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenUnpackArchiveStreamIsNullExpectException()
        {
            _tarArchive.Unpack(_testDirectoryPath, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenUnpackArhiveStreamCanSeekIsFalseExpectException()
        {
            var streamMock = new Mock<Stream>();
            streamMock.SetupGet(m => m.CanSeek).Returns(false);
            _tarArchive.Unpack(_testDirectoryPath, streamMock.Object);
        }

        [TestMethod]
        public void WhenPackThenUnpackContentIsTheSameExpectSuccess()
        {
            ArchiveTestDirectoryCreator.Create(_testDirectoryPath);

            Stream archiveStream = new MemoryStream();

            _tarArchive.Pack(_testDirectoryPath, archiveStream);
            _tarArchive.Unpack(_testOutputDirectoryPath, archiveStream);

            archiveStream.Close();

            Assert.IsTrue(
                Directory.Exists(Path.Combine(_testOutputDirectoryPath, ArchiveTestDirectoryCreator.NestedFolderName)));
            Assert.IsTrue(
                File.Exists(Path.Combine(_testOutputDirectoryPath, ArchiveTestDirectoryCreator.NestedFileName)));
            Assert.IsTrue(
                File.Exists(Path.Combine(_testOutputDirectoryPath, ArchiveTestDirectoryCreator.RootFileName)));
            CollectionAssert.AreEqual(
                File.ReadAllBytes(Path.Combine(_testOutputDirectoryPath, ArchiveTestDirectoryCreator.NestedFileName)),
                File.ReadAllBytes(Path.Combine(_testDirectoryPath, ArchiveTestDirectoryCreator.NestedFileName)));
            CollectionAssert.AreEqual(
                File.ReadAllBytes(Path.Combine(_testOutputDirectoryPath, ArchiveTestDirectoryCreator.RootFileName)),
                File.ReadAllBytes(Path.Combine(_testDirectoryPath, ArchiveTestDirectoryCreator.RootFileName)));

            archiveStream.Dispose();
        }
    }
}