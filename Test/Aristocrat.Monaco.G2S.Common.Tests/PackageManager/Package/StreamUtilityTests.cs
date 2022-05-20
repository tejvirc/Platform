namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.Package
{
    using System;
    using System.IO;
    using Protocol.Common.Installer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StreamUtilityTests
    {
        private readonly Random _random = new Random();
        private MemoryStream _resultStream;

        [TestCleanup]
        public void Cleanup()
        {
            _resultStream?.Close();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCopyFromInputStreamIsNullExpectException()
        {
            StreamUtility.CopyFrom(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCopyDisposedStreamIsNullExpectException()
        {
            var memoryStream = (MemoryStream)null;
            memoryStream.Copy(new MemoryStream());
        }

        [TestMethod]
        public void WhenCopyFromClosedStreamAllStreamBytesAreCopiedExpectSuccess()
        {
            var buffer = new byte[_random.Next(1, 10)];
            _random.NextBytes(buffer);
            var originalStream = new MemoryStream(buffer);
            originalStream.Close();

            _resultStream = StreamUtility.CopyFrom(originalStream);

            CollectionAssert.AreEqual(originalStream.ToArray(), _resultStream.ToArray());
        }
    }
}