namespace Aristocrat.Monaco.Asp.Tests.Client.Devices.Fields
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Asp.Client.DataSources;
    using Aristocrat.Monaco.Hardware.Contracts.Door;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Asp.Client.Contracts;
    using Asp.Client.Devices.Fields;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using FieldType = Asp.Client.Contracts.FieldType;

    public abstract class FieldTestsBase<TFieldType, TStoreType> where TFieldType : Field
    {
        protected TFieldType Field { get; private set; }
        protected Mock<IFieldPrototype> FieldPrototype { get; private set; }
        protected virtual int SizeInBytes => 1;
        protected abstract TStoreType TestValue { get; }
        protected abstract FieldType FieldType { get; }
        protected abstract TFieldType CreateField(IFieldPrototype prototype);
        protected abstract void SetupWrite(Mock<IByteArrayWriter> writerMock);
        protected abstract void SetupRead(Mock<IByteArrayReader> readerMock);
        public abstract void SetValueTest();

        [TestInitialize]
        public virtual void InitializeTest()
        {
            FieldPrototype = new Mock<IFieldPrototype>(MockBehavior.Strict);

            FieldPrototype.Setup(m => m.Type).Returns(FieldType);
            FieldPrototype.Setup(m => m.DefaultValue).Returns(It.IsAny<string>());
            FieldPrototype.Setup(m => m.SizeInBytes).Returns(SizeInBytes);
            FieldPrototype.SetupGet(x => x.Masks).Returns(new List<IMask>());
            Field = CreateField(FieldPrototype.Object);
        }

        [TestMethod]
        public void FieldTest()
        {
            Assert.IsNotNull(Field);
        }

        private void SetupDataSource()
        {
            FieldPrototype.SetupAllProperties();
            Field.DataSource = new DummyDataSource();
        }

        [TestMethod]
        public void ReadBytesTest()
        {
            var mockReader = new Mock<IByteArrayReader>(MockBehavior.Strict);
            SetupRead(mockReader);
            Field.ReadBytes(mockReader.Object);
            Assert.AreEqual(TestValue, Field.Value);
        }

        [TestMethod]
        public void ReadBytesNullTest()
        {
            Field.Value = TestValue;
            Field.ReadBytes(null);
            Assert.AreEqual(TestValue, Field.Value);
        }

        [TestMethod]
        public void WriteBytesTest()
        {
            Field.Value = TestValue;
            var mockWriter = new Mock<IByteArrayWriter>(MockBehavior.Strict);
            SetupWrite(mockWriter);
            Field.WriteBytes(mockWriter.Object);
        }

        [TestMethod]
        public void WriteBytesNullTest()
        {
            Field.Value = TestValue;
            var mockWriter = new Mock<IByteArrayWriter>(MockBehavior.Strict);
            Field.WriteBytes(null);
        }

        [TestMethod]
        public void DataSourceSetPropertyTest()
        {
            SetupDataSource();
            Assert.AreEqual(Field.DataSource.Name, "DummyDataSource");
            var doorService = new Mock<IDoorService>();
            var meterManager = new Mock<IMeterManager>();
            var persistenceStorageManager = new Mock<IPersistentStorageManager>();
            var newDataSource = new DoorsDataSource(doorService.Object, meterManager.Object, persistenceStorageManager.Object);

            Field.DataSource = newDataSource;
            Assert.AreEqual(Field.DataSource.Name, "Doors");
        }

    }
}