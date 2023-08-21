namespace Aristocrat.Monaco.Asp.Tests.Client.Utilities
{
    using Aristocrat.Monaco.Asp.Client.Utilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public class ByteArrayReaderTests
    {
        private ByteArrayWriter _writer;
        private ByteArrayReader _target;
        private readonly int lengthOfData = 250;
        private byte[] _data;
            
        [TestInitialize]
        public void TestInitialize()
        {
            _data = new byte[lengthOfData];
            _target = new ByteArrayReader(_data);
            _writer = new ByteArrayWriter(_data);
        }

        private void WriteByte(byte b)
        {
            _writer.Write(b);
        }

        [DataRow((byte)10,DisplayName = "byte 1")]
        [DataRow((byte)255, DisplayName = "byte 2")]
        [DataRow((byte)0, DisplayName = "byte 3")]
        [DataRow((byte)67, DisplayName = "byte 4")]
        [DataRow((byte)42, DisplayName = "byte 5")]
        [DataRow((byte)9, DisplayName = "byte 6")]
        [DataTestMethod]
        public void ReadByte(byte b)
        {
            WriteByte(b);
            var result = _target.ReadByte();
            Assert.AreEqual(b, result);
        }

        private void WriteBytes(byte[] b)
        {
            _writer.Write(b);
        }

        [DataRow(new[] { (byte)10, (byte) 20 }, DisplayName = "bytes 1")]
        [DataRow(new[] { (byte)10, (byte)255 }, DisplayName = "bytes 2")]
        [DataRow(new[] { (byte)1, (byte)0 }, DisplayName = "bytes 3")]
        [DataRow(new[] { (byte)49, (byte)220 }, DisplayName = "bytes 4")]
        [DataRow(new[] { (byte)10, (byte)20 }, DisplayName = "bytes 5")]
        [DataRow(new[] { (byte)5, (byte)5 }, DisplayName = "bytes 6")]
        [DataTestMethod]
        public void ReadBytes(byte[] b)
        {
            WriteBytes(b);
            var results = _target.ReadBytes(2);
            CollectionAssert.AreEqual(b, results);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadBytesRaiseException()
        {
            byte[] b = new byte[2] { 1, 2 };
            WriteBytes(b);
            
            _target.ReadBytes(400); // Read too many bytes, Buffer is 250
        }

        private void WriteFloat(float f)
        {
            _writer.Write(f);
        }

        [DataRow((float)120, DisplayName = "float 1")]
        [DataRow((float) 450, DisplayName = "float 2")]
        [DataRow((float) 199, DisplayName = "float 3")]
        [DataRow((float) 0, DisplayName = "float 4")]
        [DataRow((float)3566, DisplayName = "float 5")]
        [DataTestMethod]
        public void ReadFloat(float valueToRead)
        {
            WriteFloat(valueToRead);
            float results = _target.ReadFloat();
            Assert.AreEqual(valueToRead, results);
        }

        [DataRow(float.MaxValue, DisplayName = "float 1")]
        [DataRow(float.MinValue, DisplayName = "float 2")]
        [DataRow((float) - 199, DisplayName = "float 3")]
        [DataTestMethod]
        public void ReadIncorrectFloat(float valueToRead)
        {
            WriteFloat(valueToRead);
            float results = _target.ReadFloat();
            Assert.AreNotEqual(valueToRead, results);
        }

        private void WriteInt16(short f)
        {
            _writer.Write(f);
        }

        [TestMethod]
        public void ReadInt16()
        {
            short valueToRead = 20;
            WriteInt16(valueToRead);
            short results = _target.ReadInt16();
            Assert.AreEqual(valueToRead, results);
        }

        private void WriteInt32(int f)
        {
            _writer.Write(f);
        }

        [TestMethod]
        public void ReadInt32()
        {
            int valueToRead = 14;
            WriteInt32(valueToRead);
            int results = _target.ReadInt32();
            Assert.AreEqual(results, valueToRead);
        }

        private void WriteUInt16(ushort f)
        {
            _writer.Write(f);
        }

        [TestMethod]
        public void ReadUInt16()
        {
            ushort valueToRead = 1;
            WriteUInt16(valueToRead);
            ushort results = _target.ReadUInt16();
            Assert.AreEqual(results, valueToRead);
        }

        private void WriteUInt32(uint f)
        {
            _writer.Write(f);
        }

        [TestMethod]
        public void ReadUInt32()
        {
            uint valueToRead = 14;
            WriteUInt32(valueToRead);
            uint results = _target.ReadUInt32();
            Assert.AreEqual(results, valueToRead);
        }

        private void WriteString(string f)
        {
            _writer.Write(f, f.Length);
        }

        [TestMethod]
        public void ReadString()
        {
            string valueToRead = $"I am A string!";
            WriteString(valueToRead);
            string results = _target.ReadString(valueToRead.Length);
            Assert.AreEqual(results, valueToRead);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadStringRaiseException()
        {
            string valueToRead = $"I am A string!";
            WriteString(valueToRead);
            _target.ReadString(valueToRead.Length + _target.Length); // Read too large
        }

        [TestMethod]
        public void TestByteBufferLength()
        {
            Assert.AreEqual(lengthOfData + 1, _target.Length);
        }

        [DataRow(new[] { (byte)0x12, (byte)0x11, (byte)0x12, (byte)0x11 }, 0, 4, (long)303108625, DisplayName = "Valid Byte Array, Int")]
        [DataRow(new[] { (byte)0x12, (byte)0x11, (byte)0x12, (byte)0x11, (byte)0x12, (byte)0x11 , (byte)0x12, (byte)0x11, (byte)0x11 }, 0, 8, (long)1301841631813636625, DisplayName = "Valid Byte Array, Long")]
        [DataRow(new[] { (byte)0x12, (byte)0x11 }, 0, 2, (long)4625, DisplayName = "Valid Byte Array, Short")]
        [DataTestMethod]
        public void TestReadNumeric(byte[] buffer, int offset, int size, long expectedOutput)
        {
            var result = ByteArrayReader.ReadNumeric(buffer, offset, size);
            Assert.AreEqual(expectedOutput, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestReadNumericTestException()
        {
            byte[] smallBuffer = new byte[1];
            var offset = 0;
            var largeSize = smallBuffer.Length + 8;
            ByteArrayReader.ReadNumeric(smallBuffer, offset, largeSize);
        }

        [TestMethod]
        public void TestToArray()
        {
            byte[] b = new byte[10] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 } ;
            _target.Reset(b, 9, b.Length);
            var slicedArray = _target.ToArray();
            CollectionAssert.AreEqual(slicedArray, new byte[] { 100 });
        }
    }
}
