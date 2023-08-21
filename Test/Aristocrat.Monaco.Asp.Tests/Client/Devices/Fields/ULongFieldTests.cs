namespace Aristocrat.Monaco.Asp.Tests.Client.Devices.Fields
{
    using Asp.Client.Contracts;
    using Asp.Client.Devices.Fields;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ULongFieldTests : FieldTestsBase<ULongField, uint>
    {
        protected override uint TestValue => 5;
        protected override FieldType FieldType => FieldType.ULONG;

        protected override ULongField CreateField(IFieldPrototype prototype)
        {
            return new ULongField(prototype);
        }

        protected override void SetupWrite(Mock<IByteArrayWriter> writerMock)
        {
            writerMock.Setup(x => x.Write(TestValue));
        }

        protected override void SetupRead(Mock<IByteArrayReader> readerMock)
        {
            readerMock.Setup(x => x.ReadUInt32()).Returns(TestValue);
        }

        [TestMethod]
        public override void SetValueTest()
        {
            Field.Value = (long)uint.MaxValue + 1;
            Assert.AreEqual(Field.Value, (uint)0);
            Field.Value = (ulong)uint.MaxValue + 1;
            Assert.AreEqual(Field.Value, (uint)0);
        }
    }
}