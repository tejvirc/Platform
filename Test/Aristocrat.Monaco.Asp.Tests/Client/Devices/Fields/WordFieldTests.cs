namespace Aristocrat.Monaco.Asp.Tests.Client.Devices.Fields
{
    using Asp.Client.Contracts;
    using Asp.Client.Devices.Fields;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WordFieldTests : FieldTestsBase<WordField, ushort>
    {
        protected override ushort TestValue => 5;
        protected override FieldType FieldType => FieldType.WORD;

        protected override WordField CreateField(IFieldPrototype prototype)
        {
            return new WordField(prototype);
        }

        protected override void SetupWrite(Mock<IByteArrayWriter> writerMock)
        {
            writerMock.Setup(x => x.Write(TestValue));
        }

        protected override void SetupRead(Mock<IByteArrayReader> readerMock)
        {
            readerMock.Setup(x => x.ReadUInt16()).Returns(TestValue);
        }

        [TestMethod]
        public override void SetValueTest()
        {
            Field.Value = (int)ushort.MaxValue + 1;
            Assert.AreEqual(Field.Value, (ushort)0);
            Field.Value = (int)55;
            Assert.AreEqual(Field.Value, (ushort)55);
            Field.Value = (uint)6;
            Assert.AreEqual(Field.Value, (ushort)6);
            Field.Value = (long)uint.MaxValue + 1;
            Assert.AreEqual(Field.Value, (ushort)0);
            Field.Value = (ulong)6;
            Assert.AreEqual(Field.Value, (ushort)6);
        }
    }
}