namespace Aristocrat.Monaco.Asp.Tests.Client.Utilities
{
    using System;
    using System.Text;
    using Aristocrat.Monaco.Asp.Client.Utilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ByteArrayWriterTests
    {
        private ByteArrayWriter _byteArrayWriter;
        private int _startOffset = 0;
        private int _bufferSize = 250;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            byte[] data = new byte[_bufferSize];
            _byteArrayWriter = new ByteArrayWriter(data, _startOffset);
        }

        /// <summary>
        ///     Test Write(string, size) to the Buffer with size equals to its length
        ///     Expected data read back OK
        /// </summary>
        [TestMethod]
        public void WriteStringWithSizeEqualsItsLength()
        {
            var stringOrigin = "Test Write() a string to Buffer with the size equals to its length";
            _byteArrayWriter.Write(stringOrigin, stringOrigin.Length);

            char[] dataReadBack = new char[stringOrigin.Length - _startOffset];
            Array.Copy(_byteArrayWriter.Buffer, _startOffset, dataReadBack, 0, dataReadBack.Length);
            var value = String.Join("", dataReadBack);
            Assert.AreEqual(stringOrigin, value);
        }

        /// <summary>
        ///     Check the size of the Property Length is the same as the Initial BufferSize
        /// </summary>
        [TestMethod]
        public void LengthPropertyTest()
        {
            var stringOrigin = "Test Write";
            _byteArrayWriter.Write(stringOrigin, stringOrigin.Length);
            Assert.AreEqual(_byteArrayWriter.Length, _bufferSize);
        }

        /// <summary>
        ///     Check the ToArray() function is returning a proper slice of the buffer
        /// </summary>
        [TestMethod]
        public void ToArraySlicingTest()
        {
            byte[] buffer = new byte[10];
            var newByteArrayWriter = new ByteArrayWriter(buffer, 5);
            var stringOrigin = "Test";
            newByteArrayWriter.Write(stringOrigin, stringOrigin.Length);
            var slicedArray = newByteArrayWriter.ToArray();
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("Test"), slicedArray);
            Assert.IsTrue(slicedArray.Length == stringOrigin.Length);
        }

        /// <summary>
        ///     Try to write data that is not the same size 
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void BufferNotLargeEnoguhForWrite()
        {
            byte[] buffer = new byte[1];
            var newByteArrayWriter = new ByteArrayWriter(buffer);
            var stringOrigin = "Test Write()";
            newByteArrayWriter.Write(stringOrigin, stringOrigin.Length);
        }

        /// <summary>
        ///     Test Write(string, size) to the Buffer with size greater than its length
        ///     Expected data read back OK
        /// </summary>
        [TestMethod]
        public void WriteStringWithSizeGreaterThanItsLength()
        {
            var stringOrigin = "Test Write() a string to Buffer with the size equals to its length + 10";
            _byteArrayWriter.Write(stringOrigin, stringOrigin.Length + 10);

            char[] dataReadBack = new char[stringOrigin.Length - _startOffset];
            Array.Copy(_byteArrayWriter.Buffer, _startOffset, dataReadBack, 0, dataReadBack.Length);
            var value = String.Join("", dataReadBack);
            Assert.AreEqual(stringOrigin, value);
        }

        /// <summary>
        ///     Test Write(string, size) to the Buffer with size lesser than its length
        ///     Expected data read back NOT OK
        /// </summary>
        [TestMethod]
        public void WriteStringWithSizeLesserThanItsLength()
        {
            var stringOrigin = "Test Write() a string to Buffer with the size equals to its length - 1";
            _byteArrayWriter.Write(stringOrigin, stringOrigin.Length - 1);

            char[] dataReadBack = new char[stringOrigin.Length - _startOffset];
            Array.Copy(_byteArrayWriter.Buffer, _startOffset, dataReadBack, 0, dataReadBack.Length);
            var value = String.Join("", dataReadBack);
            Assert.AreNotEqual(stringOrigin, value);
        }
    }
}
