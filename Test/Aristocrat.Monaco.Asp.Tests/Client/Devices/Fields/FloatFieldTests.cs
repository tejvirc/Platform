namespace Aristocrat.Monaco.Asp.Tests.Client.Devices.Fields
{
    using Asp.Client.Contracts;
    using Asp.Client.Devices.Fields;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FloatFieldTests : FieldTestsBase<FloatField, float>
    {
        protected override float TestValue => 5.1f;
        protected override FieldType FieldType => FieldType.FLOAT;

        protected override FloatField CreateField(IFieldPrototype prototype)
        {
            return new FloatField(prototype);
        }

        protected override void SetupWrite(Mock<IByteArrayWriter> writerMock)
        {
            writerMock.Setup(x => x.Write(TestValue));
        }

        protected override void SetupRead(Mock<IByteArrayReader> readerMock)
        {
            readerMock.Setup(x => x.ReadFloat()).Returns(TestValue);
        }

        [TestMethod]
        public override void SetValueTest()
        {
            Field.Value = float.MaxValue + 1;
            Assert.AreEqual(float.MaxValue + 1, Field.Value);
        }
    }
}