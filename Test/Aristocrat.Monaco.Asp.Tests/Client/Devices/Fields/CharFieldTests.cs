namespace Aristocrat.Monaco.Asp.Tests.Client.Devices.Fields
{
    using System;
    using Asp.Client.Contracts;
    using Asp.Client.Devices.Fields;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CharFieldTests : FieldTestsBase<CharField, char>
    {
        protected override char TestValue => '5';
        protected override FieldType FieldType => FieldType.CHAR;

        protected override CharField CreateField(IFieldPrototype prototype)
        {
            return new CharField(prototype);
        }

        protected override void SetupWrite(Mock<IByteArrayWriter> writerMock)
        {
            writerMock.Setup(x => x.Write((byte)TestValue));
        }

        protected override void SetupRead(Mock<IByteArrayReader> readerMock)
        {
            readerMock.Setup(x => x.ReadByte()).Returns(Convert.ToByte(TestValue));
        }

        [TestMethod]
        public override void SetValueTest()
        {
            Field.Value = 55;
            Assert.AreEqual((char)55, Field.Value);
        }
    }
}