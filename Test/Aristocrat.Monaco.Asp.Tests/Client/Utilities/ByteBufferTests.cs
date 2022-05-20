namespace Aristocrat.Monaco.Asp.Tests.Client.Utilities
{
    using Aristocrat.Monaco.Asp.Client.Utilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public class ByteBufferTests
    {
        private ByteBuffer _target;
        private int _sizeOfBuffer = 100;

        [TestInitialize]
        public void Initialize()
        {
            byte[] buffer = new byte[_sizeOfBuffer];
            _target = new ByteBuffer(buffer);
        }

        /// <summary>
        /// ByteBuffer is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DataNullTest()
        {
            byte[] nullBuffer = null;
            ByteBuffer byteBuffer = new ByteBuffer(nullBuffer);
            _target.Reset(byteBuffer, 0, 0);
        }

        /// <summary>
        /// Start parameter for Reset() is set beyond buffer length
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void DataStartRangeExceptionTest()
        {
            byte[] smallBuffer = new byte[1];
            ByteBuffer byteBuffer = new ByteBuffer(smallBuffer);
            _target.Reset(byteBuffer, 2, 0);
        }

        /// <summary>
        /// End parameter for Reset() function is set to beyond buffer length
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void DataEndRangeExceptionTest()
        {
            byte[] smallBuffer = new byte[1];
            ByteBuffer byteBuffer = new ByteBuffer(smallBuffer);
            _target.Reset(byteBuffer, 0, 2);
        }
    }
}
