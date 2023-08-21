namespace Aristocrat.Monaco.Asp.Tests.Client.Devices.Fields
{
    using Asp.Client.Contracts;
    using Asp.Client.Devices.Fields;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class StringFieldTests : FieldTestsBase<StringField, string>
    {
        protected override string TestValue => "Test";
        protected override FieldType FieldType => FieldType.STRING;
        protected override int SizeInBytes => 5;

        protected override StringField CreateField(IFieldPrototype prototype)
        {
            return new StringField(prototype);
        }

        protected override void SetupWrite(Mock<IByteArrayWriter> writerMock)
        {
            writerMock.Setup(x => x.Write(TestValue, SizeInBytes));
        }

        protected override void SetupRead(Mock<IByteArrayReader> readerMock)
        {
            readerMock.Setup(x => x.ReadString(SizeInBytes)).Returns(TestValue);
        }

        [TestMethod]
        public override void SetValueTest()
        {
            Field.Value = $"TestString";
            Assert.AreEqual(Field.Value, $"TestString");
        }
    }
}