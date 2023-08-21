namespace Aristocrat.Monaco.Asp.Tests.Client.Devices.Fields
{
    using Asp.Client.Contracts;
    using Asp.Client.Devices.Fields;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ByteFieldTests : FieldTestsBase<ByteField, byte>
    {
        protected override byte TestValue => 5;
        protected override FieldType FieldType => FieldType.BYTE;

        protected override ByteField CreateField(IFieldPrototype prototype)
        {
            return new ByteField(prototype);
        }

        protected override void SetupWrite(Mock<IByteArrayWriter> writerMock)
        {
            writerMock.Setup(x => x.Write(TestValue));
        }

        protected override void SetupRead(Mock<IByteArrayReader> readerMock)
        {
            readerMock.Setup(x => x.ReadByte()).Returns(TestValue);
        }

        [TestMethod]
        public override void SetValueTest()
        {
            Field.Value = (short)55;
            Assert.AreEqual(Field.Value, (byte)55);
            Field.Value = (ushort)6;
            Assert.AreEqual(Field.Value, (byte)6);
            Field.Value = (int)255 + 1;
            Assert.AreEqual(Field.Value, (byte)0);
            Field.Value = (uint)6;
            Assert.AreEqual(Field.Value, (byte)6);
            Field.Value = (long)uint.MaxValue + 1;
            Assert.AreEqual(Field.Value, (byte)0);
            Field.Value = (ulong)6;
            Assert.AreEqual(Field.Value, (byte)6);
        }
    }
}