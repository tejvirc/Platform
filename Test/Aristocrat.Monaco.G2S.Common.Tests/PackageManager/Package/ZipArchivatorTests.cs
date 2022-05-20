namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.Package
{
    using System;
    using System.IO;
    using Protocol.Common.Installer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ZipArchivatorTests
    {
        private string _testDirectoryPath;

        private string _testOutputDirectoryPath;
        private ZipArchive _zipArchive;

        [TestInitialize]
        public void Initialize()
        {
            _testDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PackageManager/TestData/TestZip");
            _testOutputDirectoryPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "PackageManager/TestData/TestZipOut");

            _zipArchive = new ZipArchive();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDirectoryPath))
            {
                Directory.Delete(_testDirectoryPath, true);
            }

            if (Directory.Exists(_testOutputDirectoryPath))
            {
                Directory.Delete(_testOutputDirectoryPath, true);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPackSourceDirectoryIsNullExpectException()
        {
            _zipArchive.Pack(null, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPackTargetArchiveStreamIsNullExpectException()
        {
            _zipArchive.Pack(_testDirectoryPath, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenUnpackSourceDirectoryIsNullExpectException()
        {
            _zipArchive.Unpack(null, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenUnpackArchiveStreamIsNullExpectException()
        {
            _zipArchive.Unpack(_testDirectoryPath, null);
        }

        [TestMethod]
        public void WhenPackThenUnpackContentIsTheSameExpectSuccess()
        {
            ArchiveTestDirectoryCreator.Create(_testDirectoryPath);

            Stream archiveStream = new MemoryStream();

            _zipArchive.Pack(_testDirectoryPath, archiveStream);
            _zipArchive.Unpack(_testOutputDirectoryPath, archiveStream);

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