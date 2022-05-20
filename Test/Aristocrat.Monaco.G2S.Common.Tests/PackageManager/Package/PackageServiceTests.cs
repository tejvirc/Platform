namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.Package
{
    using System;
    using System.IO;
    using Protocol.Common.Installer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class PackageServiceTests
    {
        private readonly Randomizer _random = new Randomizer();
        private PackageService _subject;
        private Mock<ITarArchive> _tarArchivatorMock;
        private Mock<IZipArchive> _zipArchivatorMock;

        [TestInitialize]
        public void SetUp()
        {
            _zipArchivatorMock = new Mock<IZipArchive>();
            _zipArchivatorMock.Setup(x => x.Pack(It.IsAny<string>(), It.IsAny<Stream>())).Verifiable();
            _zipArchivatorMock.Setup(x => x.Unpack(It.IsAny<string>(), It.IsAny<Stream>())).Verifiable();

            _tarArchivatorMock = new Mock<ITarArchive>();
            _tarArchivatorMock.Setup(x => x.Pack(It.IsAny<string>(), It.IsAny<Stream>())).Verifiable();
            _tarArchivatorMock.Setup(x => x.Unpack(It.IsAny<string>(), It.IsAny<Stream>())).Verifiable();

            _subject = new PackageService(_zipArchivatorMock.Object, _tarArchivatorMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Pack_SourceDirectoryIsNull_ThrowsArgumentNullException()
        {
            var format = _random.NextEnum<ArchiveFormat>();
            const string dir = null;
            Stream stream = null;

            try
            {
                stream = new MemoryStream();
                _subject.Pack(format, dir, stream);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Pack_SourceDirectoryIsEmpty_ThrowsArgumentException()
        {
            var format = _random.NextEnum<ArchiveFormat>();
            var dir = string.Empty;
            Stream stream = null;

            try
            {
                stream = new MemoryStream();
                _subject.Pack(format, dir, stream);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        [TestMethod]
        public void Pack_Zip_PackedAsZipArchive()
        {
            Stream stream = new MemoryStream();
            var dir = _random.GetString();

            _subject.Pack(ArchiveFormat.Zip, dir, stream);

            _zipArchivatorMock.Verify(x => x.Pack(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);

            stream.Dispose();
        }

        [TestMethod]
        public void Pack_Tar_PackedAsTarArchive()
        {
            Stream stream = new MemoryStream();
            var dir = _random.GetString();

            _subject.Pack(ArchiveFormat.Tar, dir, stream);

            _tarArchivatorMock.Verify(x => x.Pack(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);

            stream.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Ppack_InvalidFormat_ThrowsInvalidOperationException()
        {
            var invalidFormat = (ArchiveFormat)byte.MaxValue;

            var dir = _random.GetString();

            Stream stream = null;

            try
            {
                stream = new MemoryStream();
                _subject.Pack(invalidFormat, dir, stream);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Unpack_ArchiveStreamIsNull_ThrowsArgumentNullExcpetion()
        {
            var format = _random.NextEnum<ArchiveFormat>();
            const Stream stream = null;
            var dir = _random.GetString();

            _subject.Unpack(format, dir, stream);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Unpack_TargetDirectoryIsNull_ThrowsArgumentNullExcpetion()
        {
            var format = _random.NextEnum<ArchiveFormat>();
            Stream stream = new MemoryStream();
            const string dir = null;

            _subject.Unpack(format, dir, stream);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Unpack_TargetDirectoryIsEmpty_ThrowsArgumentExcpetion()
        {
            var format = _random.NextEnum<ArchiveFormat>();
            Stream stream = new MemoryStream();
            var dir = string.Empty;

            _subject.Unpack(format, dir, stream);
        }

        [TestMethod]
        public void Unpack_Zip_UnpackedAsZipArchive()
        {
            var stream = new MemoryStream();
            var dir = _random.GetString();

            _subject.Unpack(ArchiveFormat.Zip, dir, stream);

            _zipArchivatorMock.Verify(x => x.Unpack(dir, stream));
        }

        [TestMethod]
        public void Unpack_Tar_UnpackedAsTarArchive()
        {
            var stream = new MemoryStream();
            var dir = _random.GetString();

            _subject.Unpack(ArchiveFormat.Tar, dir, stream);

            _tarArchivatorMock.Verify(x => x.Unpack(dir, stream));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Unpack_InvalidFormat_ThrowsInvalidOperationException()
        {
            var invalidFormat = (ArchiveFormat)byte.MaxValue;
            var stream = new MemoryStream();
            var dir = _random.GetString();

            _subject.Unpack(invalidFormat, dir, stream);
        }
    }
}