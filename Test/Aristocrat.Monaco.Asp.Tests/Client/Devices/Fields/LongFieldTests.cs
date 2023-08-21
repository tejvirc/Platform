namespace Aristocrat.Monaco.Asp.Tests.Client.Devices.Fields
{
    using Asp.Client.Contracts;
    using Asp.Client.Devices.Fields;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LongFieldTests : FieldTestsBase<LongField, int>
    {
        protected override int TestValue => 5;
        protected override FieldType FieldType => FieldType.LONG;

        protected override LongField CreateField(IFieldPrototype prototype)
        {
            return new LongField(prototype);
        }

        protected override void SetupWrite(Mock<IByteArrayWriter> writerMock)
        {
            writerMock.Setup(x => x.Write(TestValue));
        }

        protected override void SetupRead(Mock<IByteArrayReader> readerMock)
        {
            readerMock.Setup(x => x.ReadInt32()).Returns(TestValue);
        }

        [TestMethod]
        public override void SetValueTest()
        {
            Field.Value = (long)uint.MaxValue + 1;
            Assert.AreEqual(0, Field.Value);
            Field.Value = (ulong)uint.MaxValue + 1;
            Assert.AreEqual(0, Field.Value);
        }
    }
}