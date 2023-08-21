namespace Aristocrat.Monaco.Asp.Tests.Client.Devices.Fields
{
    using System;
    using Asp.Client.Contracts;
    using Asp.Client.Devices.Fields;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FieldFactoryTests
    {
        private Mock<IFieldPrototype> _fieldPrototype;

        [TestInitialize]
        public virtual void InitializeTest()
        {
            _fieldPrototype = new Mock<IFieldPrototype>(MockBehavior.Strict);
            _fieldPrototype.SetupAllProperties();
        }

        [TestMethod]
        public void CreateByteFieldTest()
        {
            _fieldPrototype.Setup(m => m.Type).Returns(FieldType.BYTE);
            var field = FieldFactory.CreateField(_fieldPrototype.Object);
            Assert.IsTrue(field is ByteField);
        }

        [TestMethod]
        public void CreateCharFieldTest()
        {
            _fieldPrototype.Setup(m => m.Type).Returns(FieldType.CHAR);
            var field = FieldFactory.CreateField(_fieldPrototype.Object);
            Assert.IsTrue(field is CharField);
        }

        [TestMethod]
        public void CreateFloatFieldTest()
        {
            _fieldPrototype.Setup(m => m.Type).Returns(FieldType.FLOAT);
            var field = FieldFactory.CreateField(_fieldPrototype.Object);
            Assert.IsTrue(field is FloatField);
        }

        [TestMethod]
        public void CreateIntOrLongFieldTest()
        {
            _fieldPrototype.Setup(m => m.Type).Returns(FieldType.INT);
            var field = FieldFactory.CreateField(_fieldPrototype.Object);
            Assert.IsTrue(field is LongField);

            _fieldPrototype.Setup(m => m.Type).Returns(FieldType.LONG);
            field = FieldFactory.CreateField(_fieldPrototype.Object);
            Assert.IsTrue(field is LongField);
        }

        [TestMethod]
        public void CreateUlongFieldTest()
        {
            _fieldPrototype.Setup(m => m.Type).Returns(FieldType.ULONG);
            var field = FieldFactory.CreateField(_fieldPrototype.Object);
            Assert.IsTrue(field is ULongField);
        }

        [TestMethod]
        public void CreateWordFieldTest()
        {
            _fieldPrototype.Setup(m => m.Type).Returns(FieldType.WORD);
            var field = FieldFactory.CreateField(_fieldPrototype.Object);
            Assert.IsTrue(field is WordField);
        }

        [TestMethod]
        public void CreateStringFieldTest()
        {
            _fieldPrototype.Setup(m => m.Type).Returns(FieldType.STRING);
            var field = FieldFactory.CreateField(_fieldPrototype.Object);
            Assert.IsTrue(field is StringField);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CreateUnknownFieldTest()
        {
            _fieldPrototype.Setup(m => m.Type).Returns(FieldType.UNKNOWN);
            var field = FieldFactory.CreateField(_fieldPrototype.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CreateDefaultFieldTest()
        {
            _fieldPrototype.Setup(m => m.Type).Returns((FieldType)10);
            var field = FieldFactory.CreateField(_fieldPrototype.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CreateBCDFieldTest()
        {
            _fieldPrototype.Setup(m => m.Type).Returns(FieldType.BCD);
            var field = FieldFactory.CreateField(_fieldPrototype.Object);
        }
    }
}