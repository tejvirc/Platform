namespace Aristocrat.Monaco.Application.Tests.Authentication
{
    using Application.Authentication;
    using Aristocrat.Monaco.Common.Storage;
    using Contracts.Authentication;
    using Kernel;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Test.Common;

    [TestClass]
    public class ComponentHashCalculatorTest
    {
        private const int EndOfStream = -1;

        private const int StreamLength = 127;

        private const string AlgorithmName = "HMACSHA1";

        private readonly Mock<IComponentRegistry> _componentRegistry = new Mock<IComponentRegistry>();
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>();
        private readonly Mock<IFileSystemProvider> _fileSystem = new Mock<IFileSystemProvider>();

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        [DataRow(true, null, 1, EndOfStream, DisplayName = "Computing hash with null algorithm")]
        [DataRow(false, AlgorithmName, 1, EndOfStream, DisplayName = "Computing hash with null stream")]
        [DataRow(true, "", 1, EndOfStream, DisplayName = "Computing hash with algorithm is empty")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenComputeHashExpectArgumentNullException(bool createStream, string algorithmName, long startOffset, long endOffset)
        {
            TestComputeHashForException(createStream, algorithmName, startOffset, endOffset);
        }

        [DataRow(true, AlgorithmName, 1, StreamLength + 1, DisplayName = "Computing hash with end offset more than stream length")]
        [DataRow(true, AlgorithmName, StreamLength + 1, EndOfStream, DisplayName = "Computing hash with start offset more than stream length")]
        [DataRow(true, AlgorithmName, 1, EndOfStream - 1, DisplayName = "Computing hash with end offset is lower than end of stream")]
        [DataRow(true, AlgorithmName, -1, EndOfStream, DisplayName = "Computing hash with start offset is lower than zero")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenComputeHashExpectArgumentOutOfRangeException(bool createStream, string algorithmName, long startOffset, long endOffset)
        {
            TestComputeHashForException(createStream, algorithmName, startOffset, endOffset);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenComputeHashWithStreamCanNotSeekExpectException()
        {
            var stream = new Mock<Stream>();
            stream.SetupGet(m => m.CanSeek).Returns(false);
            stream.SetupGet(m => m.Length).Returns(StreamLength);

            var hashCalculator = new AuthenticationService(_componentRegistry.Object, _eventBus.Object, _fileSystem.Object);

            hashCalculator.ComputeHash(
                stream.Object,
                AlgorithmName,
                new byte[0],
                new byte[0],
                1,
                EndOfStream);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenComputeHashWithUnknownAlgorithmExpectException()
        {
            TestComputeHashForException(true, "UNKNOWN", 1, EndOfStream);
        }

        [TestMethod]
        public void WhenComputeHashWithValidArgumentsWithoutAdditionalParamsExpectSuccess()
        {
            var stream = CreateStream();

            var hashCalculator = new AuthenticationService(_componentRegistry.Object, _eventBus.Object, _fileSystem.Object);

            var result = hashCalculator.ComputeHash(
                stream,
                AlgorithmName);

            Assert.IsFalse(result == null);
        }

        [TestMethod]
        public void WhenComputeHashWithValidArgumentsExpectSuccess()
        {
            var stream = CreateStream();

            var hashCalculator = new AuthenticationService(_componentRegistry.Object, _eventBus.Object, _fileSystem.Object);

            var result = hashCalculator.ComputeHash(
                stream,
                AlgorithmName,
                new byte[] { 1 },
                new byte[] { 1 },
                1,
                1);

            Assert.IsFalse(result == null);
        }

        [DataRow("0123456789", "SHA1", "87acec17cd9dcd20a716cc2cf67417b71c8a7016", new byte[0], new byte[0], DisplayName= "Calculate Sha1 hash with different Start/End offset")]
        [DataRow("0123456789", "SHA1", "5dd4ebdac62609c834f7768f02286b798bd82a38", new byte[0], new byte[0], (long)2, (long)8, DisplayName = "Calculate Sha1 hash with different Start/End offset")]
        [DataRow("0123456789", "SHA1", "6c69afa9ee57170f2fccd9c0a9d24c24a7495c7a", new byte[0], new byte[0], (long)5, (long)5, DisplayName = "Calculate Sha1 hash with same Start/End offset")]
        [DataRow("", "Crc16", "0000", null, new byte[0], DisplayName = "Calculate Ccr16 with valid arguments without key and salt")]
        [DataRow("0123456789", "Crc16", "E9C3", null, new byte[] { 0xFF, 0xFF }, DisplayName = "Calculate Ccr16 with valid arguments and with salt")]
        [DataRow("0123456789", "Crc16", "6E5F", null, new byte[0], DisplayName = "Calculate Ccr16 with valid arguments and with empty salt")]
        [DataRow("0123456789", "Crc16", "D820", null, new byte[0], (long)2, (long)8, DisplayName = "Calculate Ccr16 with valid arguments and different Start/End offset")]
        [DataRow("0123456789", "Crc16", "8E89", null, new byte[0], (long)5, (long)5, DisplayName = "Calculate Ccr16 with valid arguments and same Start/End offset")]
        [DataRow("0123456789", "Sha1", "87ACEC17CD9DCD20A716CC2CF67417B71C8A7016", null, new byte[0], DisplayName = "Calculate Sha1 with valid arguments")] // https://www.liavaag.org/English/SHA-Generator/HMAC/ was used for the assert checks
        [DataRow("0123456789", "Sha256", "84D89877F0D4041EFB6BF91A16F0248F2FD573E6AF05C19F96BEDB9F882F7882", null, new byte[0], DisplayName = "Calculate Sha256 with valid arguments")]
        [DataRow("0123456789", "HmacSha1", "0162206D7E984A0AD68415031B4736A562645B23", null, new byte[0], DisplayName = "Calculate HmacSha1 with valid arguments")]
        [DataRow("0123456789", "HmacSha256", "C16AE58CB83509417D24E1ED6479075A4A90FAC3882C430AC64FFAC70D41343F", null, new byte[0], DisplayName = "Calculate HmacSha256 with valid arguments")]
        [DataRow("0123456789", "HmacSha512", "4AAD3A1CE49D9F94765D1DC5A0EDB78E175DF97460BFFA6C9B20D25320C0C8B3F1E970882E5FFD244C9A1CA484ED48855DDA6140308C18F502CF72E006A62D4D", null, new byte[0], DisplayName = "Calculate HmacSha512 with valid arguments")]
        [DataTestMethod]
        public void WhenComputeHashExpectSuccess(string streamContents, string algorithmName, string expectedResult, byte[] salt, byte[] key, long? startOffset = null, long? endOffset = null)
        {
            Assert.AreEqual(expectedResult, ConvertExtensions.ToPackedHexString(TestComputeHashForHashResult(streamContents, algorithmName, salt, key, startOffset, endOffset)), true);
        }

        [TestMethod]
        public void WhenComputeUsingHmacSha1WithKeyMSBFirstExpectSuccess()
        {
            var stream = CreateStream("0123456789");
            var authenticationService = new AuthenticationService(_componentRegistry.Object, _eventBus.Object, _fileSystem.Object);
            var result = authenticationService.ComputeHash(
                stream,
                AlgorithmType.HmacSha1.ToString(),
                null,
                ConvertExtensions.FromPackedHexString("123456789ABCDEF123456789ABCDEF123456789A"));

            Assert.AreEqual("3D71428503FB226D15A80ABDB0F264C802248E7B", ConvertExtensions.ToPackedHexString(result), true);
        }

        // Master Result is xor result of individual hashes, http://xor.pw/ used for assert result
        [TestMethod]
        public void WhenComputeMasterResultExpectSuccess()
        {
            var hashes = new List<byte[]> {
                ConvertExtensions.FromPackedHexString("665112169CC0D1DF679D924038CF8DB7141047E1"),
                ConvertExtensions.FromPackedHexString("01C84A2FDA3245803A6A97DC50958C57659F83B7"),
                ConvertExtensions.FromPackedHexString("41BA1B98211631DB1B39507D579C28C561F89981"),
                ConvertExtensions.FromPackedHexString("2077335E58344EF8B68ECC6566B1BC89AD37D49D"),
                ConvertExtensions.FromPackedHexString("4C9472E6073FDEFA7720F87308AFDE6864C7D546"),
            };

            var masterResult = new BitArray(8 * 20);
            foreach (var hash in hashes) {
                masterResult.Xor(new BitArray(hash));
            }

            Assert.AreEqual("4AC0021938EF3586876061F751D84BC4D9875C0C", ConvertExtensions.ToPackedHexString(masterResult), true);
        }

        private void TestComputeHashForException(bool createStream, string algorithmName, long startOffset, long endOffset)
        {
            Stream stream = null;
            if (createStream)
            {
                stream = CreateStream();
            }
            var hashCalculator = new AuthenticationService(_componentRegistry.Object, _eventBus.Object, _fileSystem.Object);
            hashCalculator.ComputeHash(
                stream,
                algorithmName,
                new byte[0],
                new byte[0],
                startOffset,
                endOffset);
        }

        private byte[] TestComputeHashForHashResult(string streamContents, string algorithmName, byte[] salt, byte[] key, long? startOffset = null, long? endOffset = null)
        {
            Stream stream;
            if (streamContents == null)
            {
                stream = CreateStream();
            } else
            {
                stream = CreateStream(streamContents);
            }
            
            var hashCalculator = new AuthenticationService(_componentRegistry.Object, _eventBus.Object, _fileSystem.Object);

            if (startOffset.HasValue && endOffset.HasValue)
            {
                return hashCalculator.ComputeHash(
                    stream,
                    algorithmName,
                    salt,
                    key,
                    startOffset.Value,
                    endOffset.Value);
            }
            else
            {
                return hashCalculator.ComputeHash(
                    stream,
                    algorithmName,
                    salt,
                    key);
            }

        }

        private static Stream CreateStream()
        {
            return new MemoryStream(Enumerable.Range(0, StreamLength).Select(x => (byte)x).ToArray());
        }

        public static Stream CreateStream(string someArbitraryString)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.Write(someArbitraryString);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }
    }
}
